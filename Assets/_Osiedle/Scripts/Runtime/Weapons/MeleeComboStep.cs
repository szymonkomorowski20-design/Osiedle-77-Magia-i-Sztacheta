using System;
using UnityEngine;

namespace Osiedle.Weapons
{
    /// <summary>Jeden cios z serii broni białej: czasy, siła, odrzut i hit-stop.</summary>
    [Serializable]
    public struct MeleeComboStep
    {
        [Tooltip("Mnożnik obrażeń broni dla tego ciosu.")]
        [Min(0f)] public float damageMultiplier;

        [Tooltip("Zamach przed trafieniem (s).")]
        [Min(0f)] public float windup;

        [Tooltip("Jak długo cios trafia (s).")]
        [Min(0.01f)] public float active;

        [Tooltip("Powrót po ciosie (s). W tym czasie następny cios czeka w buforze.")]
        [Min(0f)] public float recovery;

        [Tooltip("Prędkość odrzutu celu (m/s).")]
        [Min(0f)] public float knockbackForce;

        [Tooltip("Hit-stop po trafieniu (s). GDD: 0,05–0,08 przy mocnym ciosie.")]
        [Min(0f)] public float hitStop;

        [Tooltip("Krok do przodu w trakcie ciosu (m).")]
        [Min(0f)] public float lunge;

        public float Duration => windup + active + recovery;
    }
}
