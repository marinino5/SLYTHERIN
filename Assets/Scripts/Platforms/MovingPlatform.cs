using UnityEngine;

namespace Slytherin.Platforms
{
    /// <summary>
    /// Plataforma que se mueve entre dos posiciones (suben y bajan o se desplazan lateralmente).
    /// - Define una posición A (la del transform al iniciar) y un offset hacia B.
    /// - Loop continuo o por trigger del jugador.
    /// - Al estar el jugador encima, lo "pega" como hijo para que se mueva con la plataforma
    ///   (sin esto el CharacterController se desliza).
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        public enum MoveMode { LoopContinuo, ActivarConTrigger }

        [Header("Movimiento")]
        [SerializeField] private Vector3 offsetToPointB = new Vector3(0f, 3f, 0f);
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private MoveMode mode = MoveMode.LoopContinuo;
        [SerializeField] private float pauseAtEnds = 0.5f;

        [Header("Trigger (solo modo ActivarConTrigger)")]
        [SerializeField] private bool startActivated = false;

        private Vector3 _pointA;
        private Vector3 _pointB;
        private bool _goingToB = true;
        private bool _isActive = true;
        private float _pauseUntil;
        private Transform _passenger;

        private void Awake()
        {
            _pointA = transform.position;
            _pointB = _pointA + offsetToPointB;
            _isActive = (mode == MoveMode.LoopContinuo) || startActivated;
        }

        private void Update()
        {
            if (!_isActive || Time.time < _pauseUntil) return;

            Vector3 prevPos = transform.position;
            Vector3 target = _goingToB ? _pointB : _pointA;
            transform.position = Vector3.MoveTowards(prevPos, target, speed * Time.deltaTime);

            // Si tenemos pasajero, lo desplazamos manualmente con el delta
            if (_passenger != null)
            {
                Vector3 delta = transform.position - prevPos;
                _passenger.position += delta;
            }

            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                _goingToB = !_goingToB;
                _pauseUntil = Time.time + pauseAtEnds;
            }
        }

        public void Activate()   => _isActive = true;
        public void Deactivate() => _isActive = false;

        // Detectar pasajero por colisión sólida (no trigger).
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
                _passenger = collision.transform;
        }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
                _passenger = null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 a = Application.isPlaying ? _pointA : transform.position;
            Vector3 b = Application.isPlaying ? _pointB : transform.position + offsetToPointB;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(a, Vector3.one * 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(b, Vector3.one * 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(a, b);
        }
    }
}
