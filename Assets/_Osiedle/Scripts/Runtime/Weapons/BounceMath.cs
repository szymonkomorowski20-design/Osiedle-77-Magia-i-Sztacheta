using UnityEngine;

namespace Osiedle.Weapons
{
    /// <summary>Odbicie pocisku od ściany w płaszczyźnie gry (bez lotu w górę/dół). Czysta logika, ma testy.</summary>
    public static class BounceMath
    {
        const float MinFlatSqr = 0.0001f;

        /// <summary>
        /// Odbija kierunek od ściany o podanej normalnej i spłaszcza go do poziomu.
        /// Zwraca false, gdy powierzchnia jest pozioma (podłoga, sufit) albo odbicie nie ma sensu.
        /// </summary>
        public static bool TryReflectFlat(Vector3 direction, Vector3 normal, out Vector3 reflected)
        {
            reflected = Vector3.zero;
            Vector3 flatNormal = new Vector3(normal.x, 0f, normal.z);
            if (flatNormal.sqrMagnitude < MinFlatSqr) return false;

            Vector3 result = Vector3.Reflect(new Vector3(direction.x, 0f, direction.z), flatNormal.normalized);
            result.y = 0f;
            if (result.sqrMagnitude < MinFlatSqr) return false;

            reflected = result.normalized;
            return true;
        }
    }
}
