using System.Collections.Generic;
using UnityEngine;
using Slytherin.Lighting;

namespace Slytherin.Player
{
    /// <summary>
    /// Detecta si el jugador está dentro del rango de alguna farola encendida.
    /// Es la "señal" que usan los enemigos: si IsInLight = true, son visibles para Petunia/Vernon.
    ///
    /// Recorre todas las StreetLight de la escena cada cierto tiempo (no cada frame, por rendimiento)
    /// y comprueba si la posición del jugador está dentro del radio de alguna que esté encendida.
    /// </summary>
    public class PlayerStealth : MonoBehaviour
    {
        [Header("Detección")]
        [Tooltip("Cada cuánto en segundos se reevalúa el estado de luz/sombra.")]
        [SerializeField] private float checkInterval = 0.15f;
        [Tooltip("Margen extra que se le suma al radio de la farola para considerar 'en luz'.")]
        [SerializeField] private float lightBuffer = 0f;

        public bool IsInLight { get; private set; }
        public event System.Action<bool> OnStealthStateChanged;

        private float _nextCheckTime;
        private readonly List<StreetLight> _lightsCache = new();

        private void Start()
        {
            RefreshLightsCache();
        }

        /// <summary>Llamar si se instancian/destruyen farolas en runtime.</summary>
        public void RefreshLightsCache()
        {
            _lightsCache.Clear();
            _lightsCache.AddRange(Object.FindObjectsByType<StreetLight>(FindObjectsSortMode.None));
        }

        private void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + checkInterval;

            bool inLight = false;
            Vector3 pos = transform.position;

            foreach (var light in _lightsCache)
            {
                if (light == null || !light.IsOn) continue;
                float r = light.LightRange + lightBuffer;
                if ((light.LightPosition - pos).sqrMagnitude <= r * r)
                {
                    inLight = true;
                    break;
                }
            }

            if (inLight != IsInLight)
            {
                IsInLight = inLight;
                OnStealthStateChanged?.Invoke(IsInLight);
            }
        }
    }
}
