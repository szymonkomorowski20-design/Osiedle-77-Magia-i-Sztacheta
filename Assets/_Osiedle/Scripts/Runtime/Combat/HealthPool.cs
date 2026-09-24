using System;

namespace Osiedle.Combat
{
    /// <summary>
    /// Czysta logika zdrowia (bez sceny, ma testy): obrażenia, leczenie, śmierć
    /// i krótka nieśmiertelność po każdym trafieniu.
    /// </summary>
    public class HealthPool
    {
        float invulnerableUntil = float.NegativeInfinity;

        public float Max { get; private set; }
        public float Current { get; private set; }
        public float InvulnerabilityAfterHit { get; private set; }

        public bool IsDead => Current <= 0f;
        public float Normalized => Max > 0f ? Current / Max : 0f;

        public HealthPool(float max, float invulnerabilityAfterHit)
        {
            Configure(max, invulnerabilityAfterHit);
            Current = Max;
        }

        public void Configure(float max, float invulnerabilityAfterHit)
        {
            Max = Math.Max(1f, max);
            InvulnerabilityAfterHit = Math.Max(0f, invulnerabilityAfterHit);
            if (Current > Max) Current = Max;
        }

        public bool IsInvulnerable(float now) => now < invulnerableUntil;

        /// <summary>
        /// Próbuje zadać obrażenia. Zwraca false, gdy trafienie się nie liczy (martwy, nietykalny, 0 obrażeń).
        /// </summary>
        public bool TryTakeDamage(float amount, float now, out float dealt)
        {
            dealt = 0f;
            if (IsDead || amount <= 0f || IsInvulnerable(now)) return false;

            dealt = Math.Min(amount, Current);
            Current -= dealt;
            invulnerableUntil = now + InvulnerabilityAfterHit;
            return true;
        }

        /// <summary>Leczy (nie wskrzesza). Zwraca, ile faktycznie dodano.</summary>
        public float Heal(float amount)
        {
            if (IsDead || amount <= 0f) return 0f;
            float healed = Math.Min(amount, Max - Current);
            Current += healed;
            return healed;
        }

        /// <summary>Przywraca pełne zdrowie, także po śmierci.</summary>
        public void Restore()
        {
            Current = Max;
            invulnerableUntil = float.NegativeInfinity;
        }
    }
}
