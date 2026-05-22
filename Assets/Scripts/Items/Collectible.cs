using UnityEngine;

namespace Slytherin.Items
{
    /// <summary>
    /// Clase base para objetos recolectables (Caramelo de limón, Reloj de oro).
    /// - Gira sobre su eje (efecto visual flotante).
    /// - Detecta al jugador por trigger.
    /// - Cuando lo recoge, llama a OnCollected() (a sobrescribir) y se destruye.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Collectible : MonoBehaviour
    {
        [Header("Animación flotante")]
        [SerializeField] protected float rotationSpeed = 90f;
        [SerializeField] protected float bobAmplitude = 0.15f;
        [SerializeField] protected float bobFrequency = 1.5f;

        [Header("Efectos al recoger")]
        [SerializeField] protected AudioClip pickupSound;
        [SerializeField] protected GameObject pickupVFX;

        private Vector3 _startPos;

        protected virtual void Awake()
        {
            // Forzar el collider a trigger
            GetComponent<Collider>().isTrigger = true;
            _startPos = transform.position;
        }

        protected virtual void Update()
        {
            // Rotación
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            // Bobbing
            float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = _startPos + new Vector3(0f, y, 0f);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            OnCollected(other);
            SpawnEffects();
            Destroy(gameObject);
        }

        /// <summary>Lógica concreta del coleccionable. Sobrescribir en cada subclase.</summary>
        protected abstract void OnCollected(Collider player);

        protected void SpawnEffects()
        {
            if (pickupVFX != null) Instantiate(pickupVFX, transform.position, Quaternion.identity);
            if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}
