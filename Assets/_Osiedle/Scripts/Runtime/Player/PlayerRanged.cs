using Osiedle.Combat;
using Osiedle.Core;
using Osiedle.Weapons;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Strzał z broni dystansowej (LPM, przytrzymaj = seria). Każdy strzał kosztuje Złom — Złom zdobywasz,
    /// bijąc wręcz (GDD: „niedobór napędza walkę”). Działa w biegu; nie strzela w trakcie ciosu ani dasha.
    /// Trafienia ładują Moc.
    /// </summary>
    [DefaultExecutionOrder(6)]
    public class PlayerRanged : MonoBehaviour
    {
        const float MinDirectionSqr = 0.01f;

        [SerializeField] PlayerData playerData;
        [SerializeField] RangedWeaponData weapon;
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerAim aim;
        [SerializeField] PlayerDash dash;
        [SerializeField] PlayerMelee melee;
        [SerializeField] PlayerResources resources;
        [SerializeField] ProjectilePool projectiles;

        FireTimer timer;
        bool shotRequested;

        public RangedWeaponData Weapon => weapon;

        void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (aim == null) aim = GetComponent<PlayerAim>();
            if (dash == null) dash = GetComponent<PlayerDash>();
            if (melee == null) melee = GetComponent<PlayerMelee>();
            if (resources == null) resources = GetComponent<PlayerResources>();
            if (playerData == null || weapon == null || projectiles == null)
            {
                OsiedleLog.Error("PlayerRanged: brak PlayerData, broni albo puli pocisków.", this);
                enabled = false;
                return;
            }

            timer = new FireTimer(weapon.fireInterval);
        }

        void OnEnable()
        {
            if (input != null) input.RangedPressed += HandlePressed;
            if (projectiles != null) projectiles.HitLanded += HandleHitLanded;
        }

        void OnDisable()
        {
            if (input != null) input.RangedPressed -= HandlePressed;
            if (projectiles != null) projectiles.HitLanded -= HandleHitLanded;
        }

        /// <summary>Prośba o jeden strzał (jak krótkie kliknięcie LPM). Używają jej testy.</summary>
        public void RequestShot() => shotRequested = true;

        void HandlePressed()
        {
            if (resources != null && resources.Scrap < weapon.scrapCost) GameEvents.RaiseOutOfScrap();
        }

        void Update()
        {
            // Dane czytamy co klatkę, żeby strojenie w Play Mode działało od razu.
            timer.Interval = weapon.fireInterval;

            bool wantsToFire = shotRequested || (input != null && input.RangedHeld);
            if (!wantsToFire) return;

            bool busy = (dash != null && dash.IsBusy) || (melee != null && melee.IsAttacking);
            if (busy || !timer.CanFire(Time.time)) return;

            shotRequested = false;
            if (resources == null || !resources.TrySpendScrap(weapon.scrapCost)) return;

            timer.Fire(Time.time);
            Shoot();
        }

        void Shoot()
        {
            Vector3 origin = transform.position + Vector3.up * playerData.hipHeight;

            Vector3 direction = aim != null ? aim.AimPoint - origin : transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude < MinDirectionSqr) direction = aim != null ? aim.AimDirection : transform.forward;
            direction.Normalize();

            float spread = Random.Range(-weapon.spreadDegrees, weapon.spreadDegrees) * 0.5f;
            direction = Quaternion.AngleAxis(spread, Vector3.up) * direction;

            var damage = new DamageInfo
            {
                Amount = weapon.damage,
                Element = weapon.element,
                KnockbackForce = weapon.knockbackForce,
                HitStop = weapon.hitStop,
                Source = gameObject,
            };

            projectiles.Fire(origin, direction, weapon, damage, transform.position.y);
            GameEvents.RaiseShot(origin, direction, weapon.shotShake);
        }

        void HandleHitLanded(Hurtbox target, DamageInfo info)
        {
            if (resources != null) resources.AddPower(weapon.powerPerHit);
        }
    }
}
