using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Manekin z worka (od Zbyszka): przyjmuje ciosy, błyska (HitFlash), odlatuje od uderzeń,
    /// leczy się po chwili spokoju i wstaje po „śmierci”. Nie atakuje.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] EnemyData data;
        [SerializeField] Health health;
        [SerializeField] Knockback knockback;
        [SerializeField] HitFlash flash;

        float lastHitTime = float.NegativeInfinity;
        float diedAt;

        void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            if (knockback == null) knockback = GetComponent<Knockback>();
            if (flash == null) flash = GetComponent<HitFlash>();
            if (data == null)
            {
                OsiedleLog.Error("TrainingDummy: brak EnemyData.", this);
                enabled = false;
                return;
            }

            health.Configure(data.maxHealth, data.hitInvulnerability);
            if (knockback != null) knockback.Configure(data.knockbackMultiplier, data.knockbackDeceleration);
            if (flash != null) flash.Configure(data.hitFlashDuration, data.hitFlashColor);
        }

        void OnEnable()
        {
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }

        void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }

        void HandleDamaged(DamageInfo info, float dealt) => lastHitTime = Time.time;

        void HandleDied(DamageInfo info)
        {
            diedAt = Time.time;
            OsiedleLog.Info($"{name}: rozwalony! Wstanie za {data.respawnDelay} s.");
        }

        void Update()
        {
            if (health.IsDead)
            {
                if (Time.time - diedAt >= data.respawnDelay) health.Restore();
                return;
            }

            if (Time.time - lastHitTime >= data.regenDelay)
                health.Heal(data.regenPerSecond * Time.deltaTime);
        }
    }
}
