using Osiedle.Combat;
using Osiedle.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Wstrząs ekranu (Cinemachine Impulse): mały przy strzale, większy, gdy Kuba dostaje.
    /// Kamera musi mieć CinemachineImpulseListener. Później dojdzie suwak siły wstrząsu w opcjach.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] PlayerData data;

        CinemachineImpulseSource source;

        void Awake() => source = GetComponent<CinemachineImpulseSource>();

        void OnEnable()
        {
            GameEvents.OnShot += HandleShot;
            GameEvents.OnDamageTaken += HandleDamageTaken;
        }

        void OnDisable()
        {
            GameEvents.OnShot -= HandleShot;
            GameEvents.OnDamageTaken -= HandleDamageTaken;
        }

        void HandleShot(Vector3 origin, Vector3 direction, float shake) => Shake(shake);

        void HandleDamageTaken(DamageInfo info, float dealt)
        {
            if (data != null) Shake(data.damageShake);
        }

        public void Shake(float force)
        {
            if (force > 0f) source.GenerateImpulseWithForce(force);
        }
    }
}
