using UnityEngine;

namespace Slytherin.Player
{
    /// <summary>
    /// Controlador del personaje (Albus) usando CharacterController.
    /// - Movimiento WASD relativo a la cámara
    /// - Salto con gravedad propia
    /// - Sprint (Shift) con multiplicador de velocidad
    /// - Rotación del personaje hacia donde se mueve
    ///
    /// Configuración:
    /// 1. Pon este script en el GameObject del jugador (Albus).
    /// 2. Añade un componente CharacterController (se añade solo por RequireComponent).
    /// 3. Asigna 'cameraTransform' a Main Camera (o al rig de cámara orbital).
    /// 4. Tag del jugador: "Player".
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Velocidades")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Salto y gravedad")]
        [SerializeField] private float jumpHeight = 1.6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedExtraGravity = -2f;

        [Header("Referencias")]
        [SerializeField] private Transform cameraTransform;

        [Header("Inputs (legacy Input Manager)")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        private CharacterController _cc;
        private Vector3 _velocity;
        private bool _isSprinting;

        public bool IsGrounded => _cc.isGrounded;
        public bool IsSprinting => _isSprinting;
        public Vector3 CurrentVelocity => _velocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            // --- Entrada WASD ---
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _isSprinting = Input.GetKey(sprintKey);
            float speed = _isSprinting ? sprintSpeed : walkSpeed;

            // Dirección relativa a la cámara, proyectada en el plano horizontal
            Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 camRight   = cameraTransform != null ? cameraTransform.right   : Vector3.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h).normalized;
            Vector3 horizontal = moveDir * speed;

            // --- Rotación hacia el movimiento ---
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
            }

            // --- Gravedad y salto ---
            if (_cc.isGrounded)
            {
                if (_velocity.y < 0f) _velocity.y = groundedExtraGravity;
                if (Input.GetKeyDown(jumpKey))
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _velocity.y += gravity * Time.deltaTime;

            // --- Aplicar movimiento ---
            Vector3 finalMove = horizontal + new Vector3(0f, _velocity.y, 0f);
            _cc.Move(finalMove * Time.deltaTime);
        }
    }
}
