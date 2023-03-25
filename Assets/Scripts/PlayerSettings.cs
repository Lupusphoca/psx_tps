namespace PSX_VerticalSlice
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerSettings : MonoBehaviour
    {
        // #region Singleton Pattern
        // public static PlayerSettings Instance { get; private set; }

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

        #region Required Components
        [Header("Required Components")]
        [SerializeField] internal PlayerInput playerInput;
        [SerializeField] internal PlayerInputValues piv;
        [SerializeField] internal Animator animator;
        [SerializeField] internal CharacterController characterController;
        [SerializeField] internal GameObject mainCamera;
        [SerializeField] internal Transform transformPlayer;
        #endregion

        [Space(25)]

        #region Player
        [Header("Player")]

        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] internal float moveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] internal float sprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [SerializeField] internal float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        [SerializeField] internal float speedChangeRate = 10.0f;

        [SerializeField] internal AudioClip landingSound;
        [SerializeField] internal AudioClip[] footstepSounds;

        [Range(0, 1)]
        [SerializeField] internal float footstepAudioVolume = 0.5f;

        [Space(25)]

        [Tooltip("The height the player can jump")]
        [SerializeField] internal float jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] internal float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] internal float jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField] internal float fallTimeout = 0.15f;
        #endregion

        #region Player Grounded
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] internal bool grounded = true;

        [Tooltip("Useful for rough ground")]
        [SerializeField] internal float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] internal float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        [SerializeField] internal LayerMask groundLayers;
        #endregion

        #region Cinemachine
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] internal GameObject cinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] internal float topClamp = 80.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] internal float bottomClamp = -80.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        [SerializeField] internal float cameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        [SerializeField] internal bool lockCameraPosition = false;

        [SerializeField] internal float normalSensitivity = 1f;
        [SerializeField] internal float aimSensitivity = 0.5f;
        internal float Sensitivity {
            get {
                if (piv.aim) { return aimSensitivity; }
                else { return normalSensitivity; }
            }
        }
        #endregion

        // Cinemachine
        internal float cinemachineTargetYaw;
        internal float cinemachineTargetPitch;

        internal float cameraRotationThreshold = 0.01f;

        // Player
        internal float speed;
        internal float animationBlend;
        internal float targetRotation = 0.0f;
        internal float rotationVelocity;
        internal float verticalVelocity;
        internal float terminalVelocity = 53.0f;

        // TimeOut DeltaTime
        internal float jumpTimeoutDelta;
        internal float fallTimeoutDelta;

        // Animation IDs
        internal int animIDSpeed;
        internal int animIDGrounded;
        internal int animIDJump;
        internal int animIDFreeFall;
        internal int animIDMotionSpeed;
        internal int animIDAim;

        // States
        internal bool hasAnimator;
        internal bool IsCurrentDeviceMouse
        {
            get
            {
                return playerInput.currentControlScheme == "KeyboardMouse" ? true : false;
            }
        }
    }
}