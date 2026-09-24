using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Player
{
    /// <summary>
    /// Ruch postaci na CharacterController: 8 kierunków względem kamery, bez przyspieszenia,
    /// grawitacja i coyote time przy krawędziach. Dash i skok przejmują ruch przez <see cref="SetLocked"/>.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(10)]
    public class PlayerMotor : MonoBehaviour
    {
        const float MinDirectionSqr = 0.001f;

        [SerializeField] PlayerData data;
        [SerializeField] PlayerInputReader input;

        CharacterController controller;
        Transform view;
        float verticalSpeed;
        float lastGroundedTime = float.NegativeInfinity;
        bool locked;

        public CharacterController Controller => controller;
        public bool IsGrounded { get; private set; }

        /// <summary>Mnożnik prędkości biegu (np. wolniej w trakcie ciosu). 1 = normalnie.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>Kierunek ruchu z wejścia w świecie, względem kamery (płaski, długość 0..1).</summary>
        public Vector3 MoveDirection { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (data == null)
            {
                OsiedleLog.Error("PlayerMotor: brak PlayerData.", this);
                enabled = false;
                return;
            }

            var cam = Camera.main;
            view = cam != null ? cam.transform : null;
        }

        /// <summary>Blokuje zwykły ruch (np. na czas dasha). Po odblokowaniu grawitacja liczy się od zera.</summary>
        public void SetLocked(bool value)
        {
            locked = value;
            verticalSpeed = 0f;
        }

        void Update()
        {
            MoveDirection = ReadMoveDirection();
            if (locked || !controller.enabled) return;

            if (controller.isGrounded)
            {
                lastGroundedTime = Time.time;
                verticalSpeed = -data.groundStickSpeed;
            }
            else if (Time.time - lastGroundedTime <= data.coyoteTime)
            {
                // Coyote time: chwilę po zejściu z krawędzi jeszcze nie spadamy.
                verticalSpeed = 0f;
            }
            else
            {
                verticalSpeed = Mathf.Max(verticalSpeed - data.gravity * Time.deltaTime, -data.maxFallSpeed);
            }

            Vector3 velocity = MoveDirection * (data.moveSpeed * SpeedMultiplier);
            velocity.y = verticalSpeed;
            controller.Move(velocity * Time.deltaTime);

            IsGrounded = controller.isGrounded;
            if (IsGrounded) lastGroundedTime = Time.time;
        }

        Vector3 ReadMoveDirection()
        {
            Vector2 raw = input != null ? Vector2.ClampMagnitude(input.Move, 1f) : Vector2.zero;

            Vector3 forward = Vector3.forward;
            if (view != null)
            {
                Vector3 flat = Vector3.ProjectOnPlane(view.forward, Vector3.up);
                if (flat.sqrMagnitude > MinDirectionSqr) forward = flat.normalized;
            }
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            return forward * raw.y + right * raw.x;
        }
    }
}
