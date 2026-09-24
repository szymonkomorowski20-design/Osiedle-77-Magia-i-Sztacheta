using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Wszystkie liczby ruchu, dasha i kamery gracza. Stroisz je w Inspectorze pliku Data/Player/PlayerData,
    /// także w trakcie gry (Play Mode) — zmiany działają od razu.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Osiedle/Dane/Gracz")]
    public class PlayerData : ScriptableObject
    {
        [Header("Ruch")]
        [Tooltip("Prędkość biegu (m/s). Bez przyspieszenia: reakcja natychmiastowa.")]
        [Min(0f)] public float moveSpeed = 6f;

        [Tooltip("Grawitacja (m/s²).")]
        [Min(0f)] public float gravity = 30f;

        [Tooltip("Maksymalna prędkość spadania (m/s).")]
        [Min(0f)] public float maxFallSpeed = 25f;

        [Tooltip("Stały docisk do ziemi (m/s), żeby postać nie odrywała się od podłogi.")]
        [Min(0f)] public float groundStickSpeed = 2f;

        [Tooltip("Coyote time (s): tyle po zejściu z krawędzi postać jeszcze nie spada.")]
        [Min(0f)] public float coyoteTime = 0.1f;

        [Header("Celowanie")]
        [Tooltip("Wysokość biodra nad stopami (m). Na tej wysokości leży płaszczyzna, w którą celuje kursor.")]
        [Min(0f)] public float hipHeight = 0.75f;

        [Header("Dash")]
        [Tooltip("Dystans dasha (m).")]
        [Min(0f)] public float dashDistance = 4f;

        [Tooltip("Czas trwania dasha (s).")]
        [Min(0.01f)] public float dashDuration = 0.18f;

        [Tooltip("Czas odnowienia jednego ładunku (s).")]
        [Min(0f)] public float dashCooldown = 0.8f;

        [Tooltip("Liczba ładunków dasha. Ulepszenia mogą dać 2.")]
        [Min(1)] public int dashCharges = 1;

        [Tooltip("Nieśmiertelność od początku dasha (s).")]
        [Min(0f)] public float dashInvulnerability = 0.3f;

        [Tooltip("Bufor wejścia (s): naciśnięcie dasha tyle za wcześnie i tak się wykona.")]
        [Min(0f)] public float inputBufferTime = 0.15f;

        [Header("Skok na przeszkodę (dash kontekstowy)")]
        [Tooltip("Jak daleko przed postacią (m, od krawędzi kapsuły) szukamy obiektu do wskoczenia.")]
        [Min(0f)] public float vaultDetectDistance = 1.2f;

        [Tooltip("Wysokość nad stopami (m), na której sprawdzamy, czy przed postacią jest przeszkoda.")]
        [Min(0f)] public float vaultProbeHeight = 0.2f;

        [Tooltip("Promień sondy wykrywającej przeszkodę (m).")]
        [Min(0.01f)] public float vaultProbeRadius = 0.15f;

        [Tooltip("Najniższa przeszkoda (m), na którą warto skakać. Niższe postać po prostu przechodzi.")]
        [Min(0f)] public float vaultMinHeight = 0.3f;

        [Tooltip("Najwyższa przeszkoda (m), na którą da się wskoczyć.")]
        [Min(0f)] public float vaultMaxHeight = 2.2f;

        [Tooltip("Czas skoku (s).")]
        [Min(0.01f)] public float vaultDuration = 0.35f;

        [Tooltip("Dodatkowa wysokość łuku skoku (m).")]
        [Min(0f)] public float vaultArcHeight = 0.5f;

        [Tooltip("Jak głęboko za krawędzią (m) postać ląduje.")]
        [Min(0f)] public float vaultLandingInset = 0.5f;

        [Tooltip("Z jakiej odległości (m) pokazuje się strzałka nad krawędzią.")]
        [Min(0f)] public float vaultPromptRadius = 1.8f;

        [Tooltip("Wysokość strzałki nad krawędzią (m).")]
        [Min(0f)] public float vaultPromptHeight = 0.4f;

        [Header("Kamera")]
        [Tooltip("Kąt patrzenia kamery w dół (stopnie).")]
        [Range(20f, 89f)] public float cameraPitch = 55f;

        [Tooltip("Odległość kamery od postaci (m).")]
        [Min(1f)] public float cameraDistance = 16f;

        [Tooltip("Pole widzenia kamery (stopnie).")]
        [Range(10f, 90f)] public float cameraFieldOfView = 40f;

        [Tooltip("Wygładzenie ruchu kamery (s). 0 = sztywno.")]
        [Min(0f)] public float cameraDamping = 0.2f;

        [Tooltip("Maksymalne wyprzedzenie kamery w stronę kursora (m).")]
        [Min(0f)] public float lookAheadMax = 3f;

        [Tooltip("Jaka część odległości do kursora przesuwa kamerę (0..1).")]
        [Range(0f, 1f)] public float lookAheadFactor = 0.35f;

        [Tooltip("Czas wygładzenia wyprzedzenia (s).")]
        [Min(0f)] public float lookAheadSmoothTime = 0.15f;
    }
}
