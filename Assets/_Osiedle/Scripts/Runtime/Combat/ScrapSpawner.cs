using Osiedle.Core;
using Osiedle.Player;
using UnityEngine;
using UnityEngine.Pool;

namespace Osiedle.Combat
{
    /// <summary>
    /// Wybija śrubki Złomu przy trafieniu i przekazuje zebrane do <see cref="PlayerResources"/>.
    /// Śrubki biorą się z puli (ObjectPool) — żadnego Instantiate/Destroy w trakcie walki.
    /// </summary>
    public class ScrapSpawner : MonoBehaviour
    {
        [SerializeField] PlayerData data;
        [SerializeField] PlayerResources resources;
        [SerializeField] ScrapPickup pickupPrefab;

        ObjectPool<ScrapPickup> pool;

        public PlayerData Data => data;
        public bool CanCollect => resources != null && !resources.IsScrapFull;
        public Vector3 CollectPoint => resources.transform.position + Vector3.up * data.hipHeight;

        void Awake()
        {
            if (resources == null) resources = GetComponentInParent<PlayerResources>();
            if (data == null || pickupPrefab == null)
            {
                OsiedleLog.Error("ScrapSpawner: brak PlayerData albo prefabu śrubki.", this);
                enabled = false;
                return;
            }

            pool = new ObjectPool<ScrapPickup>(CreatePickup, OnGet, OnRelease, OnDestroyPickup,
                collectionCheck: true, defaultCapacity: data.scrapPoolSize);
            Prewarm(data.scrapPoolSize);
        }

        void OnDestroy() => pool?.Clear();

        /// <summary>Wybija <paramref name="count"/> śrubek z punktu trafienia.</summary>
        /// <param name="groundHeight">Wysokość ziemi, na którą spadną (zwykle stopy trafionego).</param>
        public void Spawn(Vector3 point, int count, float groundHeight)
        {
            if (pool == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector2 flat = Random.insideUnitCircle.normalized * (data.scrapScatterSpeed * Random.Range(data.scrapScatterMinFraction, 1f));
                var velocity = new Vector3(flat.x, data.scrapScatterUpSpeed, flat.y);
                pool.Get().Launch(this, point, velocity, groundHeight);
            }
        }

        public void Collect(ScrapPickup pickup)
        {
            resources.AddScrap(1);
            pool.Release(pickup);
        }

        void Prewarm(int count)
        {
            var temp = new ScrapPickup[count];
            for (int i = 0; i < count; i++) temp[i] = pool.Get();
            for (int i = 0; i < count; i++) pool.Release(temp[i]);
        }

        ScrapPickup CreatePickup()
        {
            ScrapPickup pickup = Instantiate(pickupPrefab);
            pickup.gameObject.SetActive(false);
            return pickup;
        }

        static void OnGet(ScrapPickup pickup) => pickup.gameObject.SetActive(true);
        static void OnRelease(ScrapPickup pickup) => pickup.gameObject.SetActive(false);

        static void OnDestroyPickup(ScrapPickup pickup)
        {
            if (pickup != null) Destroy(pickup.gameObject);
        }
    }
}
