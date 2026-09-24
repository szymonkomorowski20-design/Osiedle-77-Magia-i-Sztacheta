using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Manekin z worka (od Zbyszka): przyjmuje ciosy, błyska, odlatuje od uderzeń, leczy się po chwili spokoju
    /// i wstaje po „śmierci”. Nie atakuje.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class TrainingDummy : MonoBehaviour
    {
        const string BaseColorProperty = "_BaseColor";

        [SerializeField] EnemyData data;
        [SerializeField] Health health;
        [SerializeField] Knockback knockback;
        [Tooltip("Bryły, które błyskają przy trafieniu.")]
        [SerializeField] Renderer[] flashRenderers;

        MaterialPropertyBlock block;
        Color[] baseColors;
        float lastHitTime = float.NegativeInfinity;
        float flashUntil;
        float diedAt;
        bool flashing;

        void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            if (knockback == null) knockback = GetComponent<Knockback>();
            if (data == null)
            {
                OsiedleLog.Error("TrainingDummy: brak EnemyData.", this);
                enabled = false;
                return;
            }

            health.Configure(data.maxHealth, data.hitInvulnerability);
            if (knockback != null) knockback.Configure(data.knockbackMultiplier, data.knockbackDeceleration);

            block = new MaterialPropertyBlock();
            baseColors = new Color[flashRenderers.Length];
            for (int i = 0; i < flashRenderers.Length; i++)
                baseColors[i] = flashRenderers[i].sharedMaterial.GetColor(BaseColorProperty);
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

        void HandleDamaged(DamageInfo info, float dealt)
        {
            lastHitTime = Time.time;
            flashUntil = Time.time + data.hitFlashDuration;
            SetFlash(true);
        }

        void HandleDied(DamageInfo info)
        {
            diedAt = Time.time;
            OsiedleLog.Info($"{name}: rozwalony! Wstanie za {data.respawnDelay} s.");
        }

        void Update()
        {
            if (flashing && Time.time >= flashUntil) SetFlash(false);

            if (health.IsDead)
            {
                if (Time.time - diedAt >= data.respawnDelay) health.Restore();
                return;
            }

            if (Time.time - lastHitTime >= data.regenDelay)
                health.Heal(data.regenPerSecond * Time.deltaTime);
        }

        void SetFlash(bool on)
        {
            flashing = on;
            for (int i = 0; i < flashRenderers.Length; i++)
            {
                flashRenderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColorProperty, on ? data.hitFlashColor : baseColors[i]);
                flashRenderers[i].SetPropertyBlock(block);
            }
        }
    }
}
