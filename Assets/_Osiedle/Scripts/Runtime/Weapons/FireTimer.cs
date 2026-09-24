namespace Osiedle.Weapons
{
    /// <summary>
    /// Szybkostrzelność: pilnuje odstępu między strzałami. Przy ciągłym ogniu trzyma równy rytm
    /// (bez „gubienia” czasu między klatkami), po przerwie liczy od nowa. Czysta logika, ma testy.
    /// </summary>
    public class FireTimer
    {
        float nextShot = float.NegativeInfinity;

        public float Interval { get; set; }

        public FireTimer(float interval)
        {
            Interval = interval;
        }

        public bool CanFire(float now) => now >= nextShot;

        /// <summary>Zapisuje strzał. Wołać tylko, gdy <see cref="CanFire"/> zwróciło true.</summary>
        public void Fire(float now)
        {
            bool onRhythm = now - nextShot < Interval;
            nextShot = onRhythm ? nextShot + Interval : now + Interval;
        }

        public void Reset() => nextShot = float.NegativeInfinity;
    }
}
