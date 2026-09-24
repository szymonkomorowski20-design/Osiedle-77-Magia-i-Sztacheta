using Osiedle.Player;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Punkt, za którym jedzie kamera. Stoi przy postaci, ale wyprzedza ją w stronę kursora
    /// (maks. <see cref="PlayerData.lookAheadMax"/>), żeby gracz widział więcej tam, gdzie celuje.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class CameraLookAhead : MonoBehaviour
    {
        [SerializeField] PlayerData data;
        [SerializeField] PlayerAim aim;

        Vector3 offset;
        Vector3 offsetVelocity;

        void LateUpdate()
        {
            if (aim == null || data == null) return;

            Vector3 player = aim.transform.position;
            Vector3 toAim = aim.AimPoint - player;
            toAim.y = 0f;

            Vector3 target = Vector3.ClampMagnitude(toAim * data.lookAheadFactor, data.lookAheadMax);
            offset = Vector3.SmoothDamp(offset, target, ref offsetVelocity, data.lookAheadSmoothTime);
            transform.position = player + offset;
        }
    }
}
