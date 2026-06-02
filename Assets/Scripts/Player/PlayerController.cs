using UnityEngine;

namespace Slytherin.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Velocidades")]
        [SerializeField] private float walkSpeed = 180f;
        [SerializeField] private float sprintSpeed = 300f;
        [SerializeField] private float rotationSpeed = 8f;

        [Header("Salto y gravedad")]
        [SerializeField] private float jumpHeight = 35f;
        [SerializeField] private float gravity = -180f;
        [SerializeField] private float groundedExtraGravity = -10f;

        [Header("Inputs")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sneakKey = KeyCode.LeftControl;

        [Header("Animaciones normales")]
        [SerializeField] private string idleAnimation = "Idle";
        [SerializeField] private string walkAnimation = "Walk";
        [SerializeField] private string runAnimation = "Running";
        [SerializeField] private string jumpAnimation = "Jump";
        [SerializeField] private string sneakAnimation = "SneakWalk";

        [Header("Animaciones cargando mantas")]
        [SerializeField] private string carryIdleAnimation = "Idle";
        [SerializeField] private string carryWalkAnimation = "Walk";
        [SerializeField] private string carryRunAnimation = "Running";
        private CharacterController controller;
        private Animator animator;
        private Vector3 velocity;
        private bool isSprinting;
        private bool isCarrying;
        private string currentAnimation = "";

        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsSprinting => isSprinting;
        public Vector3 CurrentVelocity => velocity;
        public bool IsCarrying => isCarrying;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("No se encontró Animator en el jugador o sus hijos.");
            }
        }

        private void Update()
        {
            MovePlayer();
            UpdateAnimations();
        }

        public void SetCarrying(bool value)
        {
            isCarrying = value;
        }

        private void MovePlayer()
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            bool isSneaking = Input.GetKey(sneakKey);
            isSprinting = Input.GetKey(sprintKey) && !isSneaking;

            float currentSpeed = walkSpeed;

            if (isSprinting)
            {
                currentSpeed = sprintSpeed;
            }

            if (isSneaking)
            {
                currentSpeed = walkSpeed * 0.45f;
            }

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                float turnAmount = horizontalInput * rotationSpeed * 10f * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            Vector3 moveDirection = -transform.forward * verticalInput;
            Vector3 horizontalMovement = moveDirection * currentSpeed;

            if (controller.isGrounded)
            {
                if (velocity.y < 0f)
                {
                    velocity.y = groundedExtraGravity;
                }

                if (Input.GetKeyDown(jumpKey))
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            velocity.y += gravity * Time.deltaTime;

            Vector3 finalMovement = horizontalMovement + new Vector3(0f, velocity.y, 0f);
            controller.Move(finalMovement * Time.deltaTime);
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            float verticalInput = Input.GetAxisRaw("Vertical");
            bool isMoving = Mathf.Abs(verticalInput) > 0.01f;
            bool isSneaking = Input.GetKey(sneakKey);
            bool isJumping = !controller.isGrounded;

            if (isJumping)
            {
                PlayAnimation(jumpAnimation);
                return;
            }

            if (!isMoving)
            {
                PlayAnimation(isCarrying ? carryIdleAnimation : idleAnimation);
                return;
            }

            if (isSneaking && !isCarrying)
            {
                PlayAnimation(sneakAnimation);
                return;
            }

            if (isSprinting)
            {
                PlayAnimation(isCarrying ? carryRunAnimation : runAnimation);
                return;
            }

            PlayAnimation(isCarrying ? carryWalkAnimation : walkAnimation);
        }

        private void PlayAnimation(string animationName)
        {
            if (animator == null) return;
            if (currentAnimation == animationName) return;

            currentAnimation = animationName;
            animator.CrossFade(animationName, 0.15f);
        }
    }
}