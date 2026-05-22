using UnityEngine;
using Slytherin.Player;

namespace Slytherin.Enemies
{
    /// <summary>
    /// PETUNIA — vigila desde la ventana con visión 360° (Zona 3 del nivel).
    /// - No se mueve.
    /// - Tiene un radio de visión circular completo.
    /// - Solo detecta al jugador si está en LUZ (es decir, si IsInLight = true en PlayerStealth).
    ///   Si las farolas cercanas están apagadas por el Apagador, el jugador es invisible para Petunia.
    /// - Línea de visión real (no atraviesa muros/objetos).
    /// </summary>
    public class WatcherEnemy : MonoBehaviour
    {
        [Header("Visión")]
        [SerializeField] private float detectionRadius = 7f;
        [SerializeField] private bool requiresPlayerInLight = true;
        [SerializeField] private bool requireLineOfSight = true;

        [Header("Daño")]
        [SerializeField] private int damageOnDetect = 1;
        [SerializeField] private float detectionCooldownSec = 1.0f;

        [Header("Visualización del cono de luz/vigilancia")]
        [SerializeField] private bool drawGizmos = true;

        private Transform _player;
        private PlayerHealth _playerHealth;
        private PlayerStealth _playerStealth;
        private float _nextDetectionTime;

        private void Start()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null) return;
            _player        = go.transform;
            _playerHealth  = go.GetComponent<PlayerHealth>();
            _playerStealth = go.GetComponent<PlayerStealth>();
        }

        private void Update()
        {
            if (_player == null || _playerHealth == null) return;
            if (Time.time < _nextDetectionTime) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > detectionRadius) return;

            // ¿Está en luz?
            if (requiresPlayerInLight && _playerStealth != null && !_playerStealth.IsInLight) return;

            // ¿Línea de visión libre?
            if (requireLineOfSight)
            {
                Vector3 from = transform.position;
                Vector3 to   = _player.position + Vector3.up * 1.0f;
                if (Physics.Linecast(from, to, out RaycastHit hit) && hit.transform != _player)
                    return; // hay un muro u objeto en medio
            }

            _nextDetectionTime = Time.time + detectionCooldownSec;
            _playerHealth.TakeDamage(damageOnDetect);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = new Color(1f, 0.1f, 0.3f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
