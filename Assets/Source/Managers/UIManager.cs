using TMPro;
using System.Collections.Generic;
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

        [Header("Lives UI")]
        [SerializeField] private Sprite skateSprite;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private Vector2 lifeIconSize = new Vector2(48, 48);

        // Assign a RectTransform in the Inspector (e.g. a child of `gameplayPanel`).
        [SerializeField] private RectTransform livesContainer;

        private List<Image> _lifeIcons = new List<Image>();

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

            // Setup minimal lives UI and subscribe to player life changes
            SetupLivesUI();
            Player p = FindFirstObjectByType<Player>();
            if (p != null)
            {
                p.OnLifeChanged -= UpdateLives;
                p.OnLifeChanged += UpdateLives;
                UpdateLives(p.Life);
            }
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

            // Unsubscribe and dim lives
            Player p = FindFirstObjectByType<Player>();
            if (p != null)
            {
                p.OnLifeChanged -= UpdateLives;
            }
            UpdateLives(0);
        }

        // Minimal lives UI helpers (non-intrusive)
        private void SetupLivesUI()
        {
            // Try to find a container if none assigned
            if (livesContainer == null)
            {
                // first try: child named LivesContainer under gameplayPanel
                if (gameplayPanel != null)
                {
                    Transform t = gameplayPanel.transform.Find("LivesContainer");
                    if (t != null) livesContainer = t as RectTransform;
                }

                // second try: global find
                if (livesContainer == null)
                {
                    GameObject found = GameObject.Find("LivesContainer");
                    if (found != null) livesContainer = found.GetComponent<RectTransform>();
                }
            }

            // If still no container, optionally create one if we have a sprite and a gameplayPanel
            if (livesContainer == null)
            {
                if (skateSprite == null || gameplayPanel == null)
                {
                    Debug.LogWarning("[UIManager] livesContainer not assigned and cannot create one automatically. Assign 'livesContainer' in Inspector or add a child named 'LivesContainer' under gameplayPanel.");
                    return;
                }

                GameObject containerGO = new GameObject("LivesContainer");
                containerGO.transform.SetParent(gameplayPanel.transform, false);
                livesContainer = containerGO.AddComponent<RectTransform>();
                var hg = containerGO.AddComponent<HorizontalLayoutGroup>();
                hg.spacing = 6f;
            }

            // If icons already exist as children, reuse them (designer-created)
            _lifeIcons.Clear();
            for (int i = 0; i < livesContainer.childCount && _lifeIcons.Count < maxLives; i++)
            {
                var child = livesContainer.GetChild(i);
                var img = child.GetComponent<Image>();
                if (img != null)
                {
                    _lifeIcons.Add(img);
                }
            }

            // If no existing child Images, create them (requires skateSprite)
            if (_lifeIcons.Count == 0)
            {
                if (skateSprite == null)
                {
                    Debug.LogWarning("[UIManager] No child Images found in livesContainer and 'skateSprite' is not assigned. No life icons will be displayed.");
                    return; // nothing to create
                }

                for (int i = 0; i < maxLives; i++)
                {
                    GameObject go = new GameObject($"LifeIcon_{i}");
                    go.transform.SetParent(livesContainer, false);
                    Image img = go.AddComponent<Image>();
                    img.sprite = skateSprite;
                    img.rectTransform.sizeDelta = lifeIconSize;
                    img.preserveAspect = true;
                    _lifeIcons.Add(img);
                }
            }

            // Ensure icon count matches maxLives by creating placeholders if needed
            while (_lifeIcons.Count < maxLives)
            {
                if (skateSprite == null) break;
                GameObject go = new GameObject($"LifeIcon_auto_{_lifeIcons.Count}");
                go.transform.SetParent(livesContainer, false);
                Image img = go.AddComponent<Image>();
                img.sprite = skateSprite;
                img.rectTransform.sizeDelta = lifeIconSize;
                img.preserveAspect = true;
                _lifeIcons.Add(img);
            }
        }

        public void UpdateLives(int life)
        {
            for (int i = 0; i < _lifeIcons.Count; i++)
            {
                bool on = i < life;
                if (_lifeIcons[i] != null)
                {
                    _lifeIcons[i].enabled = on;
                    Color c = _lifeIcons[i].color;
                    c.a = on ? 1f : 0.25f;
                    _lifeIcons[i].color = c;
                }
            }
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
        // Méthodes UI pour boutons supprimées car non référencées dans le code.
        // Si des boutons UI dans les scènes utilisaient ces callbacks, rajoutez-les manuellement.
    }
}
