using Osiedle.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Osiedle.Core
{
    /// <summary>Szybki restart walki klawiszem R: wczytuje bieżącą scenę od nowa (działa zawsze, także po śmierci).</summary>
    public class QuickRestart : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;

        void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
        }

        void OnEnable()
        {
            if (input != null) input.RestartPressed += Restart;
        }

        void OnDisable()
        {
            if (input != null) input.RestartPressed -= Restart;
        }

        public void Restart()
        {
            // Hit-stop mógł zostawić zatrzymany czas.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
