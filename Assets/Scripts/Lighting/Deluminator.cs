using System.Collections.Generic;
using UnityEngine;

namespace Slytherin.Lighting
{
    /// <summary>
    /// EL APAGADOR (Deluminator) — pieza central de la mecánica de sigilo del nivel.
    ///
    /// Funcionamiento:
    /// - Detecta la cercanía del jugador con un SphereCollider en modo Trigger.
    /// - Al entrar el jugador en el radio, hace TOGGLE de todas las farolas que controla.
    /// - Soporta dos modos: AUTO (toggle al entrar al trigger) o INTERACT (toggle al pulsar tecla).
    /// - Opcionalmente sirve como CHECKPOINT (guarda posición de respawn).
    /// - Tiene cooldown para evitar conmutar muchas veces seguidas si el jugador entra/sale rápido.
    /// - Las farolas controladas se pueden asignar manualmente o buscarse automáticamente por radio.
    ///
    /// Cómo configurarlo en la escena:
    /// 1. Pon este script sobre el prefab del apagador (apagador.fbx).
    /// 2. Asegúrate de que el GameObject tiene un SphereCollider con IsTrigger = true.
    ///    (Si no, este script crea uno en Awake con 'autoDetectionRadius').
    /// 3. El jugador debe tener Tag = "Player".
    /// 4a. Opción A — asigna manualmente las farolas a 'controlledLights'.
    /// 4b. Opción B — deja vacío y activa 'autoFindLightsByRadius' con 'lightSearchRadius'.
    /// 5. Activa 'isCheckpoint' si quieres que sea punto de respawn.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Deluminator : MonoBehaviour
    {
        public enum ActivationMode
        {
            Auto,       // Al entrar al trigger se hace toggle automáticamente
            Interact    // Al entrar al trigger se muestra prompt y el jugador pulsa una tecla
        }

        [Header("Modo de activación")]
        [SerializeField] private ActivationMode mode = ActivationMode.Auto;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float detectionRadius = 2.5f;
        [Tooltip("Tiempo mínimo entre activaciones para evitar parpadeo.")]
        [SerializeField] private float cooldown = 0.5f;

        [Header("Farolas controladas")]
        [Tooltip("Si está vacío y autoFindLightsByRadius=true, busca automáticamente todas las StreetLight dentro de lightSearchRadius.")]
        [SerializeField] private List<StreetLight> controlledLights = new();
        [SerializeField] private bool autoFindLightsByRadius = true;
        [SerializeField] private float lightSearchRadius = 12f;

        [Header("Checkpoint (opcional)")]
        [SerializeField] private bool isCheckpoint = false;
        [Tooltip("Offset respecto al apagador para el punto de respawn.")]
        [SerializeField] private Vector3 respawnOffset = new Vector3(0f, 0f, -1f);

        [Header("Visual cuando el jugador está en rango")]
        [SerializeField] private Renderer crystalRenderer;
        [SerializeField] private Color crystalIdle = new Color(0.2f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color crystalReady = new Color(0.4f, 1f, 0.6f, 1f) * 3f;

        [Header("Audio (opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip useClip;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        // ---- Estado interno ----
        private SphereCollider _trigger;
        private bool _playerInRange;
        private bool _checkpointTaken;
        private float _lastActivationTime = -999f;
        private Transform _playerTransform;
        private MaterialPropertyBlock _mpb;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        // Evento que otros sistemas pueden escuchar (ej. UI prompt "Pulsa E").
        public System.Action<bool> OnPlayerInRangeChanged;
        public System.Action OnActivated;

        private void Awake()
        {
            _trigger = GetComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = detectionRadius;

            _mpb = new MaterialPropertyBlock();
            SetCrystalColor(crystalIdle);

            // Auto-buscar farolas si la lista está vacía
            if (autoFindLightsByRadius && controlledLights.Count == 0)
                AutoFindLights();
        }

        private void AutoFindLights()
        {
            controlledLights.Clear();
            // FindObjectsByType es más rápido y la API recomendada en Unity 6
            StreetLight[] all = Object.FindObjectsByType<StreetLight>(FindObjectsSortMode.None);
            foreach (var sl in all)
            {
                if (Vector3.Distance(sl.transform.position, transform.position) <= lightSearchRadius)
                    controlledLights.Add(sl);
            }
        }

        private void Update()
        {
            // Modo INTERACT: si el jugador está dentro y pulsa la tecla, activa
            if (mode == ActivationMode.Interact && _playerInRange && Input.GetKeyDown(interactKey))
                TryActivate();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = true;
            _playerTransform = other.transform;
            SetCrystalColor(crystalReady);
            OnPlayerInRangeChanged?.Invoke(true);

            // Marcar checkpoint la primera vez que se toca
            if (isCheckpoint && !_checkpointTaken)
            {
                _checkpointTaken = true;
                Vector3 respawnPos = transform.position + respawnOffset;
                Managers.GameManager.Instance?.SetCheckpoint(respawnPos);
            }

            // Modo AUTO: toggle inmediato al entrar
            if (mode == ActivationMode.Auto)
                TryActivate();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            _playerTransform = null;
            SetCrystalColor(crystalIdle);
            OnPlayerInRangeChanged?.Invoke(false);
        }

        /// <summary>Ejecuta el toggle si el cooldown lo permite.</summary>
        private void TryActivate()
        {
            if (Time.time - _lastActivationTime < cooldown) return;
            _lastActivationTime = Time.time;

            // Toggle de todas las farolas conectadas
            foreach (var light in controlledLights)
            {
                if (light != null) light.Toggle();
            }

            // Feedback de audio
            if (audioSource != null && useClip != null)
                audioSource.PlayOneShot(useClip);

            OnActivated?.Invoke();
        }

        private void SetCrystalColor(Color c)
        {
            if (crystalRenderer == null) return;
            crystalRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorID, c);
            crystalRenderer.SetPropertyBlock(_mpb);
        }

        // ----------- Gizmos para visualizar en el editor -----------
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Radio de detección del jugador
            Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Radio de búsqueda de farolas
            if (autoFindLightsByRadius)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.4f, 0.15f);
                Gizmos.DrawWireSphere(transform.position, lightSearchRadius);
            }

            // Líneas hacia farolas conectadas
            Gizmos.color = new Color(1f, 0.85f, 0.4f, 0.9f);
            foreach (var l in controlledLights)
            {
                if (l != null) Gizmos.DrawLine(transform.position, l.transform.position);
            }

            // Punto de respawn si es checkpoint
            if (isCheckpoint)
            {
                Gizmos.color = new Color(0.3f, 0.6f, 1f, 1f);
                Gizmos.DrawWireCube(transform.position + respawnOffset, Vector3.one * 0.4f);
            }
        }
    }
}
