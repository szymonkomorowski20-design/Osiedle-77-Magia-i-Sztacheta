using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>Obliczenia szarży bez sceny (mają testy).</summary>
    public static class ChargeMath
    {
        const float MinSqr = 0.0001f;

        /// <summary>
        /// Kierunek odrzutu trafionego: na bok od toru szarży (po tej stronie, po której stoi cel),
        /// z lekkim pchnięciem do przodu. Stojący idealnie na osi leci w prawo.
        /// </summary>
        public static Vector3 KnockbackDirection(Vector3 chargeDirection, Vector3 toTarget)
        {
            Vector3 forward = new Vector3(chargeDirection.x, 0f, chargeDirection.z).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            float side = Vector3.Dot(new Vector3(toTarget.x, 0f, toTarget.z), right);
            Vector3 lateral = side >= 0f ? right : -right;
            Vector3 result = lateral + forward * 0.5f;
            return result.sqrMagnitude > MinSqr ? result.normalized : right;
        }

        /// <summary>Czy dwa ciała (okręgi na płaszczyźnie) stykają się, z dodatkowym zapasem.</summary>
        public static bool Touching(Vector3 a, float radiusA, Vector3 b, float radiusB, float padding)
        {
            Vector3 flat = new Vector3(b.x - a.x, 0f, b.z - a.z);
            float reach = radiusA + radiusB + padding;
            return flat.sqrMagnitude <= reach * reach;
        }
    }
}
