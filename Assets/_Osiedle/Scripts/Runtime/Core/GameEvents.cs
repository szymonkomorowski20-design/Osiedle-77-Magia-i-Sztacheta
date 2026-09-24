using System;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Statyczne zdarzenia gry. Systemy (ulepszenia, UI, dźwięk) tylko się podpinają w OnEnable
    /// i zawsze odpinają w OnDisable. Kolejne zdarzenia (OnHit, OnKill, OnPerfectDash...) dochodzą w następnych etapach.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Gracz wykonał dash. Parametr: kierunek dasha (płaski, znormalizowany).</summary>
        public static event Action<Vector3> OnDash;

        /// <summary>Gracz wskoczył na obiekt (dash kontekstowy). Parametr: punkt lądowania.</summary>
        public static event Action<Vector3> OnVault;

        public static void RaiseDash(Vector3 direction) => OnDash?.Invoke(direction);
        public static void RaiseVault(Vector3 landingPoint) => OnVault?.Invoke(landingPoint);

        // Czyści subskrypcje na starcie Play Mode (działa też przy wyłączonym przeładowaniu domeny).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetAll()
        {
            OnDash = null;
            OnVault = null;
        }
    }
}
