using Osiedle.Combat;
using UnityEngine;

namespace Osiedle.Weapons
{
    /// <summary>
    /// Pocisk broni dystansowej — ruchomy Hitbox: leci poziomo, odbija się od ścian (tyle razy, ile daje broń)
    /// i trafia pierwszy Hurtbox na drodze. Cele sprawdza w pionowym „słupie” pod i nad linią lotu,
    /// żeby z podwyższenia trafiać wrogów na dole. Żyje w puli <see cref="ProjectilePool"/>.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        const int MaxHits = 16;
        const float BounceOffset = 0.01f;

        readonly RaycastHit[] hits = new RaycastHit[MaxHits];

        ProjectilePool pool;
        GameObject owner;
        RangedWeaponData weapon;
        DamageInfo damage;
        LayerMask mask;
        float shooterFeetY;
        Vector3 direction;
        float travelled;
        int bouncesLeft;
        bool alive;
        TrailRenderer trail;

        void Awake() => trail = GetComponentInChildren<TrailRenderer>();

        public void Launch(ProjectilePool fromPool, GameObject shooter, LayerMask hitMask, Vector3 origin,
            Vector3 flatDirection, RangedWeaponData data, DamageInfo info, float shooterFeet)
        {
            pool = fromPool;
            owner = shooter;
            mask = hitMask;
            weapon = data;
            damage = info;
            shooterFeetY = shooterFeet;
            direction = flatDirection;
            travelled = 0f;
            bouncesLeft = data.bounces;
            alive = true;

            transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction, Vector3.up));
            if (trail != null) trail.Clear();
        }

        void Update()
        {
            if (!alive) return;
            Advance(weapon.projectileSpeed * Time.deltaTime);
        }

        void Advance(float step)
        {
            Vector3 position = transform.position;
            float radius = weapon.projectileRadius;

            // 1. Ściana na drodze (pierwsza przeszkoda, która nie jest celem).
            bool hitWall = false;
            RaycastHit wall = default;
            float blockedAt = step;
            if (Physics.SphereCast(position, radius, direction, out RaycastHit first, step, mask, QueryTriggerInteraction.Ignore)
                && first.collider.GetComponentInParent<Hurtbox>() == null)
            {
                hitWall = true;
                wall = first;
                blockedAt = first.distance;
            }

            // 2. Najbliższy cel przed ścianą.
            Vector3 top = position + Vector3.up * weapon.targetReachUp;
            Vector3 bottom = position + Vector3.down * weapon.targetReachDown;
            int count = Physics.CapsuleCastNonAlloc(bottom, top, radius, direction, hits, step, mask, QueryTriggerInteraction.Collide);

            Hurtbox target = null;
            float targetDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var hurtbox = hits[i].collider.GetComponentInParent<Hurtbox>();
                if (hurtbox == null || hurtbox.Owner == owner) continue;

                float distance = hits[i].distance;
                if (distance > blockedAt || distance >= targetDistance) continue;
                target = hurtbox;
                targetDistance = distance;
            }

            if (target != null)
            {
                Strike(target, position + direction * targetDistance);
                return;
            }

            if (hitWall)
            {
                Vector3 contact = position + direction * wall.distance;
                travelled += wall.distance;
                if (bouncesLeft > 0 && BounceMath.TryReflectFlat(direction, wall.normal, out Vector3 bounced))
                {
                    bouncesLeft--;
                    direction = bounced;
                    transform.SetPositionAndRotation(contact + wall.normal * BounceOffset,
                        Quaternion.LookRotation(direction, Vector3.up));
                    return;
                }

                Despawn();
                return;
            }

            transform.position = position + direction * step;
            travelled += step;
            if (travelled >= weapon.maxRange) Despawn();
        }

        void Strike(Hurtbox target, Vector3 point)
        {
            DamageInfo info = damage;
            info.Direction = direction;
            info.Point = point;
            info.Amount *= ElevationBonus.Multiplier(shooterFeetY, target.Owner.transform.position.y,
                weapon.elevationThreshold, weapon.elevationBonus);

            if (target.ReceiveHit(info)) pool.ReportHit(target, info);
            Despawn();
        }

        void Despawn()
        {
            if (!alive) return;
            alive = false;
            pool.Release(this);
        }
    }
}
