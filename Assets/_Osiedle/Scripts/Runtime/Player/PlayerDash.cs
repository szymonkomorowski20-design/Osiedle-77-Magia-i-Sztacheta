using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Dash kontekstowy. W otwartym terenie: szybki unik z nieśmiertelnością.
    /// Gdy tuż przed postacią stoi obiekt z <see cref="Vaultable"/>, ten sam przycisk robi skok na niego.
    /// Kierunek: tam, gdzie gracz idzie; gdy stoi — tam, gdzie celuje.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    [DefaultExecutionOrder(0)]
    public class PlayerDash : MonoBehaviour
    {
        const float MinDirectionSqr = 0.01f;
        const float LandingClearance = 0.05f;

        enum State { Ready, Dashing, Vaulting }

        [SerializeField] PlayerData data;
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerAim aim;

        [Tooltip("Warstwy przeszkód: tu szukamy obiektów do wskoczenia i sprawdzamy miejsce do lądowania.")]
        [SerializeField] LayerMask obstacleMask = ~0;

        State state;
        DashCharges charges;
        InputBuffer buffer;
        float stateTimer;
        float invulnerableUntil = float.NegativeInfinity;
        Vector3 dashDirection;
        Vector3 vaultStart;
        Vector3 vaultEnd;

        /// <summary>Czy postać jest teraz nietykalna (używane przez Hurtbox od M1).</summary>
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        /// <summary>Czy trwa dash albo skok.</summary>
        public bool IsBusy => state != State.Ready;

        public DashCharges Charges => charges;

        void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (aim == null) aim = GetComponent<PlayerAim>();
            if (data == null)
            {
                OsiedleLog.Error("PlayerDash: brak PlayerData.", this);
                enabled = false;
                return;
            }

            charges = new DashCharges(data.dashCharges, data.dashCooldown);
            buffer = new InputBuffer(data.inputBufferTime);
        }

        void OnEnable()
        {
            if (input != null) input.DashPressed += OnDashPressed;
        }

        void OnDisable()
        {
            if (input != null) input.DashPressed -= OnDashPressed;
            if (state != State.Ready) Finish();
        }

        void OnDashPressed() => RequestDash();

        /// <summary>Prośba o dash (przez bufor wejścia). Wywołuje ją przycisk, a także testy.</summary>
        public void RequestDash() => buffer.Press(Time.time);

        void Update()
        {
            // Dane czytamy co klatkę, żeby strojenie w Play Mode działało od razu.
            charges.Configure(data.dashCharges, data.dashCooldown);
            buffer.Window = data.inputBufferTime;
            charges.Tick(Time.deltaTime);

            if (state == State.Dashing) UpdateDash();
            else if (state == State.Vaulting) UpdateVault();

            if (state == State.Ready && charges.Charges > 0 && buffer.TryConsume(Time.time))
            {
                charges.TryConsume();
                Begin();
            }
        }

        void Begin()
        {
            Vector3 direction = motor.MoveDirection;
            if (direction.sqrMagnitude > MinDirectionSqr) direction.Normalize();
            else direction = aim != null ? aim.AimDirection : transform.forward;

            dashDirection = direction;
            invulnerableUntil = Time.time + data.dashInvulnerability;
            stateTimer = 0f;
            motor.SetLocked(true);

            if (TryFindVaultLanding(direction, out Vector3 landing))
            {
                vaultStart = transform.position;
                vaultEnd = landing;
                // Na czas skoku wyłączamy kolizje kontrolera, żeby nie zaczepić o krawędź.
                motor.Controller.enabled = false;
                state = State.Vaulting;
                GameEvents.RaiseVault(landing);
            }
            else
            {
                state = State.Dashing;
                GameEvents.RaiseDash(direction);
            }
        }

        void UpdateDash()
        {
            float remaining = data.dashDuration - stateTimer;
            float step = Mathf.Min(Time.deltaTime, remaining);
            stateTimer += Time.deltaTime;

            float speed = data.dashDistance / data.dashDuration;
            motor.Controller.Move(dashDirection * (speed * step));

            if (stateTimer >= data.dashDuration) Finish();
        }

        void UpdateVault()
        {
            stateTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stateTimer / data.vaultDuration);

            // Wysokość rośnie szybciej niż ruch w poziomie (ease-out) + łuk, żeby nie przenikać przez krawędź.
            float easeOut = 1f - (1f - t) * (1f - t);
            Vector3 position = Vector3.Lerp(vaultStart, vaultEnd, t);
            position.y = Mathf.Lerp(vaultStart.y, vaultEnd.y, easeOut) + Mathf.Sin(t * Mathf.PI) * data.vaultArcHeight;
            transform.position = position;

            if (t >= 1f) Finish();
        }

        void Finish()
        {
            state = State.Ready;
            if (!motor.Controller.enabled) motor.Controller.enabled = true;
            motor.SetLocked(false);
        }

        /// <summary>
        /// Sprawdza, czy przed postacią jest obiekt do wskoczenia i czy na nim jest miejsce do lądowania.
        /// </summary>
        bool TryFindVaultLanding(Vector3 direction, out Vector3 landing)
        {
            landing = default;
            CharacterController cc = motor.Controller;
            Vector3 feet = transform.position;
            Vector3 origin = feet + Vector3.up * data.vaultProbeHeight;
            float distance = cc.radius + data.vaultDetectDistance;

            if (!Physics.SphereCast(origin, data.vaultProbeRadius, direction, out RaycastHit hit, distance,
                    obstacleMask, QueryTriggerInteraction.Ignore))
                return false;

            var vaultable = hit.collider.GetComponentInParent<Vaultable>();
            if (vaultable == null) return false;

            float rise = vaultable.TopHeight - feet.y;
            if (rise < data.vaultMinHeight || rise > data.vaultMaxHeight) return false;

            // Szukamy powierzchni lądowania z góry, trochę za krawędzią.
            Vector3 target = hit.point + direction * data.vaultLandingInset;
            float probeTop = vaultable.TopHeight + cc.height;
            var down = new Vector3(target.x, probeTop, target.z);
            if (!Physics.Raycast(down, Vector3.down, out RaycastHit ground, cc.height + data.vaultMaxHeight,
                    obstacleMask, QueryTriggerInteraction.Ignore))
                return false;

            if (Vector3.Angle(ground.normal, Vector3.up) > cc.slopeLimit) return false;
            if (ground.point.y - feet.y > data.vaultMaxHeight) return false;

            // Czy na miejscu lądowania zmieści się postać?
            Vector3 bottom = ground.point + Vector3.up * (cc.radius + LandingClearance);
            Vector3 top = ground.point + Vector3.up * Mathf.Max(cc.height - cc.radius, cc.radius + LandingClearance);
            if (Physics.CheckCapsule(bottom, top, cc.radius, obstacleMask, QueryTriggerInteraction.Ignore))
                return false;

            landing = ground.point;
            return true;
        }
    }
}
