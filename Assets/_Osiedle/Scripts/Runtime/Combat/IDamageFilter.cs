namespace Osiedle.Combat
{
    /// <summary>
    /// Komponent, który może zablokować trafienie, zanim dotrze do Health
    /// (np. nietykalność w trakcie dasha, a później tarcza Milicjanta od przodu).
    /// </summary>
    public interface IDamageFilter
    {
        bool BlocksDamage(in DamageInfo info);
    }
}
