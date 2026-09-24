using Osiedle.Combat;
using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Enemies
{
    /// <summary>
    /// Kibic Szarżujący — maszyna stanów: podchodzi → zamach z czerwonym telegrafem → szarża prosto
    /// (nie skręca, przelatuje przez Kubę i spycha go na bok) → ogłuszenie po wbiciu się w przeszkodę
    /// albo krótki odpoczynek → od nowa. Idzie prosto do celu (bez NavMesh — na otwartym podwórku wystarcza).
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public class ChargerBrain : MonoBehaviour
    {
        const float MinDirectionSqr = 0.01f;
        // Szarża przesunęła się o mniej niż tyle z kroku → coś ją zatrzymało (ściana, skrzynia, manekin).
        const float BlockedFraction = 0.5f;

        // Kąty kija bejsbolowego — tylko czytelność zamachu (szara bryła).
        const float BatRestPitch = 40f;
        const float BatRaisedPitch = -110f;
        const float BatChargePitch = -15f;
        const float BatStunnedPitch = 85f;

        public enum State { Chase, Windup, Charge, Stunned, Recover, Dead }

        [SerializeField] EnemyData data;
        [SerializeField] ChargerData charge;
        [SerializeField] Health health;
        [SerializeField] Knockback knockback;
        [SerializeField] HitFlash flash;
        [SerializeField] AttackTelegraph telegraph;
        [Tooltip("Punkt obrotu kija (wygląd zamachu).")]
        [SerializeField] Transform batPivot;
        [Tooltip("Cel: Hurtbox gracza.")]
        [SerializeField] Hurtbox target;

        CharacterController controller;
        CharacterController targetController;
        State state;
        float timer;
        Vector3 chargeDirection;
        float travelled;
        bool hitThisCharge;

        public State CurrentState => state;

        bool TargetAlive => target != null && target.Health != null && !target.Health.IsDead;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (health == null) health = GetComponent<Health>();
            if (knockback == null) knockback = GetComponent<Knockback>();
            if (flash == null) flash = GetComponent<HitFlash>();
            if (data == null || charge == null)
            {
                OsiedleLog.Error("ChargerBrain: brak EnemyData albo ChargerData.", this);
                enabled = false;
                return;
            }

            health.Configure(data.maxHealth, data.hitInvulnerability);
            if (knockback != null) knockback.Configure(data.knockbackMultiplier, data.knockbackDeceleration);
            if (flash != null) flash.Configure(data.hitFlashDuration, data.hitFlashColor);
            if (telegraph != null) telegraph.Hide();
            if (target != null) targetController = target.Owner.GetComponent<CharacterController>();
        }

        void OnEnable() => health.Died += HandleDied;

        void OnDisable()
        {
            health.Died -= HandleDied;
            IgnoreTarget(false);
        }

        void Update()
        {
            switch (state)
            {
                case State.Chase: UpdateChase(); break;
                case State.Windup: UpdateWindup(); break;
                case State.Charge: UpdateCharge(); break;
                case State.Stunned: Rest(charge.stunDuration); break;
                case State.Recover: Rest(charge.recoverDuration); break;
                case State.Dead: return;
            }
            UpdateBat();
        }

        void UpdateChase()
        {
            if (!TargetAlive)
            {
                controller.SimpleMove(Vector3.zero);
                return;
            }

            Vector3 toTarget = Flat(target.Owner.transform.position - transform.position);
            if (toTarget.sqrMagnitude < MinDirectionSqr) return;

            Vector3 direction = toTarget.normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            if (toTarget.magnitude <= charge.engageDistance) BeginWindup(direction);
            else controller.SimpleMove(direction * data.moveSpeed);
        }

        void BeginWindup(Vector3 direction)
        {
            state = State.Windup;
            timer = 0f;
            chargeDirection = direction;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            if (telegraph != null)
                telegraph.Show(charge.chargeDistance + controller.radius, controller.radius * 2f + charge.telegraphExtraWidth);
        }

        void UpdateWindup()
        {
            timer += Time.deltaTime;
            controller.SimpleMove(Vector3.zero);
            if (telegraph != null) telegraph.SetProgress(timer / charge.windupTime);
            if (timer >= charge.windupTime) BeginCharge();
        }

        void BeginCharge()
        {
            state = State.Charge;
            travelled = 0f;
            hitThisCharge = false;
            if (telegraph != null) telegraph.Hide();
            // Szarża przelatuje przez Kubę (spycha go na bok), zamiast się o niego zatrzymać.
            IgnoreTarget(true);
        }

        void UpdateCharge()
        {
            TryHitTarget();

            Vector3 before = transform.position;
            float step = charge.chargeSpeed * Time.deltaTime;
            controller.SimpleMove(chargeDirection * charge.chargeSpeed);
            float moved = Flat(transform.position - before).magnitude;
            travelled += moved;

            if (moved < step * BlockedFraction) EndCharge(State.Stunned);
            else if (travelled >= charge.chargeDistance) EndCharge(State.Recover);
        }

        void TryHitTarget()
        {
            if (hitThisCharge || !TargetAlive) return;

            Vector3 targetPosition = target.Owner.transform.position;
            float targetRadius = targetController != null ? targetController.radius : 0f;
            if (!ChargeMath.Touching(transform.position, controller.radius, targetPosition, targetRadius, charge.contactPadding))
                return;

            var info = new DamageInfo
            {
                Amount = charge.damage,
                KnockbackForce = charge.knockbackForce,
                Direction = ChargeMath.KnockbackDirection(chargeDirection, targetPosition - transform.position),
                Point = target.Center,
                Source = gameObject,
                HitStop = charge.hitStop,
            };
            // Zablokowane trafienie (np. nietykalność dasha) nie zużywa szarży — może jeszcze trafić.
            if (target.ReceiveHit(info)) hitThisCharge = true;
        }

        void EndCharge(State next)
        {
            IgnoreTarget(false);
            state = next;
            timer = 0f;
        }

        void Rest(float duration)
        {
            timer += Time.deltaTime;
            controller.SimpleMove(Vector3.zero);
            if (timer >= duration) state = State.Chase;
        }

        void HandleDied(DamageInfo info)
        {
            state = State.Dead;
            if (telegraph != null) telegraph.Hide();
            IgnoreTarget(false);
            controller.enabled = false;
            // Pada na plecy: oś „w górę” bryły kładzie się do tyłu (szara bryła do M10).
            Vector3 back = -Flat(transform.forward).normalized;
            transform.rotation = Quaternion.LookRotation(Vector3.up, back);
            OsiedleLog.Info($"{name}: znokautowany. R — restart.");
        }

        void IgnoreTarget(bool ignore)
        {
            if (controller != null && targetController != null)
                Physics.IgnoreCollision(controller, targetController, ignore);
        }

        void UpdateBat()
        {
            if (batPivot == null) return;
            float pitch = state switch
            {
                State.Windup => Mathf.Lerp(BatRestPitch, BatRaisedPitch, timer / charge.windupTime),
                State.Charge => BatChargePitch,
                State.Stunned => BatStunnedPitch,
                _ => BatRestPitch,
            };
            batPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
