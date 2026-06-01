using UnityEngine;

namespace Slytherin.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Velocidades")]
        [SerializeField] private float walkSpeed = 400f;
        [SerializeField] private float sprintSpeed = 800f;
        [SerializeField] private float rotationSpeed = 8f;

        [Header("Salto y gravedad")]
        [SerializeField] private float jumpHeight = 100f;
        [SerializeField] private float gravity = -400f;
        [SerializeField] private float groundedExtraGravity = -15f;

        [Header("Inputs")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sneakKey = KeyCode.LeftControl;

        private CharacterController controller;
        private Animator animator;
        private Vector3 velocity;
        private bool isSprinting;
        private string currentAnimation = "";

        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsSprinting => isSprinting;
        public Vector3 CurrentVelocity => velocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // Busca el Animator en el modelo hijo, por ejemplo Severu_Idle_WithSkin
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

        private void MovePlayer()
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal"); // A / D
            float verticalInput = Input.GetAxisRaw("Vertical");     // W / S

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

            // A y D giran al jugador
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                float turnAmount = horizontalInput * rotationSpeed * 10f * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            // W y S mueven hacia adelante o atrás
            Vector3 moveDirection = -transform.forward * verticalInput;
            Vector3 horizontalMovement = moveDirection * currentSpeed;

            // Gravedad y salto
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
                PlayAnimation("Jump");
                return;
            }

            if (!isMoving)
            {
                PlayAnimation("Idle");
                return;
            }

            if (isSneaking)
            {
                PlayAnimation("SneakWalk");
                return;
            }

            if (isSprinting)
            {
                PlayAnimation("Running");
                return;
            }

            PlayAnimation("Walk");
        }

        private void PlayAnimation(string animationName)
        {
            if (currentAnimation == animationName) return;

            currentAnimation = animationName;
            animator.CrossFade(animationName, 0.15f);
        }
    }
}