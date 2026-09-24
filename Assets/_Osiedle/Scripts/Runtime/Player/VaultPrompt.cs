using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Pokazuje małą strzałkę nad krawędzią najbliższego obiektu, na który da się wskoczyć.
    /// </summary>
    public class VaultPrompt : MonoBehaviour
    {
        const int MaxColliders = 16;

        [SerializeField] PlayerData data;
        [SerializeField] PlayerDash dash;
        [Tooltip("Obiekt strzałki (dziecko gracza). Włączany tylko, gdy jest na co wskoczyć.")]
        [SerializeField] Transform arrow;
        [SerializeField] LayerMask obstacleMask = ~0;

        readonly Collider[] hits = new Collider[MaxColliders];

        void Awake()
        {
            if (dash == null) dash = GetComponent<PlayerDash>();
        }

        void OnDisable()
        {
            if (arrow != null) arrow.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (arrow == null || data == null) return;

            bool found = TryFindNearestEdge(out Vector3 edge);
            bool show = found && (dash == null || !dash.IsBusy);
            if (arrow.gameObject.activeSelf != show) arrow.gameObject.SetActive(show);
            if (!show) return;

            arrow.SetPositionAndRotation(edge + Vector3.up * data.vaultPromptHeight, Quaternion.identity);
        }

        bool TryFindNearestEdge(out Vector3 bestEdge)
        {
            bestEdge = default;
            Vector3 feet = transform.position;
            Vector3 center = feet + Vector3.up * data.hipHeight;
            int count = Physics.OverlapSphereNonAlloc(center, data.vaultPromptRadius, hits, obstacleMask,
                QueryTriggerInteraction.Ignore);

            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var vaultable = hits[i].GetComponentInParent<Vaultable>();
                if (vaultable == null) continue;

                float rise = vaultable.TopHeight - feet.y;
                if (rise < data.vaultMinHeight || rise > data.vaultMaxHeight) continue;

                Vector3 edge = vaultable.ClosestEdgePoint(feet);
                Vector3 flat = edge - feet;
                flat.y = 0f;
                float distance = flat.sqrMagnitude;
                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestEdge = edge;
            }

            return bestDistance < float.MaxValue;
        }
    }
}
