using System;
using System.Collections.Generic;
using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Zadaje obrażenia. Po <see cref="Activate"/> co klatkę sprawdza łuk przed sobą (zasięg + kąt)
    /// i trafia każdy Hurtbox najwyżej raz na aktywację. Kierunek łuku = transform.forward tego obiektu.
    /// </summary>
    [DefaultExecutionOrder(30)]
    public class Hitbox : MonoBehaviour
    {
        const int MaxColliders = 32;
        const float MinDirectionSqr = 0.0001f;

        [Tooltip("Właściciel ciosu: jego własne Hurtboxy są pomijane.")]
        [SerializeField] GameObject owner;
        [SerializeField] LayerMask targetMask = ~0;

        readonly Collider[] buffer = new Collider[MaxColliders];
        readonly HashSet<Hurtbox> alreadyHit = new HashSet<Hurtbox>();

        DamageInfo template;
        float range;
        float halfArc;

        public bool IsActive { get; private set; }

        /// <summary>Trafienie przyjęte przez cel. Parametry: cel, pełne dane trafienia.</summary>
        public event Action<Hurtbox, DamageInfo> HitLanded;

        void Awake()
        {
            if (owner == null) owner = transform.root.gameObject;
        }

        void OnDisable() => Deactivate();

        /// <summary>Włącza cios. Kierunek i punkt trafienia Hitbox uzupełnia sam dla każdego celu.</summary>
        public void Activate(DamageInfo damage, float reach, float arcDegrees)
        {
            template = damage;
            range = reach;
            halfArc = arcDegrees * 0.5f;
            alreadyHit.Clear();
            IsActive = true;
            Sweep();
        }

        public void Deactivate() => IsActive = false;

        void Update()
        {
            if (IsActive) Sweep();
        }

        void Sweep()
        {
            Vector3 origin = transform.position;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            int count = Physics.OverlapSphereNonAlloc(origin, range, buffer, targetMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var hurtbox = buffer[i].GetComponentInParent<Hurtbox>();
                if (hurtbox == null || hurtbox.Owner == owner || alreadyHit.Contains(hurtbox)) continue;

                Vector3 toTarget = hurtbox.Center - origin;
                toTarget.y = 0f;
                Vector3 direction = toTarget.sqrMagnitude > MinDirectionSqr ? toTarget.normalized : forward;
                if (Vector3.Angle(forward, direction) > halfArc) continue;

                DamageInfo info = template;
                info.Direction = direction;
                info.Point = buffer[i].ClosestPoint(origin);
                if (info.Source == null) info.Source = owner;

                alreadyHit.Add(hurtbox);
                if (hurtbox.ReceiveHit(info)) HitLanded?.Invoke(hurtbox, info);
            }
        }
    }
}
