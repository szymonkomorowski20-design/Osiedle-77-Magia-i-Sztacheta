using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Widoczny zamach broni białej (szara bryła do M10). Ciosy serii idą poziomo na przemian
    /// z lewej i z prawej, ostatni cios uderza z góry. Broń widać tylko w trakcie ciosu.
    /// Łuk zamachu = arcDegrees broni, więc wygląd zgadza się z tym, co naprawdę trafia.
    /// </summary>
    [DefaultExecutionOrder(40)]
    public class MeleeSwingVisual : MonoBehaviour
    {
        [SerializeField] PlayerMelee melee;
        [Tooltip("Punkt obrotu broni (na wysokości biodra). Broń jest jego dzieckiem, skierowana wzdłuż +Z.")]
        [SerializeField] Transform pivot;

        void Awake()
        {
            if (melee == null) melee = GetComponent<PlayerMelee>();
        }

        void LateUpdate()
        {
            if (melee == null || pivot == null || melee.Weapon == null) return;

            bool visible = melee.IsAttacking;
            if (pivot.gameObject.activeSelf != visible) pivot.gameObject.SetActive(visible);
            if (!visible) return;

            var weapon = melee.Weapon;
            float t = melee.PhaseProgress;
            float eased = 1f - (1f - t) * (1f - t);

            if (melee.IsFinisher)
            {
                // Uderzenie z góry: broń uniesiona nad głowę, potem w dół przed postacią.
                float raised = -weapon.overheadRaiseDegrees;
                float pitch = melee.Phase switch
                {
                    MeleePhase.Windup => Mathf.Lerp(0f, raised, eased),
                    MeleePhase.Active => Mathf.Lerp(raised, weapon.overheadEndDegrees, eased),
                    _ => weapon.overheadEndDegrees,
                };
                pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                return;
            }

            // Poziomy zamach: parzyste ciosy z prawej na lewą, nieparzyste odwrotnie.
            float half = weapon.arcDegrees * 0.5f;
            float side = melee.CurrentStep % 2 == 0 ? 1f : -1f;
            float from = half * side;
            float to = -half * side;
            float yaw = melee.Phase switch
            {
                MeleePhase.Windup => Mathf.Lerp(0f, from, eased),
                MeleePhase.Active => Mathf.Lerp(from, to, eased),
                _ => to,
            };
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
