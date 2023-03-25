namespace PSX_VerticalSlice
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    using PierreARNAUDET.Core.Attributes;

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputValues : MonoBehaviour
    {
        // #region Singleton Pattern
        // public static PlayerInputValues Instance { get; private set; }

        // private void Awake()
        // {
        //     if (Instance != null && Instance != this)
        //     {
        //         Destroy(gameObject);
        //         return;
        //     }
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        // #endregion

        [Header("Character Input Values")]
        [ReadOnly, SerializeField] internal Vector2 move;
        [ReadOnly, SerializeField] internal Vector2 look;
        [ReadOnly, SerializeField] internal bool jump;
        [ReadOnly, SerializeField] internal bool sprint;
        [ReadOnly, SerializeField] internal bool aim;
        [ReadOnly, SerializeField] internal bool shoot;

        [Header("Movement Settings")]
        [ReadOnly, SerializeField] internal bool analogMovement;

        [Header("Mouse Cursor Settings")]
        [ReadOnly, SerializeField] internal bool cursorLocked = true;
        [ReadOnly, SerializeField] internal bool cursorInputForLook = true;

        #region Receiving Events
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }

        public void OnShoot(InputValue value)
        {
            ShootInput(value.isPressed);
        }
        #endregion

        #region Values 
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        public void ShootInput(bool newShootState)
        {
            shoot = newShootState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
        #endregion
    }
}