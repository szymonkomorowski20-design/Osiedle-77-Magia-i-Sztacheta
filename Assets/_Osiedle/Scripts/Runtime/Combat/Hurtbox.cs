using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Przyjmuje trafienia z Hitboxa: pyta filtry (np. nietykalność w dashu), przekazuje obrażenia do Health,
    /// nakłada odrzut i ogłasza <see cref="GameEvents.OnHit"/>. Musi leżeć na obiekcie z colliderem.
    /// </summary>
    public class Hurtbox : MonoBehaviour
    {
        [SerializeField] Health health;
        [Tooltip("Opcjonalny odrzut. Bez niego cel nie odlatuje.")]
        [SerializeField] Knockback knockback;

        IDamageFilter[] filters;
        Collider cachedCollider;

        public Health Health => health;

        /// <summary>Obiekt-właściciel (ten, na którym jest Health). Hitbox nie trafia własnego właściciela.</summary>
        public GameObject Owner => health != null ? health.gameObject : gameObject;

        /// <summary>Środek celu (do sprawdzania kąta ciosu i miejsca trafienia).</summary>
        public Vector3 Center => cachedCollider != null ? cachedCollider.bounds.center : transform.position;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
            if (knockback == null) knockback = GetComponentInParent<Knockback>();
            cachedCollider = GetComponent<Collider>();
            filters = Owner.GetComponentsInChildren<IDamageFilter>();
        }

        /// <summary>Zwraca true, jeśli trafienie zostało przyjęte.</summary>
        public bool ReceiveHit(DamageInfo info)
        {
            if (health == null) return false;

            foreach (IDamageFilter filter in filters)
                if (filter.BlocksDamage(info)) return false;

            if (!health.TakeDamage(info)) return false;

            if (knockback != null) knockback.Apply(info.KnockbackVelocity);
            GameEvents.RaiseHit(info, this);
            return true;
        }
    }
}
