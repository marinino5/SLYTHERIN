using UnityEngine;

namespace Slytherin.Lighting
{
    /// <summary>
    /// Farola controlable. Maneja el Light component, el material emisivo de la bombilla
    /// y opcionalmente un audio al conmutar. La invoca el Deluminator (Apagador).
    ///
    /// Cómo configurarla en la escena:
    /// 1. Pon este script en el GameObject raíz del prefab del poste (posteA).
    /// 2. Asigna 'lightSource' al Light hijo (Point Light o Spot Light).
    /// 3. Asigna 'bulbRenderer' al MeshRenderer de la bombilla (el material amarillo_farol).
    /// 4. (Opcional) Asigna 'onMaterial' y 'offMaterial' si quieres swap visual.
    /// 5. Tag recomendado: "StreetLight".
    /// </summary>
    [DisallowMultipleComponent]
    public class StreetLight : MonoBehaviour
    {
        [Header("Estado inicial")]
        [SerializeField] private bool startsOn = true;

        [Header("Referencias visuales")]
        [Tooltip("Componente Light del foco (Point o Spot). Se enciende/apaga con SetActive en el GameObject del Light o cambiando enabled.")]
        [SerializeField] private Light lightSource;

        [Tooltip("Renderer de la bombilla. Si está asignado, se cambia su color emisivo.")]
        [SerializeField] private Renderer bulbRenderer;

        [Header("Emisivo (opcional)")]
        [SerializeField] private Color emissiveOn = new Color(1f, 0.85f, 0.4f) * 2f;
        [SerializeField] private Color emissiveOff = Color.black;

        [Header("Materiales swap (opcional, alternativa al emisivo)")]
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;

        [Header("Audio (opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleClip;

        // Estado actual de la farola.
        public bool IsOn { get; private set; }

        // Propiedad para que otros scripts (ej. PlayerStealth) sepan si esta luz proyecta zona iluminada.
        public Vector3 LightPosition => lightSource != null ? lightSource.transform.position : transform.position;
        public float LightRange => lightSource != null ? lightSource.range : 0f;

        private MaterialPropertyBlock _mpb;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            ApplyState(startsOn, instant: true);
        }

        /// <summary>Conmuta el estado actual.</summary>
        public void Toggle()
        {
            ApplyState(!IsOn, instant: false);
        }

        /// <summary>Forzar encendido.</summary>
        public void TurnOn()  => ApplyState(true,  instant: false);

        /// <summary>Forzar apagado.</summary>
        public void TurnOff() => ApplyState(false, instant: false);

        private void ApplyState(bool on, bool instant)
        {
            IsOn = on;

            // 1) Light component
            if (lightSource != null)
                lightSource.enabled = on;

            // 2) Visual de la bombilla
            if (bulbRenderer != null)
            {
                if (onMaterial != null && offMaterial != null)
                {
                    // Modo swap de material
                    bulbRenderer.sharedMaterial = on ? onMaterial : offMaterial;
                }
                else
                {
                    // Modo emisivo via MaterialPropertyBlock (no instancia el material -> mejor rendimiento)
                    bulbRenderer.GetPropertyBlock(_mpb);
                    _mpb.SetColor(EmissionColorID, on ? emissiveOn : emissiveOff);
                    bulbRenderer.SetPropertyBlock(_mpb);
                }
            }

            // 3) Audio
            if (!instant && audioSource != null && toggleClip != null)
                audioSource.PlayOneShot(toggleClip);
        }

        // Gizmo visual en el editor: dibuja una esfera del rango de la luz.
        private void OnDrawGizmosSelected()
        {
            if (lightSource == null) return;
            Gizmos.color = IsOn ? new Color(1f, 0.85f, 0.4f, 0.25f) : new Color(0.2f, 0.2f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(lightSource.transform.position, lightSource.range);
        }
    }
}
