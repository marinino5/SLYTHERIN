using UnityEngine;
using TMPro;
using Slytherin.Player;
using Slytherin.Managers;

namespace Slytherin.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup messagePanel;

        [Header("Player")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Puntaje")]
        [SerializeField] private int requiredScore = 100;

        [Header("Mensajes")]
        [SerializeField] private float messageDuration = 2.5f;
        [SerializeField] private float messageFadeSpeed = 4f;

        private float messageHideAt;
        private bool showingMessage;
        private bool gameOver;
        private bool levelWon;

        private void Start()
        {
            Time.timeScale = 1f;

            if (playerHealth == null)
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player");

                if (playerGo != null)
                    playerHealth = playerGo.GetComponent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged.AddListener(UpdateLives);
                playerHealth.OnDamageTaken.AddListener(() => ShowMessage("¡Detectado!"));
                playerHealth.OnDeath.AddListener(ShowGameOver);

                UpdateLives(playerHealth.CurrentLives);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
                GameManager.Instance.OnLevelWon.AddListener(ShowVictory);

                UpdateScore(GameManager.Instance.Score);
            }

            if (messageText != null)
                messageText.text = "";

            if (messagePanel != null)
                messagePanel.alpha = 0f;
        }

        private void Update()
        {
            if ((gameOver || levelWon) && Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;

                if (GameManager.Instance != null)
                    GameManager.Instance.RestartLevel();

                return;
            }

            if (messagePanel == null) return;

            float targetAlpha = showingMessage ? 1f : 0f;
            messagePanel.alpha = Mathf.MoveTowards(
                messagePanel.alpha,
                targetAlpha,
                messageFadeSpeed * Time.unscaledDeltaTime
            );

            if (showingMessage && Time.unscaledTime > messageHideAt)
            {
                showingMessage = false;
            }
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
                livesText.text = $"♥ {lives}";
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"⏱ {score}/{requiredScore}";
        }

        private void ShowGameOver()
        {
            gameOver = true;

            ShowMessage("GAME OVER\nPresiona R para reiniciar", true);

            Time.timeScale = 0f;
        }

        private void ShowVictory(string message)
        {
            levelWon = true;

            ShowMessage("MISIÓN COMPLETADA\nPresiona R para reiniciar", true);

            Time.timeScale = 0f;
        }

        public void ShowMessage(string msg, bool persistent = false)
        {
            if (messageText != null)
                messageText.text = msg;

            showingMessage = true;
            messageHideAt = persistent ? float.MaxValue : Time.unscaledTime + messageDuration;

            if (messagePanel != null)
                messagePanel.alpha = 1f;
        }
    }
}