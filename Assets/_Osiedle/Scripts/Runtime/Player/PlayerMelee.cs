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
        enum Phase { Idle, Windup, Active, Recovery }

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
        Phase phase;
        int stepIndex;
        float phaseTimer;
        MeleeComboStep step;

        public bool IsAttacking => phase != Phase.Idle;

        /// <summary>Numer trwającego ciosu serii (0 = pierwszy), -1 gdy nie atakuje.</summary>
        public int CurrentStep => IsAttacking ? stepIndex : -1;

        public MeleeWeaponData Weapon => weapon;

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

            if (phase != Phase.Idle) Tick();

            bool dashing = dash != null && dash.IsBusy;
            if (phase == Phase.Idle && !dashing && weapon.combo.Length > 0 && buffer.TryConsume(Time.time))
                BeginStep();
        }

        void BeginStep()
        {
            stepIndex = combo.Next(Time.time);
            step = weapon.combo[stepIndex];
            phase = Phase.Windup;
            phaseTimer = 0f;
            motor.SpeedMultiplier = weapon.moveSpeedMultiplier;
        }

        void Tick()
        {
            phaseTimer += Time.deltaTime;

            switch (phase)
            {
                case Phase.Windup:
                    if (phaseTimer < step.windup) return;
                    phaseTimer -= step.windup;
                    phase = Phase.Active;
                    hitbox.Activate(BuildDamage(), weapon.range, weapon.arcDegrees);
                    break;

                case Phase.Active:
                    Lunge();
                    if (phaseTimer < step.active) return;
                    phaseTimer -= step.active;
                    hitbox.Deactivate();
                    phase = Phase.Recovery;
                    break;

                case Phase.Recovery:
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
            phase = Phase.Idle;
            combo.EndStep(Time.time);
            motor.SpeedMultiplier = 1f;
        }

        /// <summary>Przerywa cios (np. dashem). Seria trwa dalej, jeśli następny cios przyjdzie w oknie.</summary>
        void Cancel()
        {
            if (phase == Phase.Idle) return;
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
