using UnityEngine;
using UnityEngine.Events;

namespace Slytherin.Player
{
    /// <summary>
    /// Vida del jugador (Albus).
    /// - Empieza con maxLives.
    /// - Pierde vida al colisionar con enemigos (a través de OnTriggerEnter o llamadas externas).
    /// - Si llega a 0, dispara OnDeath -> GameManager reinicia el nivel/checkpoint.
    /// - Periodo de invulnerabilidad tras recibir daño.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invulnerabilityTime = 1.2f;

        [Header("Eventos")]
        public UnityEvent<int> OnLivesChanged;   // pasa el nº actual
        public UnityEvent OnDamageTaken;
        public UnityEvent OnDeath;

        public int CurrentLives { get; private set; }
        public bool IsInvulnerable => Time.time - _lastDamageTime < invulnerabilityTime;
        public bool IsDead { get; private set; }

        private float _lastDamageTime = -999f;

        private void Awake()
        {
            CurrentLives = maxLives;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(CurrentLives);
        }

        /// <summary>Quita 'amount' vidas (default 1) si no es invulnerable.</summary>
        public void TakeDamage(int amount = 1)
        {
            if (IsDead || IsInvulnerable) return;

            _lastDamageTime = Time.time;
            CurrentLives = Mathf.Max(0, CurrentLives - amount);
            OnLivesChanged?.Invoke(CurrentLives);
            OnDamageTaken?.Invoke();

            if (CurrentLives <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }

        /// <summary>Restaura vida (consumibles tipo caramelo de limón).</summary>
        public void Heal(int amount = 1)
        {
            if (IsDead) return;
            CurrentLives = Mathf.Min(maxLives, CurrentLives + amount);
            OnLivesChanged?.Invoke(CurrentLives);
        }

        /// <summary>Resetea vida al reaparecer en checkpoint.</summary>
        public void ResetHealth()
        {
            IsDead = false;
            CurrentLives = maxLives;
            OnLivesChanged?.Invoke(CurrentLives);
        }
    }
}
