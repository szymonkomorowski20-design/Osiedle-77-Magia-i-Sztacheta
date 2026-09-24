using Osiedle.Player;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Ustawia kamerę Cinemachine z góry: stały kąt (bez obracania, bo myli celowanie),
    /// odległość i pole widzenia z <see cref="PlayerData"/>. Wartości czyta co klatkę, więc da się je stroić w Play Mode.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    [DefaultExecutionOrder(-50)]
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] PlayerData data;

        CinemachineCamera cam;
        CinemachineFollow follow;

        void Awake()
        {
            cam = GetComponent<CinemachineCamera>();
            follow = GetComponent<CinemachineFollow>();
        }

        void LateUpdate()
        {
            if (data == null) return;
            Apply(data, cam, follow);
        }

        /// <summary>Wspólne ustawienie kamery — używane też przez budowniczego sceny w edytorze.</summary>
        public static void Apply(PlayerData data, CinemachineCamera cam, CinemachineFollow follow)
        {
            Quaternion rotation = Quaternion.Euler(data.cameraPitch, 0f, 0f);
            cam.transform.rotation = rotation;
            cam.Lens.FieldOfView = data.cameraFieldOfView;

            if (follow == null) return;
            follow.FollowOffset = rotation * Vector3.back * data.cameraDistance;
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            follow.TrackerSettings.PositionDamping = Vector3.one * data.cameraDamping;
        }
    }
}
