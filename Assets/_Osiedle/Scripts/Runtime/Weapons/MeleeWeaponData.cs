using Osiedle.Combat;
using UnityEngine;

namespace Osiedle.Weapons
{
    /// <summary>
    /// Dane broni białej. Domyślne wartości to Sztacheta z płotu z GDD: 30 obrażeń, wolna,
    /// seria 3 ciosów, trzeci odrzuca najmocniej.
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeWeapon", menuName = "Osiedle/Dane/Broń biała")]
    public class MeleeWeaponData : ScriptableObject
    {
        [Header("Obrażenia")]
        [Tooltip("Obrażenia bazowe jednego ciosu.")]
        [Min(0f)] public float damage = 30f;
        public Element element = Element.Physical;

        [Header("Zasięg")]
        [Tooltip("Zasięg ciosu od biodra postaci (m).")]
        [Min(0.1f)] public float range = 2f;

        [Tooltip("Szerokość łuku ciosu (stopnie).")]
        [Range(10f, 360f)] public float arcDegrees = 120f;

        [Header("Seria")]
        [Tooltip("Kolejne ciosy serii.")]
        public MeleeComboStep[] combo =
        {
            new MeleeComboStep { damageMultiplier = 1f, windup = 0.14f, active = 0.1f, recovery = 0.22f, knockbackForce = 6f, hitStop = 0.05f, lunge = 0.3f },
            new MeleeComboStep { damageMultiplier = 1f, windup = 0.14f, active = 0.1f, recovery = 0.22f, knockbackForce = 6f, hitStop = 0.05f, lunge = 0.3f },
            new MeleeComboStep { damageMultiplier = 1.5f, windup = 0.22f, active = 0.12f, recovery = 0.38f, knockbackForce = 14f, hitStop = 0.08f, lunge = 0.5f },
        };

        [Tooltip("Ile sekund po ciosie można jeszcze kontynuować serię.")]
        [Min(0f)] public float comboResetTime = 0.5f;

        [Tooltip("Mnożnik prędkości ruchu w trakcie ciosu.")]
        [Range(0f, 1f)] public float moveSpeedMultiplier = 0.3f;

        [Header("Wygląd zamachu (szara bryła do M10)")]
        [Tooltip("Długość broni (m). Zmiana wymaga ponownego zbudowania sceny.")]
        [Min(0.1f)] public float visualLength = 1.2f;

        [Tooltip("Ostatni cios serii: jak wysoko broń idzie nad głowę (stopnie).")]
        [Range(0f, 120f)] public float overheadRaiseDegrees = 80f;

        [Tooltip("Ostatni cios serii: gdzie kończy się uderzenie (stopnie w dół).")]
        [Range(0f, 60f)] public float overheadEndDegrees = 25f;

        [Header("Złom i Moc")]
        [Tooltip("Najmniej śrubek wybitych jednym trafieniem.")]
        [Min(0)] public int scrapPerHitMin = 1;

        [Tooltip("Najwięcej śrubek wybitych jednym trafieniem.")]
        [Min(0)] public int scrapPerHitMax = 3;

        [Tooltip("Moc za każde trafienie.")]
        [Min(0f)] public float powerPerHit = 4f;
    }
}
