using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoScope
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Gameplay UI")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Slider enemyHealthSlider;
        [SerializeField] private TextMeshProUGUI enemyHealthText;

        [Header("QTE UI")]
        [SerializeField] private GameObject qtePanel;
        [SerializeField] private TextMeshProUGUI qteSequenceText;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // S'abonne aux événements
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
                GameManager.Instance.OnGamePause += OnGamePause;
                GameManager.Instance.OnGameResume += OnGameResume;
                GameManager.Instance.OnGameLose += OnGameLose;
                GameManager.Instance.OnGameWin += OnGameWin;
                GameManager.Instance.OnScoreChanged += UpdateScore;
            }

            ShowMainMenu();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameStarted())
            {
                UpdateGameplayUI();
            }
        }

        private void UpdateGameplayUI()
        {
            // Met à jour le temps
            if (timeText != null)
            {
                float gameTime = GameManager.Instance.GetGameTime();
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            // Met à jour la vitesse
            Player player = GameManager.Instance.GetPlayer();
            if (player != null && speedText != null)
            {
                speedText.text = $"Speed: {player.GetCurrentSpeed():F1}";
            }

            // Met à jour la barre de vie de l'ennemi
            EnemyMass enemy = GameManager.Instance.GetEnemyMass();
            if (enemy != null)
            {
                if (enemyHealthSlider != null)
                {
                    enemyHealthSlider.value = enemy.GetHealthPercentage();
                }

                if (enemyHealthText != null)
                {
                    enemyHealthText.text = $"Enemy HP: {enemy.GetHealthPercentage() * 100f:F0}%";
                }
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        private void OnGameStart()
        {
            HideAllPanels();
            if (gameplayPanel != null)
                gameplayPanel.SetActive(true);
        }

        private void OnGamePause()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        private void OnGameResume()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnGameLose()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }

        private void OnGameWin()
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        // Méthodes appelées par les boutons UI
        public void OnStartButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        public void OnPauseButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }

        public void OnResumeButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        public void OnRestartButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        public void OnQuitButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
