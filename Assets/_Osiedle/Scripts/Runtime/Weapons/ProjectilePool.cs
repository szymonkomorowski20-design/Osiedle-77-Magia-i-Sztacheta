using System;
using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Osiedle.Weapons
{
    /// <summary>
    /// Pula pocisków jednego strzelca (ObjectPool) — żadnego Instantiate/Destroy w trakcie walki.
    /// Ogłasza trafienia swoich pocisków, żeby strzelec mógł np. ładować Moc.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] Projectile prefab;
        [Tooltip("Właściciel pocisków: jego własne Hurtboxy są pomijane.")]
        [SerializeField] GameObject owner;
        [SerializeField] LayerMask hitMask = ~0;
        [Tooltip("Ile pocisków przygotować na start sceny.")]
        [Min(1)] [SerializeField] int prewarmCount = 24;

        ObjectPool<Projectile> pool;

        /// <summary>Pocisk z tej puli trafił cel. Parametry: cel, trafienie.</summary>
        public event Action<Hurtbox, DamageInfo> HitLanded;

        void Awake()
        {
            if (owner == null) owner = transform.root.gameObject;
            if (prefab == null)
            {
                OsiedleLog.Error("ProjectilePool: brak prefabu pocisku.", this);
                enabled = false;
                return;
            }

            pool = new ObjectPool<Projectile>(Create, OnGet, OnRelease, OnDestroyProjectile,
                collectionCheck: true, defaultCapacity: prewarmCount);

            var temp = new Projectile[prewarmCount];
            for (int i = 0; i < prewarmCount; i++) temp[i] = pool.Get();
            for (int i = 0; i < prewarmCount; i++) pool.Release(temp[i]);
        }

        void OnDestroy() => pool?.Clear();

        public void Fire(Vector3 origin, Vector3 flatDirection, RangedWeaponData weapon, DamageInfo damage, float shooterFeetY)
        {
            if (pool == null) return;
            pool.Get().Launch(this, owner, hitMask, origin, flatDirection, weapon, damage, shooterFeetY);
        }

        public void ReportHit(Hurtbox target, DamageInfo info) => HitLanded?.Invoke(target, info);

        public void Release(Projectile projectile) => pool.Release(projectile);

        Projectile Create()
        {
            Projectile projectile = Instantiate(prefab);
            projectile.gameObject.SetActive(false);
            return projectile;
        }

        static void OnGet(Projectile projectile) => projectile.gameObject.SetActive(true);
        static void OnRelease(Projectile projectile) => projectile.gameObject.SetActive(false);

        static void OnDestroyProjectile(Projectile projectile)
        {
            if (projectile != null) Destroy(projectile.gameObject);
        }
    }
}
