using Unity.Cinemachine;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Wstrząs ekranu (Cinemachine Impulse). Podpina się pod zdarzenia gry — na razie strzał (mały wstrząs).
    /// Kamera musi mieć CinemachineImpulseListener. Później dojdzie suwak siły wstrząsu w opcjach.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraShake : MonoBehaviour
    {
        CinemachineImpulseSource source;

        void Awake() => source = GetComponent<CinemachineImpulseSource>();

        void OnEnable() => GameEvents.OnShot += HandleShot;
        void OnDisable() => GameEvents.OnShot -= HandleShot;

        void HandleShot(Vector3 origin, Vector3 direction, float shake) => Shake(shake);

        public void Shake(float force)
        {
            if (force > 0f) source.GenerateImpulseWithForce(force);
        }
    }
}
