using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Czerwony telegraf ataku na ziemi: ciemnoczerwony pas pokazuje, gdzie uderzy atak,
    /// a jasnoczerwone wypełnienie rośnie do pełna w chwili ataku. Czerwień jest zarezerwowana tylko do tego.
    /// </summary>
    public class AttackTelegraph : MonoBehaviour
    {
        [Tooltip("Pełny obrys strefy ataku.")]
        [SerializeField] Transform lane;
        [Tooltip("Wypełnienie, które rośnie z postępem zamachu.")]
        [SerializeField] Transform fill;

        float length;
        float width;

        public void Show(float laneLength, float laneWidth)
        {
            length = laneLength;
            width = laneWidth;
            gameObject.SetActive(true);
            SetStrip(lane, 1f);
            SetProgress(0f);
        }

        /// <summary>Postęp zamachu 0..1 — wypełnienie od wroga do końca pasa.</summary>
        public void SetProgress(float progress) => SetStrip(fill, Mathf.Clamp01(progress));

        public void Hide() => gameObject.SetActive(false);

        void SetStrip(Transform strip, float fraction)
        {
            float stripLength = length * fraction;
            Vector3 scale = strip.localScale;
            strip.localScale = new Vector3(width, scale.y, Mathf.Max(stripLength, 0.001f));
            Vector3 position = strip.localPosition;
            strip.localPosition = new Vector3(0f, position.y, stripLength * 0.5f);
        }
    }
}
