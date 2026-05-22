using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Slytherin.Player;

namespace Slytherin.Managers
{
    /// <summary>
    /// GameManager — singleton que gobierna el estado global del nivel.
    /// - Score
    /// - Checkpoint actual
    /// - Reinicio del jugador al checkpoint cuando muere
    /// - Victoria / Game Over
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Respawn")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float respawnDelay = 1.5f;

        [Header("Eventos UI")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<string> OnLevelWon;
        public UnityEvent OnPlayerRespawn;

        public int Score { get; private set; }
        public Vector3 CurrentCheckpoint { get; private set; }
        public bool HasCheckpoint { get; private set; }
        public bool LevelEnded { get; private set; }

        private GameObject _playerGo;
        private PlayerHealth _playerHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _playerGo = GameObject.FindGameObjectWithTag("Player");
            if (_playerGo != null)
            {
                _playerHealth = _playerGo.GetComponent<PlayerHealth>();
                if (_playerHealth != null)
                    _playerHealth.OnDeath.AddListener(HandlePlayerDeath);
            }

            // Checkpoint inicial = posición inicial del jugador
            if (_playerGo != null && spawnPoint == null)
                CurrentCheckpoint = _playerGo.transform.position;
            else if (spawnPoint != null)
                CurrentCheckpoint = spawnPoint.position;

            HasCheckpoint = true;
        }

        public void AddScore(int amount)
        {
            if (LevelEnded) return;
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void SetCheckpoint(Vector3 pos)
        {
            CurrentCheckpoint = pos;
            HasCheckpoint = true;
        }

        public void WinLevel(string message)
        {
            if (LevelEnded) return;
            LevelEnded = true;
            OnLevelWon?.Invoke(message);
            Debug.Log("[GameManager] Nivel completado: " + message);
        }

        private void HandlePlayerDeath()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (_playerGo == null) yield break;

            // CharacterController interfiere con teleport directo: desactivarlo
            var cc = _playerGo.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            _playerGo.transform.position = CurrentCheckpoint;

            if (cc != null) cc.enabled = true;

            _playerHealth?.ResetHealth();
            OnPlayerRespawn?.Invoke();
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
