using System;
using Osiedle.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Osiedle.Player
{
    /// <summary>
    /// Czyta sterowanie gracza z pliku akcji Input System (Data/Input/OsiedleControls)
    /// i udostępnia je pozostałym skryptom gracza.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        public const string MapName = "Player";
        public const string MoveAction = "Move";
        public const string AimAction = "Aim";
        public const string DashAction = "Dash";
        public const string MeleeAction = "Melee";
        public const string RangedAction = "Ranged";
        public const string SpellAction = "Spell";
        public const string InteractAction = "Interact";
        public const string ThrowAction = "Throw";

        [SerializeField] InputActionAsset actions;

        InputActionMap map;
        InputAction move;
        InputAction aim;
        InputAction dash;

        /// <summary>Kierunek ruchu z klawiatury (WASD), długość 0..1.</summary>
        public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Pozycja kursora na ekranie (piksele).</summary>
        public Vector2 PointerScreenPosition => aim != null ? aim.ReadValue<Vector2>() : Vector2.zero;

        public event Action DashPressed;

        void Awake()
        {
            if (actions == null)
            {
                OsiedleLog.Error("PlayerInputReader: brak pliku akcji wejścia. Uruchom Osiedle/Build/Test_Movement.", this);
                enabled = false;
                return;
            }

            map = actions.FindActionMap(MapName, true);
            move = map.FindAction(MoveAction, true);
            aim = map.FindAction(AimAction, true);
            dash = map.FindAction(DashAction, true);
        }

        void OnEnable()
        {
            if (map == null) return;
            dash.performed += HandleDash;
            map.Enable();
        }

        void OnDisable()
        {
            if (map == null) return;
            dash.performed -= HandleDash;
            map.Disable();
        }

        void HandleDash(InputAction.CallbackContext context) => DashPressed?.Invoke();
    }
}
