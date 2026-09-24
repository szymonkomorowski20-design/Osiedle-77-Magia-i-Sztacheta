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
        InputAction melee;
        InputAction ranged;

        /// <summary>Kierunek ruchu z klawiatury (WASD), długość 0..1.</summary>
        public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Pozycja kursora na ekranie (piksele).</summary>
        public Vector2 PointerScreenPosition => aim != null ? aim.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Czy LPM (strzał) jest trzymany.</summary>
        public bool RangedHeld => ranged != null && ranged.IsPressed();

        public event Action DashPressed;
        public event Action MeleePressed;
        public event Action RangedPressed;

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
            melee = map.FindAction(MeleeAction, true);
            ranged = map.FindAction(RangedAction, true);
        }

        void OnEnable()
        {
            if (map == null) return;
            dash.performed += HandleDash;
            melee.performed += HandleMelee;
            ranged.performed += HandleRanged;
            map.Enable();
        }

        void OnDisable()
        {
            if (map == null) return;
            dash.performed -= HandleDash;
            melee.performed -= HandleMelee;
            ranged.performed -= HandleRanged;
            map.Disable();
        }

        void HandleDash(InputAction.CallbackContext context) => DashPressed?.Invoke();
        void HandleMelee(InputAction.CallbackContext context) => MeleePressed?.Invoke();
        void HandleRanged(InputAction.CallbackContext context) => RangedPressed?.Invoke();
    }
}
