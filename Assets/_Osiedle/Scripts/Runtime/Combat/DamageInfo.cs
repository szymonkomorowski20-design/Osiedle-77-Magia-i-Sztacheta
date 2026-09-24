using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Jedno trafienie: ile obrażeń, jaki żywioł, jak mocny odrzut i w którą stronę, kto uderzył, czy krytyk
    /// i jak długi hit-stop. Niesie je Hitbox do Hurtboxa.
    /// </summary>
    public struct DamageInfo
    {
        public float Amount;
        public Element Element;

        /// <summary>Początkowa prędkość odrzutu (m/s).</summary>
        public float KnockbackForce;

        /// <summary>Płaski kierunek odrzutu (od uderzającego do celu).</summary>
        public Vector3 Direction;

        /// <summary>Punkt trafienia w świecie (tu wylatują śrubki i efekty).</summary>
        public Vector3 Point;

        public GameObject Source;
        public bool IsCritical;

        /// <summary>Na ile sekund zamrozić grę po trafieniu (0 = bez hit-stopu).</summary>
        public float HitStop;

        public Vector3 KnockbackVelocity => Direction * KnockbackForce;
    }
}
