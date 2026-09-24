using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Znacznik obiektu, na który da się wskoczyć dashem (murek, skrzynia, maska samochodu, dach garażu).
    /// Dodaj go do obiektu z colliderem.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Vaultable : MonoBehaviour
    {
        Collider cachedCollider;

        public Collider Collider => cachedCollider != null ? cachedCollider : cachedCollider = GetComponent<Collider>();

        /// <summary>Wysokość górnej powierzchni obiektu w świecie.</summary>
        public float TopHeight => Collider.bounds.max.y;

        /// <summary>Najbliższy punkt górnej krawędzi względem podanej pozycji (tu stawiamy strzałkę).</summary>
        public Vector3 ClosestEdgePoint(Vector3 from)
        {
            float top = TopHeight;
            Vector3 point = Collider.ClosestPoint(new Vector3(from.x, top, from.z));
            point.y = top;
            return point;
        }
    }
}
