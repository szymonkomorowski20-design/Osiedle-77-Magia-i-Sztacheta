namespace Osiedle.Weapons
{
    /// <summary>Premia do obrażeń za strzał z podwyższenia (GDD: z góry proca +20%). Czysta logika, ma testy.</summary>
    public static class ElevationBonus
    {
        /// <summary>Mnożnik obrażeń: 1 + premia, gdy strzelec stoi co najmniej <paramref name="threshold"/> m wyżej niż cel.</summary>
        public static float Multiplier(float shooterFeetY, float targetFeetY, float threshold, float bonus)
        {
            return shooterFeetY - targetFeetY >= threshold ? 1f + bonus : 1f;
        }
    }
}
