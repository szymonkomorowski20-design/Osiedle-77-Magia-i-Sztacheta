using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Celowanie myszą: promień z kursora przecina płaszczyznę na wysokości biodra postaci.
    /// Dzięki temu pociski polecą tam, gdzie gracz klika, nawet na podwyższeniu. Postać zawsze patrzy na kursor.
    /// </summary>
    [DefaultExecutionOrder(20)]
    public class PlayerAim : MonoBehaviour
    {
        const float MinDirectionSqr = 0.0001f;

        [SerializeField] PlayerData data;
        [SerializeField] PlayerInputReader input;

        Camera view;

        /// <summary>Punkt w świecie, w który celuje gracz (na wysokości biodra).</summary>
        public Vector3 AimPoint { get; private set; }

        /// <summary>Płaski, znormalizowany kierunek od postaci do celu.</summary>
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (data == null)
            {
                OsiedleLog.Error("PlayerAim: brak PlayerData.", this);
                enabled = false;
                return;
            }

            view = Camera.main;
            AimPoint = transform.position + transform.forward;
            AimDirection = transform.forward;
        }

        void Update()
        {
            if (view == null)
            {
                view = Camera.main;
                if (view == null || input == null) return;
            }

            float planeHeight = transform.position.y + data.hipHeight;
            var plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
            Ray ray = view.ScreenPointToRay(input.PointerScreenPosition);
            if (!plane.Raycast(ray, out float enter)) return;

            AimPoint = ray.GetPoint(enter);

            Vector3 flat = AimPoint - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < MinDirectionSqr) return;

            AimDirection = flat.normalized;
            transform.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
        }
    }
}
