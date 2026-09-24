using System;
using Osiedle.Combat;
using UnityEngine;

namespace Osiedle.Core
{
    /// <summary>
    /// Statyczne zdarzenia gry. Systemy (ulepszenia, UI, dźwięk) tylko się podpinają w OnEnable
    /// i zawsze odpinają w OnDisable. Kolejne zdarzenia (OnPerfectDash, OnRoomCleared...) dochodzą w następnych etapach.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Gracz wykonał dash. Parametr: kierunek dasha (płaski, znormalizowany).</summary>
        public static event Action<Vector3> OnDash;

        /// <summary>Gracz wskoczył na obiekt (dash kontekstowy). Parametr: punkt lądowania.</summary>
        public static event Action<Vector3> OnVault;

        /// <summary>Dowolne przyjęte trafienie (gracza lub wroga). Parametry: trafienie, cel.</summary>
        public static event Action<DamageInfo, Hurtbox> OnHit;

        /// <summary>Coś zginęło. Parametry: ostatnie trafienie, zdrowie ofiary.</summary>
        public static event Action<DamageInfo, Health> OnKill;

        /// <summary>Gracz dostał obrażenia. Parametry: trafienie, faktycznie zadane obrażenia.</summary>
        public static event Action<DamageInfo, float> OnDamageTaken;

        /// <summary>Zmienił się Złom gracza. Parametry: obecnie, maksimum.</summary>
        public static event Action<int, int> OnScrapChanged;

        /// <summary>Zmieniła się Moc gracza. Parametry: obecnie, maksimum.</summary>
        public static event Action<float, float> OnPowerChanged;

        public static void RaiseDash(Vector3 direction) => OnDash?.Invoke(direction);
        public static void RaiseVault(Vector3 landingPoint) => OnVault?.Invoke(landingPoint);
        public static void RaiseHit(DamageInfo info, Hurtbox target) => OnHit?.Invoke(info, target);
        public static void RaiseKill(DamageInfo info, Health victim) => OnKill?.Invoke(info, victim);
        public static void RaiseDamageTaken(DamageInfo info, float dealt) => OnDamageTaken?.Invoke(info, dealt);
        public static void RaiseScrapChanged(int current, int max) => OnScrapChanged?.Invoke(current, max);
        public static void RaisePowerChanged(float current, float max) => OnPowerChanged?.Invoke(current, max);

        // Czyści subskrypcje na starcie Play Mode (działa też przy wyłączonym przeładowaniu domeny).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetAll()
        {
            OnDash = null;
            OnVault = null;
            OnHit = null;
            OnKill = null;
            OnDamageTaken = null;
            OnScrapChanged = null;
            OnPowerChanged = null;
        }
    }
}
