using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slytherin.Player;
using Slytherin.Managers;

namespace Slytherin.UI
{
    /// <summary>
    /// HUD del juego.
    /// - Muestra vidas (texto o iconos)
    /// - Muestra puntaje
    /// - Muestra mensajes de evento (Detectado, Misión completa, etc.)
    ///
    /// Requiere TextMeshPro. Si no quieres TMP, cambiar TMP_Text por UnityEngine.UI.Text.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Referencias UI (TextMeshPro)")]
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup messagePanel;

        [Header("Player ref (auto si está vacío)")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Mensajes")]
        [SerializeField] private float messageDuration = 2.5f;
        [SerializeField] private float messageFadeSpeed = 4f;

        private float _messageHideAt;
        private bool _showingMessage;

        private void Start()
        {
            if (playerHealth == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) playerHealth = go.GetComponent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged.AddListener(UpdateLives);
                playerHealth.OnDamageTaken.AddListener(() => ShowMessage("¡Detectado!"));
                UpdateLives(playerHealth.CurrentLives);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
                GameManager.Instance.OnLevelWon.AddListener(msg => ShowMessage(msg, persistent: true));
                UpdateScore(GameManager.Instance.Score);
            }

            if (messagePanel != null) messagePanel.alpha = 0f;
        }

        private void Update()
        {
            if (messagePanel == null) return;
            float target = _showingMessage ? 1f : 0f;
            messagePanel.alpha = Mathf.MoveTowards(messagePanel.alpha, target, messageFadeSpeed * Time.deltaTime);

            if (_showingMessage && Time.time > _messageHideAt) _showingMessage = false;
        }

        private void UpdateLives(int n)
        {
            if (livesText != null) livesText.text = $"♥ {n}";
        }

        private void UpdateScore(int s)
        {
            if (scoreText != null) scoreText.text = $"⏱ {s:D5}";
        }

        public void ShowMessage(string msg, bool persistent = false)
        {
            if (messageText != null) messageText.text = msg;
            _showingMessage = true;
            _messageHideAt = persistent ? float.MaxValue : Time.time + messageDuration;
        }
    }
}
