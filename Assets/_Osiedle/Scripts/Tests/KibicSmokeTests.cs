using System.Collections;
using NUnit.Framework;
using Osiedle.Combat;
using Osiedle.Core;
using Osiedle.Enemies;
using Osiedle.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Osiedle.Tests
{
    /// <summary>
    /// Test dymny pierwszej części „Podwórka” w scenie Test_Combat: szarża Kibica z telegrafem rani Kubę,
    /// a śmierć i restart działają. Wymaga zbudowanej sceny (Osiedle/Build/Test_Combat).
    /// </summary>
    public class KibicSmokeTests
    {
        const string ScenePath = "Assets/_Osiedle/Scenes/Test_Combat.unity";
        const float Tolerance = 0.5f;

        [UnityTest]
        public IEnumerator ChargeWithTelegraphHurtsStandingPlayer()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();

            var kibic = Object.FindAnyObjectByType<ChargerBrain>();
            var player = Object.FindAnyObjectByType<PlayerResources>();
            Assert.IsNotNull(kibic, "Brak Kibica w scenie.");

            // Kuba stoi 5 m przed Kibicem, na jego osi.
            var motor = player.GetComponent<PlayerMotor>();
            motor.Controller.enabled = false;
            player.transform.position = kibic.transform.position + Vector3.back * 5f;
            motor.Controller.enabled = true;

            float startHealth = player.Health.Current;
            var telegraph = kibic.transform.Find("Telegraf").gameObject;

            yield return WaitUntil(() => kibic.CurrentState == ChargerBrain.State.Windup, 2f);
            Assert.AreEqual(ChargerBrain.State.Windup, kibic.CurrentState, "Kibic powinien zacząć zamach.");
            yield return null;
            Assert.IsTrue(telegraph.activeSelf, "Zamach musi mieć widoczny telegraf.");

            yield return WaitUntil(() => player.Health.Current < startHealth, 2f);
            float damage = startHealth - player.Health.Current;
            Assert.AreEqual(20f, damage, Tolerance, "Szarża powinna zadać Kubie obrażenia z ChargerData.");
            Assert.IsFalse(telegraph.activeSelf, "W trakcie szarży telegraf znika.");

            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator DeathDisablesControlsAndRestartReloads()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();

            var player = Object.FindAnyObjectByType<PlayerResources>();
            var hurtbox = player.GetComponent<Hurtbox>();
            hurtbox.ReceiveHit(new DamageInfo { Amount = 1000f, Direction = Vector3.forward });
            yield return null;

            var death = player.GetComponent<PlayerDeath>();
            Assert.IsTrue(death.IsDead, "Kuba powinien zginąć.");
            Assert.IsFalse(player.GetComponent<PlayerMelee>().enabled, "Po śmierci nie da się bić.");
            Assert.IsFalse(player.GetComponent<PlayerMotor>().enabled, "Po śmierci nie da się chodzić.");

            player.GetComponent<QuickRestart>().Restart();
            yield return null;
            yield return null;

            var fresh = Object.FindAnyObjectByType<PlayerResources>();
            Assert.IsNotNull(fresh);
            Assert.AreNotSame(player, fresh, "Restart powinien wczytać scenę od nowa.");
            Assert.IsFalse(fresh.GetComponent<PlayerDeath>().IsDead);
            Assert.AreEqual(fresh.Health.Max, fresh.Health.Current);

            yield return new ExitPlayMode();
        }

        static IEnumerator WaitUntil(System.Func<bool> condition, float timeout)
        {
            float end = Time.unscaledTime + timeout;
            while (!condition() && Time.unscaledTime < end) yield return null;
        }
    }
}
