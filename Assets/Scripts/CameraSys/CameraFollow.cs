using UnityEngine;

namespace Slytherin.CameraSys
{
    /// <summary>
    /// Cámara orbital de tercera persona.
    /// - Sigue al target (jugador) con offset.
    /// - Rota alrededor del jugador con el mouse (botón derecho o siempre).
    /// - Hace zoom con la rueda.
    /// - Evita atravesar paredes con un raycast hacia el target.
    ///
    /// Configuración:
    /// 1. Pon este script en Main Camera (o en un GameObject vacío con la cámara como hijo).
    /// 2. Asigna 'target' = transform del jugador.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.6f, 0f);

        [Header("Órbita")]
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float rotationSpeed = 220f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Suavizado")]
        [SerializeField] private float positionSmooth = 12f;

        [Header("Colisión")]
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float collisionPadding = 0.2f;

        private float _yaw;
        private float _pitch = 15f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Input de cámara
            _yaw   += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            _pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            // Zoom con rueda
            distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            // Calcular posición deseada
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pivot = target.position + offset;
            Vector3 desiredPos = pivot - rot * Vector3.forward * distance;

            // Anti-atravesar paredes
            if (Physics.Linecast(pivot, desiredPos, out RaycastHit hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPos = hit.point + hit.normal * collisionPadding;
            }

            // Suavizado posición
            transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);
            transform.rotation = rot;
        }

        public void SetTarget(Transform t) => target = t;
    }
}
