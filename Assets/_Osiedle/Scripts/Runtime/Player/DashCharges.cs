using System;

namespace Osiedle.Player
{
    /// <summary>
    /// Ładunki dasha i ich odnawianie. Czysta logika bez sceny, dzięki czemu ma testy.
    /// Ładunki odnawiają się po jednym, każdy przez czas <see cref="Cooldown"/>.
    /// </summary>
    public class DashCharges
    {
        float rechargeTimer;

        public int MaxCharges { get; private set; }
        public int Charges { get; private set; }
        public float Cooldown { get; private set; }

        /// <summary>Postęp odnawiania kolejnego ładunku, 0..1 (1 = wszystkie ładunki pełne).</summary>
        public float RechargeProgress =>
            Charges >= MaxCharges || Cooldown <= 0f ? 1f : rechargeTimer / Cooldown;

        public DashCharges(int maxCharges, float cooldown)
        {
            Configure(maxCharges, cooldown);
            Charges = MaxCharges;
        }

        /// <summary>Zmienia limity (np. po ulepszeniu albo przy strojeniu danych w trakcie gry).</summary>
        public void Configure(int maxCharges, float cooldown)
        {
            MaxCharges = Math.Max(1, maxCharges);
            Cooldown = Math.Max(0f, cooldown);
            if (Charges > MaxCharges) Charges = MaxCharges;
        }

        public bool TryConsume()
        {
            if (Charges <= 0) return false;
            Charges--;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (Charges >= MaxCharges)
            {
                rechargeTimer = 0f;
                return;
            }

            if (Cooldown <= 0f)
            {
                Charges = MaxCharges;
                rechargeTimer = 0f;
                return;
            }

            rechargeTimer += deltaTime;
            while (rechargeTimer >= Cooldown && Charges < MaxCharges)
            {
                rechargeTimer -= Cooldown;
                Charges++;
            }

            if (Charges >= MaxCharges) rechargeTimer = 0f;
        }
    }
}
