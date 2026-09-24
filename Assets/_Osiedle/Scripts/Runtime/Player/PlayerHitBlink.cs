using Osiedle.Combat;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>GDD: po trafieniu Kuba miga przez czas nieśmiertelności (0,8 s).</summary>
    public class PlayerHitBlink : MonoBehaviour
    {
        [SerializeField] PlayerData data;
        [SerializeField] Health health;
        [Tooltip("Bryły, które migają.")]
        [SerializeField] Renderer[] renderers;

        bool hidden;

        void Awake()
        {
            if (health == null) health = GetComponent<Health>();
        }

        void OnDisable() => SetVisible(true);

        void LateUpdate()
        {
            bool blinking = health.IsInvulnerable && !health.IsDead;
            bool visible = !blinking || Mathf.FloorToInt(Time.time / data.hitBlinkInterval) % 2 == 0;
            SetVisible(visible);
        }

        void SetVisible(bool visible)
        {
            if (hidden == !visible) return;
            hidden = !visible;
            foreach (Renderer r in renderers)
                if (r != null) r.enabled = visible;
        }
    }
}
