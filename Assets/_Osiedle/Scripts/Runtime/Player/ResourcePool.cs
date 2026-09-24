using System;

namespace Osiedle.Player
{
    /// <summary>
    /// Czysta logika zasobu z limitem (Złom, Moc). Bez sceny, ma testy.
    /// </summary>
    public class ResourcePool
    {
        public float Max { get; private set; }
        public float Current { get; private set; }

        public bool IsFull => Current >= Max;
        public float Normalized => Max > 0f ? Current / Max : 0f;

        public ResourcePool(float max, float start)
        {
            SetMax(max);
            Current = Math.Max(0f, Math.Min(start, Max));
        }

        /// <summary>Zmienia limit; nadmiar przepada.</summary>
        public void SetMax(float max)
        {
            Max = Math.Max(0f, max);
            if (Current > Max) Current = Max;
        }

        /// <summary>Dodaje do limitu. Zwraca, ile faktycznie dodano.</summary>
        public float Add(float amount)
        {
            if (amount <= 0f) return 0f;
            float added = Math.Min(amount, Max - Current);
            Current += added;
            return added;
        }

        /// <summary>Wydaje, jeśli wystarczy. Inaczej nic nie zabiera i zwraca false.</summary>
        public bool TrySpend(float amount)
        {
            if (amount < 0f || Current < amount) return false;
            Current -= amount;
            return true;
        }

        public void Clear() => Current = 0f;
    }
}
