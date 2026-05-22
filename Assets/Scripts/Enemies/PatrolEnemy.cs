using UnityEngine;
using Slytherin.Player;

namespace Slytherin.Enemies
{
    /// <summary>
    /// VERNON — enemigo que patrulla entre dos puntos (A ↔ B).
    /// - Camina linealmente entre patrolPointA y patrolPointB.
    /// - Tiene un cono de visión hacia delante.
    /// - Si detecta al jugador (en luz o muy cerca), dispara daño/game over.
    /// - Si el jugador está en sombra (PlayerStealth.IsInLight == false), Vernon NO lo ve aunque esté en el cono,
    ///   salvo que esté muy cerca (proximityDetectionRadius).
    ///
    /// Usar para Vernon en la Zona 5 del nivel.
    /// </summary>
    public class PatrolEnemy : MonoBehaviour
    {
        [Header("Ruta de patrullaje")]
        [SerializeField] private Transform patrolPointA;
        [SerializeField] private Transform patrolPointB;
        [SerializeField] private float moveSpeed = 1.6f;
        [SerializeField] private float waitAtEndpoint = 0.8f;

        [Header("Detección por visión")]
        [SerializeField] private float visionRange = 6f;
        [SerializeField, Range(5f, 180f)] private float visionAngle = 60f;
        [Tooltip("Si el jugador está en sombra, ¿puede ser visto igual?")]
        [SerializeField] private bool seesPlayerInDarkness = false;

        [Header("Detección por proximidad (toca al jugador)")]
        [SerializeField] private float proximityDetectionRadius = 0.9f;

        [Header("Daño")]
        [SerializeField] private int damageOnDetect = 1;
        [SerializeField] private bool instaKill = false;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private Transform _target;          // siguiente punto A o B
        private bool _goingToB = true;
        private float _waitUntil;
        private Transform _playerTransform;
        private PlayerHealth _playerHealth;
        private PlayerStealth _playerStealth;
        private float _detectionCooldown;

        private void Start()
        {
            _target = patrolPointB != null ? patrolPointB : transform;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                _playerTransform = playerGo.transform;
                _playerHealth    = playerGo.GetComponent<PlayerHealth>();
                _playerStealth   = playerGo.GetComponent<PlayerStealth>();
            }
        }

        private void Update()
        {
            Patrol();
            TryDetectPlayer();
        }

        private void Patrol()
        {
            if (patrolPointA == null || patrolPointB == null) return;
            if (Time.time < _waitUntil) return;

            Vector3 targetPos = _target.position;
            targetPos.y = transform.position.y; // ignorar diferencias verticales
            Vector3 toTarget = targetPos - transform.position;

            if (toTarget.magnitude < 0.1f)
            {
                _waitUntil = Time.time + waitAtEndpoint;
                _goingToB = !_goingToB;
                _target = _goingToB ? patrolPointB : patrolPointA;
                return;
            }

            Vector3 dir = toTarget.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            // Rotar suavemente hacia donde camina
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }

        private void TryDetectPlayer()
        {
            if (_playerTransform == null || _playerHealth == null) return;
            if (Time.time < _detectionCooldown) return;

            Vector3 toPlayer = _playerTransform.position - transform.position;
            float dist = toPlayer.magnitude;

            // (a) Proximidad: tocar al jugador = daño seguro
            if (dist < proximityDetectionRadius)
            {
                InflictDamage();
                return;
            }

            // (b) Visión por cono
            if (dist <= visionRange)
            {
                float angle = Vector3.Angle(transform.forward, toPlayer);
                if (angle <= visionAngle * 0.5f)
                {
                    // ¿Está en luz o lo veo aunque esté en sombra?
                    bool playerVisible = seesPlayerInDarkness || (_playerStealth != null && _playerStealth.IsInLight);

                    if (playerVisible)
                    {
                        // Línea de visión real (no detrás de paredes)
                        if (!Physics.Linecast(transform.position + Vector3.up * 1.4f,
                                              _playerTransform.position + Vector3.up * 1.0f,
                                              out RaycastHit hit) || hit.transform == _playerTransform)
                        {
                            InflictDamage();
                        }
                    }
                }
            }
        }

        private void InflictDamage()
        {
            _detectionCooldown = Time.time + 1.0f; // 1s entre detecciones
            if (instaKill)
            {
                _playerHealth.TakeDamage(99);
            }
            else
            {
                _playerHealth.TakeDamage(damageOnDetect);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            // Ruta
            if (patrolPointA != null && patrolPointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
                Gizmos.DrawWireSphere(patrolPointA.position, 0.2f);
                Gizmos.DrawWireSphere(patrolPointB.position, 0.2f);
            }

            // Proximidad
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, proximityDetectionRadius);

            // Cono de visión
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            Vector3 origin = transform.position + Vector3.up * 1.4f;
            Quaternion left  = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
            Quaternion right = Quaternion.AngleAxis( visionAngle * 0.5f, Vector3.up);
            Vector3 fwd = transform.forward * visionRange;
            Gizmos.DrawLine(origin, origin + left  * fwd);
            Gizmos.DrawLine(origin, origin + right * fwd);
            Gizmos.DrawLine(origin + left * fwd, origin + right * fwd);
        }
    }
}
