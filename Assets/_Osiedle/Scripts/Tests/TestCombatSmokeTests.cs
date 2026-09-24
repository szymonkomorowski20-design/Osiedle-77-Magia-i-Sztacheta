using System.Collections;
using NUnit.Framework;
using Osiedle.Combat;
using Osiedle.Enemies;
using Osiedle.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Osiedle.Tests
{
    /// <summary>
    /// Test dymny sceny Test_Combat: seria 3 ciosów w manekina daje obrażenia, odrzut, Złom i Moc,
    /// a hit-stop zawsze przywraca normalny czas. Wymaga zbudowanej sceny (Osiedle/Build/Test_Combat).
    /// </summary>
    public class TestCombatSmokeTests
    {
        const string ScenePath = "Assets/_Osiedle/Scenes/Test_Combat.unity";
        const float Tolerance = 0.5f;

        [UnityTest]
        public IEnumerator ThreeHitComboDamagesDummyAndDropsScrap()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();

            var melee = Object.FindAnyObjectByType<PlayerMelee>();
            Assert.IsNotNull(melee, "Brak gracza w scenie.");
            var resources = melee.GetComponent<PlayerResources>();
            var aim = melee.GetComponent<PlayerAim>();

            Health target = FindDummy("Manekin_Srodek");
            Assert.IsNotNull(target, "Brak środkowego manekina.");

            // Stawiamy gracza przed manekinem i ręcznie kierujemy go na cel (w trybie wsadowym nie ma myszy).
            aim.enabled = false;
            var motor = melee.GetComponent<PlayerMotor>();
            motor.Controller.enabled = false;
            melee.transform.SetPositionAndRotation(target.transform.position + Vector3.back * 1.4f, Quaternion.identity);
            motor.Controller.enabled = true;

            yield return WaitSeconds(0.2f);

            float startHealth = target.Current;
            int startScrap = resources.Scrap;
            Vector3 startPosition = target.transform.position;
            var weapon = melee.Weapon;

            float expected = 0f;
            foreach (var step in weapon.combo)
            {
                expected += weapon.damage * step.damageMultiplier;
                melee.RequestAttack();
                // Czekamy na koniec ciosu (plus zapas na hit-stop).
                yield return WaitSeconds(step.Duration + 0.1f);
            }

            Assert.AreEqual(startHealth - expected, target.Current, Tolerance, "Manekin powinien dostać pełną serię.");
            Assert.Greater(target.transform.position.z - startPosition.z, 1f, "Odrzut powinien odepchnąć manekina.");
            Assert.Greater(resources.Power, 0f, "Trafienia powinny ładować Moc.");

            yield return WaitSeconds(1.5f);
            Assert.Greater(resources.Scrap, startScrap, "Śrubki powinny dolecieć do gracza.");
            Assert.AreEqual(1f, Time.timeScale, "Hit-stop musi przywrócić normalny czas.");

            yield return new ExitPlayMode();
        }

        static Health FindDummy(string name)
        {
            foreach (var dummy in Object.FindObjectsByType<TrainingDummy>())
                if (dummy.name == name) return dummy.GetComponent<Health>();
            return null;
        }

        static IEnumerator WaitSeconds(float seconds)
        {
            float end = Time.unscaledTime + seconds;
            while (Time.unscaledTime < end) yield return null;
        }
    }
}
