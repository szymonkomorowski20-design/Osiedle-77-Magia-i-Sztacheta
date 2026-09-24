using System;

namespace Osiedle.Combat
{
    /// <summary>Losowanie liczby śrubek wybitych trafieniem. Czysta logika, ma testy.</summary>
    public static class ScrapRoll
    {
        /// <summary>Losuje liczbę z przedziału [min, max] włącznie. Odwrócony przedział jest poprawiany, ujemne = 0.</summary>
        public static int Count(int min, int max, Random rng)
        {
            min = Math.Max(0, min);
            max = Math.Max(0, max);
            if (min > max) (min, max) = (max, min);
            return rng.Next(min, max + 1);
        }
    }
}
