using UnityEngine;
using UnityEngine.AI;
using Slytherin.Player;

namespace Slytherin.Enemies
{
    public class PatrolEnemy : MonoBehaviour
    {
        private enum State { Patrol, Chase }

        [Header("Ruta")]
        [SerializeField] private Transform patrolPointA;
        [SerializeField] private Transform patrolPointB;

        [Header("Velocidades")]
        [SerializeField] private float patrolSpeed = 8f;
        [SerializeField] private float chaseSpeed = 14f;
        [SerializeField] private float waitAtEndpoint = 0.8f;

        [Header("Detección")]
        [SerializeField] private float visionRange = 180f;
        [SerializeField, Range(5f, 180f)] private float visionAngle = 100f;
        [SerializeField] private float proximityDetectionRadius = 20f;
        [SerializeField] private bool seesPlayerInDarkness = true;

        [Header("Persecución")]
        [SerializeField] private float losePlayerDistance = 260f;
        [SerializeField] private float catchDistance = 12f;

        [Header("Daño")]
        [SerializeField] private int damageOnDetect = 1;
        [SerializeField] private float damageCooldown = 1.2f;

        [Header("Animaciones")]
        [SerializeField] private string idleAnimation = "Idle";
        [SerializeField] private string walkAnimation = "Walk";
        [SerializeField] private string runAnimation = "Run";
        [SerializeField] private string angryAnimation = "Angry";
        [SerializeField] private string lookAroundAnimation = "LookAround";

        private State state = State.Patrol;
        private NavMeshAgent agent;
        private Animator animator;

        private Transform currentPatrolTarget;
        private bool goingToB = true;
        private float waitUntil;

        private Transform playerTransform;
        private PlayerHealth playerHealth;
        private PlayerStealth playerStealth;

        private float nextDamageTime;
        private string currentAnimation = "";

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");

            if (playerGo != null)
            {
                Debug.Log("Player encontrado por Vernon: " + playerGo.name);

                playerTransform = playerGo.transform;
                playerHealth = playerGo.GetComponentInChildren<PlayerHealth>();
                playerStealth = playerGo.GetComponentInChildren<PlayerStealth>();

                if (playerHealth == null)
                {
                    Debug.Log("ERROR: No encontré PlayerHealth en el player");
                }
                else
                {
                    Debug.Log("PlayerHealth encontrado correctamente");
                }
            }
            else
            {
                Debug.Log("ERROR: Vernon no encontró ningún objeto con Tag Player");
            }

            currentPatrolTarget = patrolPointB;

            if (agent != null)
            {
                agent.speed = patrolSpeed;
            }

            PlayAnimation(idleAnimation);
        }

        private void Update()
        {
            if (playerTransform == null) return;

            bool canSeePlayer = CanDetectPlayer();

            if (canSeePlayer)
            {
                state = State.Chase;
            }

            if (state == State.Chase)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }

        private void Patrol()
        {
            agent.speed = patrolSpeed;

            if (patrolPointA == null || patrolPointB == null)
            {
                PlayAnimation(idleAnimation);
                return;
            }

            if (Time.time < waitUntil)
            {
                agent.isStopped = true;
                PlayAnimation(lookAroundAnimation);
                return;
            }

            agent.isStopped = false;
            PlayAnimation(walkAnimation);

            if (currentPatrolTarget == null)
            {
                currentPatrolTarget = patrolPointB;
            }

            agent.SetDestination(currentPatrolTarget.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
            {
                waitUntil = Time.time + waitAtEndpoint;

                goingToB = !goingToB;
                currentPatrolTarget = goingToB ? patrolPointB : patrolPointA;
            }
        }

        private void ChasePlayer()
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            agent.speed = chaseSpeed;

            Debug.Log("DISTANCIA A JUGADOR: " + distance);

            if (distance <= catchDistance)
            {
                agent.isStopped = true;
                PlayAnimation(angryAnimation);
                DamagePlayer();
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
            PlayAnimation(runAnimation);

            if (distance > losePlayerDistance)
            {
                state = State.Patrol;
                agent.isStopped = false;
                agent.speed = patrolSpeed;
            }
        }

        private bool CanDetectPlayer()
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            float distance = toPlayer.magnitude;

            if (distance <= proximityDetectionRadius)
            {
                Debug.Log("VERNON DETECTÓ AL JUGADOR POR CERCANÍA");
                DamagePlayer();
                return true;
            }

            if (distance > visionRange)
            {
                return false;
            }

            float angle = Vector3.Angle(transform.forward, toPlayer);

            if (angle > visionAngle * 0.5f)
            {
                return false;
            }

            bool playerVisible = seesPlayerInDarkness ||
                                 (playerStealth != null && playerStealth.IsInLight);

            if (!playerVisible)
            {
                return false;
            }

            Debug.Log("VERNON TE ESTÁ VIENDO");
            return true;
        }

        private void DamagePlayer()
        {
            Debug.Log("ENTRÓ A DAMAGE PLAYER");

            if (Time.time < nextDamageTime)
            {
                Debug.Log("NO QUITA VIDA POR COOLDOWN");
                return;
            }

            PlayAnimation(angryAnimation);

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnDetect);
                Debug.Log("VIDA QUITADA POR VERNON");
            }
            else
            {
                Debug.Log("ERROR: Vernon no encontró PlayerHealth");
            }

            nextDamageTime = Time.time + damageCooldown;

            Debug.Log("VERNON TE ALCANZÓ");
        }

        public void AlertToPlayer()
        {
            if (playerTransform == null) return;

            Debug.Log("VERNON RECIBIÓ ALERTA DE LA TÍA");

            state = State.Chase;

            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }
        }
        private void PlayAnimation(string animationName)
        {
            if (animator == null) return;
            if (currentAnimation == animationName) return;

            currentAnimation = animationName;
            animator.CrossFade(animationName, 0.15f);
        }
    }
}