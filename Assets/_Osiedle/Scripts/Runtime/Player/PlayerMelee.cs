using Osiedle.Combat;
using Osiedle.Core;
using Osiedle.Weapons;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Atak wręcz (PPM): seria do 3 ciosów z danych broni. Każdy cios ma zamach, moment trafienia i powrót.
    /// Trafienie wybija Złom (śrubki) i ładuje Moc. Dash przerywa cios („wejdź, uderz, odskocz”).
    /// </summary>
    [DefaultExecutionOrder(5)]
    public class PlayerMelee : MonoBehaviour
    {
        [SerializeField] PlayerData playerData;
        [SerializeField] MeleeWeaponData weapon;
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerDash dash;
        [SerializeField] PlayerResources resources;
        [SerializeField] Hitbox hitbox;
        [SerializeField] ScrapSpawner scrapSpawner;

        readonly System.Random rng = new System.Random();

        InputBuffer buffer;
        ComboCounter combo;
        MeleePhase phase;
        int stepIndex;
        float phaseTimer;
        MeleeComboStep step;

        public bool IsAttacking => phase != MeleePhase.Idle;

        /// <summary>Numer trwającego ciosu serii (0 = pierwszy), -1 gdy nie atakuje.</summary>
        public int CurrentStep => IsAttacking ? stepIndex : -1;

        public MeleeWeaponData Weapon => weapon;

        public MeleePhase Phase => phase;

        /// <summary>Czy trwający cios jest ostatnim w serii (np. inny zamach).</summary>
        public bool IsFinisher => IsAttacking && weapon != null && stepIndex == weapon.combo.Length - 1;

        /// <summary>Postęp bieżącej fazy ciosu, 0..1 (dla wyglądu zamachu).</summary>
        public float PhaseProgress
        {
            get
            {
                float duration = phase switch
                {
                    MeleePhase.Windup => step.windup,
                    MeleePhase.Active => step.active,
                    MeleePhase.Recovery => step.recovery,
                    _ => 0f,
                };
                return duration > 0f ? Mathf.Clamp01(phaseTimer / duration) : 1f;
            }
        }

        void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (dash == null) dash = GetComponent<PlayerDash>();
            if (resources == null) resources = GetComponent<PlayerResources>();
            if (playerData == null || weapon == null || hitbox == null)
            {
                OsiedleLog.Error("PlayerMelee: brak PlayerData, broni albo Hitboxa.", this);
                enabled = false;
                return;
            }

            buffer = new InputBuffer(playerData.inputBufferTime);
            combo = new ComboCounter(weapon.combo.Length, weapon.comboResetTime);
        }

        void OnEnable()
        {
            if (input != null) input.MeleePressed += RequestAttack;
            if (hitbox != null) hitbox.HitLanded += HandleHitLanded;
            GameEvents.OnDash += HandleDash;
            GameEvents.OnVault += HandleDash;
        }

        void OnDisable()
        {
            if (input != null) input.MeleePressed -= RequestAttack;
            if (hitbox != null) hitbox.HitLanded -= HandleHitLanded;
            GameEvents.OnDash -= HandleDash;
            GameEvents.OnVault -= HandleDash;
            Cancel();
        }

        /// <summary>Prośba o cios (przez bufor wejścia). Wywołuje ją przycisk, a także testy.</summary>
        public void RequestAttack() => buffer?.Press(Time.time);

        void Update()
        {
            // Dane czytamy co klatkę, żeby strojenie w Play Mode działało od razu.
            buffer.Window = playerData.inputBufferTime;
            combo.SetLength(weapon.combo.Length);
            combo.ResetTime = weapon.comboResetTime;

            if (phase != MeleePhase.Idle) Tick();

            bool dashing = dash != null && dash.IsBusy;
            if (phase == MeleePhase.Idle && !dashing && weapon.combo.Length > 0 && buffer.TryConsume(Time.time))
                BeginStep();
        }

        void BeginStep()
        {
            stepIndex = combo.Next(Time.time);
            step = weapon.combo[stepIndex];
            phase = MeleePhase.Windup;
            phaseTimer = 0f;
            motor.SpeedMultiplier = weapon.moveSpeedMultiplier;
        }

        void Tick()
        {
            phaseTimer += Time.deltaTime;

            switch (phase)
            {
                case MeleePhase.Windup:
                    if (phaseTimer < step.windup) return;
                    phaseTimer -= step.windup;
                    phase = MeleePhase.Active;
                    hitbox.Activate(BuildDamage(), weapon.range, weapon.arcDegrees);
                    break;

                case MeleePhase.Active:
                    Lunge();
                    if (phaseTimer < step.active) return;
                    phaseTimer -= step.active;
                    hitbox.Deactivate();
                    phase = MeleePhase.Recovery;
                    break;

                case MeleePhase.Recovery:
                    if (phaseTimer < step.recovery) return;
                    Finish();
                    break;
            }
        }

        void Lunge()
        {
            if (step.lunge <= 0f || !motor.Controller.enabled) return;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float speed = step.lunge / step.active;
            motor.Controller.Move(forward * (speed * Time.deltaTime));
        }

        DamageInfo BuildDamage()
        {
            return new DamageInfo
            {
                Amount = weapon.damage * step.damageMultiplier,
                Element = weapon.element,
                KnockbackForce = step.knockbackForce,
                HitStop = step.hitStop,
                Source = gameObject,
            };
        }

        void Finish()
        {
            phase = MeleePhase.Idle;
            combo.EndStep(Time.time);
            motor.SpeedMultiplier = 1f;
        }

        /// <summary>Przerywa cios (np. dashem). Seria trwa dalej, jeśli następny cios przyjdzie w oknie.</summary>
        void Cancel()
        {
            if (phase == MeleePhase.Idle) return;
            hitbox.Deactivate();
            Finish();
        }

        void HandleDash(Vector3 _) => Cancel();

        void HandleHitLanded(Hurtbox target, DamageInfo info)
        {
            if (resources != null) resources.AddPower(weapon.powerPerHit);
            if (scrapSpawner == null) return;

            int count = ScrapRoll.Count(weapon.scrapPerHitMin, weapon.scrapPerHitMax, rng);
            scrapSpawner.Spawn(info.Point, count, target.Owner.transform.position.y);
        }
    }
}
