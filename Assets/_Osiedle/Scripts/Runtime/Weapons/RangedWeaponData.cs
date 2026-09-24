using Osiedle.Combat;
using UnityEngine;

namespace Osiedle.Weapons
{
    /// <summary>
    /// Dane broni dystansowej. Domyślne wartości to Proca na śruby z GDD: precyzyjna, szybka seria,
    /// 1 Złom na strzał, pocisk odbija się raz od ściany, z podwyższenia +20% obrażeń.
    /// </summary>
    [CreateAssetMenu(fileName = "RangedWeapon", menuName = "Osiedle/Dane/Broń dystansowa")]
    public class RangedWeaponData : ScriptableObject
    {
        [Header("Obrażenia")]
        [Min(0f)] public float damage = 12f;
        public Element element = Element.Physical;

        [Tooltip("Prędkość odrzutu celu (m/s). Proca odpycha lekko.")]
        [Min(0f)] public float knockbackForce = 3f;

        [Tooltip("Hit-stop po trafieniu (s). Proca: bez hit-stopu, żeby seria była płynna.")]
        [Min(0f)] public float hitStop = 0f;

        [Header("Strzelanie")]
        [Tooltip("Koszt Złomu za jeden strzał.")]
        [Min(0)] public int scrapCost = 1;

        [Tooltip("Czas między strzałami przy trzymaniu LPM (s).")]
        [Min(0.02f)] public float fireInterval = 0.18f;

        [Tooltip("Losowy rozrzut strzału (stopnie, całkowita szerokość).")]
        [Range(0f, 30f)] public float spreadDegrees = 2f;

        [Header("Pocisk")]
        [Tooltip("Prędkość pocisku (m/s). Pociski gracza są szybsze niż pociski wrogów.")]
        [Min(1f)] public float projectileSpeed = 22f;

        [Tooltip("Promień pocisku (m) — do zderzeń ze ścianami i celami.")]
        [Min(0.01f)] public float projectileRadius = 0.12f;

        [Tooltip("Zasięg (m), po którym pocisk znika.")]
        [Min(1f)] public float maxRange = 18f;

        [Tooltip("Ile razy pocisk odbija się od ściany.")]
        [Min(0)] public int bounces = 1;

        [Tooltip("Jak nisko pod linią lotu (m) pocisk trafia cele — żeby z podwyższenia trafiać wrogów na dole.")]
        [Min(0f)] public float targetReachDown = 1.5f;

        [Tooltip("Jak wysoko nad linią lotu (m) pocisk trafia cele.")]
        [Min(0f)] public float targetReachUp = 0.5f;

        [Header("Strzał z góry")]
        [Tooltip("Premia do obrażeń, gdy strzelasz z podwyższenia (0,2 = +20%).")]
        [Min(0f)] public float elevationBonus = 0.2f;

        [Tooltip("O ile metrów stopy strzelca muszą być wyżej niż stopy celu, żeby liczyła się premia.")]
        [Min(0f)] public float elevationThreshold = 0.5f;

        [Header("Moc i odczucia")]
        [Tooltip("Moc za każde trafienie.")]
        [Min(0f)] public float powerPerHit = 2f;

        [Tooltip("Siła wstrząsu ekranu przy strzale (mały).")]
        [Min(0f)] public float shotShake = 0.08f;
    }
}
