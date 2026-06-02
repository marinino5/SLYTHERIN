using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slytherin.Lighting
{
    [RequireComponent(typeof(SphereCollider))]
    public class Deluminator : MonoBehaviour
    {
        [Header("Interacción")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float detectionRadius = 2.5f;

        [Header("Tiempo")]
        [SerializeField] private float lightsOffDuration = 6f;
        [SerializeField] private float cooldown = 20f;

        [Header("Farolas controladas")]
        [SerializeField] private List<StreetLight> controlledLights = new();
        [SerializeField] private bool autoFindLightsByRadius = true;
        [SerializeField] private float lightSearchRadius = 12f;

        [Header("Visual")]
        [SerializeField] private Renderer crystalRenderer;
        [SerializeField] private Color crystalIdle = new Color(0.2f, 0.8f, 0.4f, 1f);
        [SerializeField] private Color crystalReady = new Color(0.4f, 1f, 0.6f, 1f) * 3f;
        [SerializeField] private Color crystalCooldown = new Color(0.1f, 0.1f, 0.1f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip useClip;

        private SphereCollider triggerCollider;
        private bool playerInRange;
        private bool isOnCooldown;
        private bool isActive;
        private MaterialPropertyBlock mpb;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            triggerCollider = GetComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = detectionRadius;

            mpb = new MaterialPropertyBlock();

            if (autoFindLightsByRadius && controlledLights.Count == 0)
            {
                AutoFindLights();
            }

            SetCrystalColor(crystalIdle);
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                TryUseDeluminator();
            }
        }

        private void AutoFindLights()
        {
            controlledLights.Clear();

            StreetLight[] allLights = Object.FindObjectsByType<StreetLight>(FindObjectsSortMode.None);

            foreach (StreetLight streetLight in allLights)
            {
                if (Vector3.Distance(streetLight.transform.position, transform.position) <= lightSearchRadius)
                {
                    controlledLights.Add(streetLight);
                }
            }
        }

        private void TryUseDeluminator()
        {
            if (isOnCooldown || isActive)
            {
                return;
            }

            StartCoroutine(TemporaryLightsOff());
        }

        private IEnumerator TemporaryLightsOff()
        {
            isActive = true;
            isOnCooldown = true;

            SetCrystalColor(crystalCooldown);

            foreach (StreetLight streetLight in controlledLights)
            {
                if (streetLight != null)
                {
                    streetLight.TurnOff();
                }
            }

            if (audioSource != null && useClip != null)
            {
                audioSource.PlayOneShot(useClip);
            }

            yield return new WaitForSeconds(lightsOffDuration);

            foreach (StreetLight streetLight in controlledLights)
            {
                if (streetLight != null)
                {
                    streetLight.TurnOn();
                }
            }

            isActive = false;

            yield return new WaitForSeconds(cooldown);

            isOnCooldown = false;

            if (playerInRange)
            {
                SetCrystalColor(crystalReady);
            }
            else
            {
                SetCrystalColor(crystalIdle);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = true;

            if (!isOnCooldown && !isActive)
            {
                SetCrystalColor(crystalReady);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = false;

            if (!isOnCooldown && !isActive)
            {
                SetCrystalColor(crystalIdle);
            }
        }

        private void SetCrystalColor(Color color)
        {
            if (crystalRenderer == null) return;

            crystalRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorID, color);
            crystalRenderer.SetPropertyBlock(mpb);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            if (autoFindLightsByRadius)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.4f, 0.15f);
                Gizmos.DrawWireSphere(transform.position, lightSearchRadius);
            }

            if (controlledLights == null) return;

            Gizmos.color = new Color(1f, 0.85f, 0.4f, 0.9f);
            foreach (StreetLight light in controlledLights)
            {
                if (light != null)
                {
                    Gizmos.DrawLine(transform.position, light.transform.position);
                }
            }
        }
    }
}