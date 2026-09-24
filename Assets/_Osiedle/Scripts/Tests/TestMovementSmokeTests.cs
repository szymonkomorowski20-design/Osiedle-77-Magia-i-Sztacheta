using System.Collections;
using NUnit.Framework;
using Osiedle.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Osiedle.Tests
{
    /// <summary>
    /// Test dymny sceny Test_Movement: uruchamia ją w Play Mode i sprawdza dash oraz skok na skrzynię.
    /// Wymaga zbudowanej sceny (Osiedle/Build/Test_Movement).
    /// </summary>
    public class TestMovementSmokeTests
    {
        const string ScenePath = "Assets/_Osiedle/Scenes/Test_Movement.unity";
        const float Tolerance = 0.3f;

        [UnityTest]
        public IEnumerator DashAndVaultWorkInTestScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();

            var motor = Object.FindAnyObjectByType<PlayerMotor>();
            Assert.IsNotNull(motor, "Brak gracza w scenie.");
            var dash = motor.GetComponent<PlayerDash>();
            var aim = motor.GetComponent<PlayerAim>();

            // Celowanie ustawiamy ręcznie (w trybie wsadowym nie ma myszy).
            aim.enabled = false;
            typeof(PlayerAim).GetProperty(nameof(PlayerAim.AimDirection)).SetValue(aim, Vector3.forward);

            yield return WaitSeconds(0.5f);
            Assert.IsTrue(motor.IsGrounded, "Postać powinna stać na podłodze.");

            // 1. Dash w otwartym terenie: ~4 m do przodu i nieśmiertelność na starcie.
            Teleport(motor, new Vector3(-10f, 0f, -10f));
            yield return null;
            Vector3 start = motor.transform.position;
            dash.RequestDash();
            yield return null;
            Assert.IsTrue(dash.IsInvulnerable, "Dash powinien dawać nieśmiertelność.");
            yield return WaitSeconds(0.4f);
            float travelled = motor.transform.position.z - start.z;
            PlayerData data = ReadData(dash);
            Assert.AreEqual(data.dashDistance, travelled, Tolerance, "Dash powinien przenieść postać o dashDistance.");

            // 2. Skok kontekstowy: przed Skrzynią A (góra na wysokości 1 m) dash zamienia się w skok.
            yield return WaitSeconds(data.dashCooldown);
            Teleport(motor, new Vector3(4f, 0f, 1.5f));
            yield return null;
            dash.RequestDash();
            yield return WaitSeconds(data.vaultDuration + 0.3f);
            Assert.AreEqual(1f, motor.transform.position.y, Tolerance, "Postać powinna stać na skrzyni.");

            yield return new ExitPlayMode();
        }

        static PlayerData ReadData(PlayerDash dash)
        {
            var field = typeof(PlayerDash).GetField("data",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (PlayerData)field.GetValue(dash);
        }

        static void Teleport(PlayerMotor motor, Vector3 position)
        {
            motor.Controller.enabled = false;
            motor.transform.position = position;
            motor.Controller.enabled = true;
        }

        static IEnumerator WaitSeconds(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }
    }
}
