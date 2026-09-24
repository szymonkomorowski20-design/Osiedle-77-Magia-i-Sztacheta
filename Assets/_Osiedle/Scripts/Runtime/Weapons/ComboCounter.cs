using System;

namespace Osiedle.Weapons
{
    /// <summary>
    /// Czysta logika serii ciosów (bez sceny, ma testy). Po ostatnim ciosie seria zaczyna się od nowa;
    /// jeśli od końca poprzedniego ciosu minęło więcej niż <see cref="ResetTime"/>, też.
    /// </summary>
    public class ComboCounter
    {
        int nextIndex;
        float lastStepEnd = float.NegativeInfinity;

        public int Length { get; private set; }
        public float ResetTime { get; set; }

        public ComboCounter(int length, float resetTime)
        {
            SetLength(length);
            ResetTime = resetTime;
        }

        public void SetLength(int length)
        {
            Length = Math.Max(1, length);
            if (nextIndex >= Length) nextIndex = 0;
        }

        /// <summary>Zwraca indeks ciosu do wykonania teraz (0 = pierwszy) i przesuwa serię.</summary>
        public int Next(float now)
        {
            if (now - lastStepEnd > ResetTime) nextIndex = 0;
            int current = nextIndex;
            nextIndex = (nextIndex + 1) % Length;
            return current;
        }

        /// <summary>Wołane, gdy cios się skończył (od tej chwili liczy się okno na kolejny).</summary>
        public void EndStep(float now) => lastStepEnd = now;

        public void Reset()
        {
            nextIndex = 0;
            lastStepEnd = float.NegativeInfinity;
        }
    }
}
