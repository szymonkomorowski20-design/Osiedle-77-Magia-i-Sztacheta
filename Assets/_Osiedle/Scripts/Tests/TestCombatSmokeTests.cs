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

            var weaponPivot = melee.transform.Find("WeaponPivot");
            Assert.IsNotNull(weaponPivot, "Brak widocznej broni (WeaponPivot) w prefabie gracza.");
            Assert.IsFalse(weaponPivot.gameObject.activeSelf, "Broń powinna być schowana, gdy gracz nie bije.");

            float expected = 0f;
            bool first = true;
            foreach (var step in weapon.combo)
            {
                expected += weapon.damage * step.damageMultiplier;
                melee.RequestAttack();
                if (first)
                {
                    yield return null;
                    yield return null;
                    Assert.IsTrue(weaponPivot.gameObject.activeSelf, "Broń powinna być widoczna w trakcie ciosu.");
                    first = false;
                }
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

        [UnityTest]
        public IEnumerator SlingshotShotCostsScrapAndHitsDummy()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();

            var ranged = Object.FindAnyObjectByType<PlayerRanged>();
            Assert.IsNotNull(ranged, "Brak procy u gracza.");
            var resources = ranged.GetComponent<PlayerResources>();
            var aim = ranged.GetComponent<PlayerAim>();
            var motor = ranged.GetComponent<PlayerMotor>();

            Health target = FindDummy("Manekin_Srodek");
            Assert.IsNotNull(target, "Brak środkowego manekina.");

            // Gracz 5 m przed manekinem, celownik ustawiony ręcznie na manekina.
            aim.enabled = false;
            motor.Controller.enabled = false;
            ranged.transform.SetPositionAndRotation(target.transform.position + Vector3.back * 5f, Quaternion.identity);
            motor.Controller.enabled = true;
            typeof(PlayerAim).GetProperty(nameof(PlayerAim.AimPoint))
                .SetValue(aim, target.transform.position + Vector3.up * 0.75f);

            yield return WaitSeconds(0.2f);

            int startScrap = resources.Scrap;
            float startHealth = target.Current;
            ranged.RequestShot();
            yield return WaitSeconds(0.6f);

            Assert.AreEqual(startScrap - ranged.Weapon.scrapCost, resources.Scrap, "Strzał powinien kosztować Złom.");
            Assert.AreEqual(startHealth - ranged.Weapon.damage, target.Current, Tolerance, "Śruba powinna trafić manekina.");
            Assert.Greater(resources.Power, 0f, "Trafienie z procy powinno ładować Moc.");

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
