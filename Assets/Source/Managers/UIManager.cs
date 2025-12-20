using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NoScope
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels - MainMenu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("UI Panels - Game")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject qtePanel;

        [Header("UI Elements - Gameplay")]
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Slider _enemyHealthSlider;
        [SerializeField] private TextMeshProUGUI _enemyHealthText;
        [SerializeField] private TextMeshProUGUI _multiplierText;
        [SerializeField] private TextMeshProUGUI _multiplierScoreText; // Le chiffre du multiplicateur

        private bool _hasCompletedFirstQTE = false; // Track si au moins une QTE a été faite

        private void Awake()
        {
            // UIManager existe dans chaque scène, pas de singleton persistant
            Instance = this;
        }

        private void Start()
        {
            // Cache les éléments de multiplicateur au démarrage
            HideMultiplierUI();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
                GameManager.Instance.OnGamePause += OnGamePause;
                GameManager.Instance.OnGameResume += OnGameResume;
                GameManager.Instance.OnGameLose += OnGameLose;
                GameManager.Instance.OnGameWin += OnGameWin;
                GameManager.Instance.OnScoreChanged += UpdateScore;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= OnGameStart;
                GameManager.Instance.OnGamePause -= OnGamePause;
                GameManager.Instance.OnGameResume -= OnGameResume;
                GameManager.Instance.OnGameLose -= OnGameLose;
                GameManager.Instance.OnGameWin -= OnGameWin;
                GameManager.Instance.OnScoreChanged -= UpdateScore;
            }
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
            if (_timeText != null)
            {
                float gameTime = GameManager.Instance.GetGameTime();
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                _timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }


            // Met à jour la barre de vie de l'ennemi
            EnemyMass enemy = GameManager.Instance.GetEnemyMass();
            if (enemy != null)
            {
                if (_enemyHealthSlider != null)
                {
                    _enemyHealthSlider.value = enemy.GetHealthPercentage();
                }

                if (_enemyHealthText != null)
                {
                    // _enemyHealthText.text = $"Enemy HP: {enemy.GetHealthPercentage() * 100f:F0}%";
                }
            }
        }

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// Met à jour l'affichage du multiplicateur et du texte de style rank
        /// </summary>
        /// <param name="multiplier">Le chiffre du multiplicateur (ex: 3.5)</param>
        /// <param name="rankText">Le texte du style rank (ex: "Smokin' Sexy Style!!")</param>
        public void UpdateMultiplier(float multiplier, string rankText)
        {
            // Affiche les éléments lors de la première QTE
            if (!_hasCompletedFirstQTE)
            {
                ShowMultiplierUI();
                _hasCompletedFirstQTE = true;
            }

            if (_multiplierScoreText != null)
            {
                _multiplierScoreText.text = $"x{multiplier:F1}";
            }

            if (_multiplierText != null)
            {
                _multiplierText.text = rankText;
            }
        }

        /// <summary>
        /// Cache les éléments de multiplicateur
        /// </summary>
        private void HideMultiplierUI()
        {
            if (_multiplierScoreText != null)
                _multiplierScoreText.gameObject.SetActive(false);

            if (_multiplierText != null)
                _multiplierText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Affiche les éléments de multiplicateur
        /// </summary>
        private void ShowMultiplierUI()
        {
            if (_multiplierScoreText != null)
                _multiplierScoreText.gameObject.SetActive(true);

            if (_multiplierText != null)
                _multiplierText.gameObject.SetActive(true);
        }

        public void ShowGameplayUI()
        {
            HideAllPanels();
            if (gameplayPanel != null)
                gameplayPanel.SetActive(true);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        public void ShowCredits()
        {
            HideAllPanels();
            if (creditsPanel != null)
                creditsPanel.SetActive(true);
        }

        public void ShowSettings()
        {
            HideAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
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
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // Méthodes appelées par les boutons UI
        public void OnCreditsButtonClicked()
        {
            HideAllPanels();
            creditsPanel.SetActive(true);
        }

        public void OnSettingsButtonClicked()
        {
            ShowSettings();
        }

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
