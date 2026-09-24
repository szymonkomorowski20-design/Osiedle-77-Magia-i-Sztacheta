using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Szarża Kibica (GDD, bestiariusz): po 0,6 s zamachu biegnie prosto, nie skręca.
    /// Kontra: dash w bok, a gdy wbije się w ścianę — bić w plecy, póki jest ogłuszony.
    /// </summary>
    [CreateAssetMenu(fileName = "ChargerData", menuName = "Osiedle/Dane/Szarża wroga")]
    public class ChargerData : ScriptableObject
    {
        [Header("Kiedy szarżuje")]
        [Tooltip("Z jakiej odległości (m) od gracza zaczyna zamach.")]
        [Min(0.5f)] public float engageDistance = 7f;

        [Tooltip("Zamach = telegraf (s). Zasada gry: każdy atak wroga ma co najmniej 0,4 s ostrzeżenia.")]
        [Min(0.4f)] public float windupTime = 0.6f;

        [Header("Szarża")]
        [Tooltip("Prędkość szarży (m/s).")]
        [Min(1f)] public float chargeSpeed = 13f;

        [Tooltip("Długość szarży (m) — tyle ma też czerwony pas telegrafu.")]
        [Min(1f)] public float chargeDistance = 9f;

        [Tooltip("Obrażenia dla Kuby.")]
        [Min(0f)] public float damage = 20f;

        [Tooltip("Prędkość odrzutu Kuby (m/s) — spycha na bok z toru szarży.")]
        [Min(0f)] public float knockbackForce = 12f;

        [Tooltip("Hit-stop po trafieniu Kuby (s).")]
        [Min(0f)] public float hitStop = 0.08f;

        [Tooltip("Dodatkowy zasięg trafienia (m) poza obrysem ciał.")]
        [Min(0f)] public float contactPadding = 0.15f;

        [Header("Po szarży")]
        [Tooltip("Ogłuszenie po wbiciu się w ścianę (s) — okno na kontrę.")]
        [Min(0f)] public float stunDuration = 1.5f;

        [Tooltip("Odpoczynek po szarży bez ściany (s).")]
        [Min(0f)] public float recoverDuration = 0.7f;

        [Header("Telegraf")]
        [Tooltip("Ile szerszy (m) od ciała Kibica jest czerwony pas.")]
        [Min(0f)] public float telegraphExtraWidth = 0.5f;
    }
}
