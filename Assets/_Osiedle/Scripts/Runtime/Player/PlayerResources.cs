using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Zasoby gracza: ustawia jego Health z PlayerData, trzyma Złom (amunicja) i Moc (czar na Q).
    /// Każdą zmianę ogłasza w GameEvents, żeby UI i ulepszenia mogły się podpiąć.
    /// </summary>
    public class PlayerResources : MonoBehaviour
    {
        [SerializeField] PlayerData data;
        [SerializeField] Health health;

        ResourcePool scrap;
        ResourcePool power;

        public Health Health => health;
        public int Scrap => Mathf.RoundToInt(scrap.Current);
        public int MaxScrap => Mathf.RoundToInt(scrap.Max);
        public bool IsScrapFull => scrap.IsFull;
        public float Power => power.Current;
        public float MaxPower => power.Max;

        void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            if (data == null)
            {
                OsiedleLog.Error("PlayerResources: brak PlayerData.", this);
                enabled = false;
                return;
            }

            health.Configure(data.maxHealth, data.hitInvulnerability);
            scrap = new ResourcePool(data.maxScrap, data.startScrap);
            power = new ResourcePool(data.maxPower, 0f);
        }

        void OnEnable()
        {
            if (health != null) health.Damaged += HandleDamaged;
        }

        void OnDisable()
        {
            if (health != null) health.Damaged -= HandleDamaged;
        }

        void Start()
        {
            GameEvents.RaiseScrapChanged(Scrap, MaxScrap);
            GameEvents.RaisePowerChanged(Power, MaxPower);
        }

        /// <summary>Dodaje Złom (do limitu). Zwraca, ile faktycznie dodano.</summary>
        public int AddScrap(int amount)
        {
            int added = Mathf.RoundToInt(scrap.Add(amount));
            if (added > 0) GameEvents.RaiseScrapChanged(Scrap, MaxScrap);
            return added;
        }

        public bool TrySpendScrap(int amount)
        {
            if (!scrap.TrySpend(amount)) return false;
            GameEvents.RaiseScrapChanged(Scrap, MaxScrap);
            return true;
        }

        public void AddPower(float amount)
        {
            if (power.Add(amount) > 0f) GameEvents.RaisePowerChanged(Power, MaxPower);
        }

        void HandleDamaged(DamageInfo info, float dealt) => GameEvents.RaiseDamageTaken(info, dealt);
    }
}
