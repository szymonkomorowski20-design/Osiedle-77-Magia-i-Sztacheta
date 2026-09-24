using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Śmierć Kuby: wyłącza sterowanie (ruch, dash, ciosy, strzał, celowanie) i kładzie postać.
    /// W fabule to ucieczka z powrotem na trzepak — restart klawiszem R (QuickRestart).
    /// </summary>
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] Health health;
        [Tooltip("Komponenty wyłączane po śmierci.")]
        [SerializeField] Behaviour[] disableOnDeath;
        [Tooltip("Wygląd postaci, który się przewraca.")]
        [SerializeField] Transform visualRoot;

        public bool IsDead { get; private set; }

        void Awake()
        {
            if (health == null) health = GetComponent<Health>();
        }

        void OnEnable() => health.Died += HandleDied;
        void OnDisable() => health.Died -= HandleDied;

        void HandleDied(DamageInfo info)
        {
            IsDead = true;
            foreach (Behaviour behaviour in disableOnDeath)
                if (behaviour != null) behaviour.enabled = false;

            if (visualRoot != null)
            {
                // Pada na plecy, w stronę, w którą odepchnął go cios.
                Vector3 fall = info.Direction.sqrMagnitude > 0f ? info.Direction : -transform.forward;
                visualRoot.rotation = Quaternion.LookRotation(Vector3.up, new Vector3(fall.x, 0f, fall.z).normalized);
            }

            OsiedleLog.Info("Kuba wraca na trzepak. R — jeszcze raz.");
        }
    }
}
