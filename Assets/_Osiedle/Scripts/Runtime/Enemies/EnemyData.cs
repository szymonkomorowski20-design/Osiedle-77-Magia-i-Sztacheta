using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Dane wroga. W M1 używa ich tylko manekin; zachowanie, ataki i telegrafy dojdą z pierwszymi wrogami.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Osiedle/Dane/Wróg")]
    public class EnemyData : ScriptableObject
    {
        [Header("Ciało (zmiana wymaga ponownego zbudowania sceny: Osiedle/Build/...)")]
        [Tooltip("Wysokość kapsuły kolizji (m).")]
        [Min(0.3f)] public float bodyHeight = 1.6f;

        [Tooltip("Promień kapsuły kolizji (m). Większy = łatwiej trafić.")]
        [Min(0.1f)] public float bodyRadius = 0.4f;

        [Header("Zdrowie")]
        [Min(1f)] public float maxHealth = 300f;

        [Tooltip("Nieśmiertelność po trafieniu (s). Wrogowie zwykle 0.")]
        [Min(0f)] public float hitInvulnerability = 0f;

        [Header("Odrzut")]
        [Tooltip("1 = normalny odrzut, 0 = nie odlatuje.")]
        [Min(0f)] public float knockbackMultiplier = 1f;

        [Tooltip("Jak szybko wygasa odrzut (m/s²). Mniej = dalej leci.")]
        [Min(0f)] public float knockbackDeceleration = 40f;

        [Header("Reakcja na trafienie")]
        [Tooltip("Jak długo wróg błyska po trafieniu (s).")]
        [Min(0f)] public float hitFlashDuration = 0.08f;

        [Tooltip("Kolor błysku. Nie czerwony: czerwień jest zarezerwowana dla telegrafów.")]
        public Color hitFlashColor = Color.white;

        [Header("Tylko manekin treningowy")]
        [Tooltip("Po ilu sekundach bez trafienia zaczyna się leczyć.")]
        [Min(0f)] public float regenDelay = 2f;

        [Tooltip("Leczenie na sekundę.")]
        [Min(0f)] public float regenPerSecond = 150f;

        [Tooltip("Po ilu sekundach wstaje po „śmierci”.")]
        [Min(0f)] public float respawnDelay = 1f;
    }
}
