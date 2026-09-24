using System;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Zdrowie postaci lub wroga. Liczby (maks. HP, nieśmiertelność po trafieniu) ustawia właściciel
    /// ze swoich danych przez <see cref="Configure"/>. Obrażenia przychodzą tylko przez Hurtbox.
    /// </summary>
    public class Health : MonoBehaviour
    {
        HealthPool pool;

        /// <summary>Trafienie się liczyło. Parametry: trafienie, faktycznie zadane obrażenia.</summary>
        public event Action<DamageInfo, float> Damaged;
        public event Action<DamageInfo> Died;
        public event Action Restored;

        public bool IsConfigured => pool != null;
        public float Current => pool?.Current ?? 0f;
        public float Max => pool?.Max ?? 0f;
        public float Normalized => pool?.Normalized ?? 0f;
        public bool IsDead => pool == null || pool.IsDead;

        /// <summary>Czy trwa nieśmiertelność po trafieniu (np. żeby postać migała).</summary>
        public bool IsInvulnerable => pool != null && pool.IsInvulnerable(Time.time);

        public void Configure(float max, float invulnerabilityAfterHit)
        {
            if (pool == null) pool = new HealthPool(max, invulnerabilityAfterHit);
            else pool.Configure(max, invulnerabilityAfterHit);
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (pool == null)
            {
                OsiedleLog.Warn($"{name}: Health bez konfiguracji — trafienie pominięte.", this);
                return false;
            }

            if (!pool.TryTakeDamage(info.Amount, Time.time, out float dealt)) return false;

            Damaged?.Invoke(info, dealt);
            if (pool.IsDead)
            {
                Died?.Invoke(info);
                GameEvents.RaiseKill(info, this);
            }
            return true;
        }

        public float Heal(float amount) => pool?.Heal(amount) ?? 0f;

        public void Restore()
        {
            if (pool == null) return;
            pool.Restore();
            Restored?.Invoke();
        }
    }
}
