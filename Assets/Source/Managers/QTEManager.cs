using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using NoScope.States;

namespace NoScope
{
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Serializable]
    public class QTESequence
    {
        public List<ArrowDirection> sequence;
        public float timeLimit = 2f;
    }

    public class QTEManager : MonoBehaviour
    {
        public static QTEManager Instance { get; private set; }

        [Header("Style Rank Settings")]
        [SerializeField]
        private string[] styleRanks = new string[]
        {
            "Dismal",
            "Crazy",
            "Badass",
            "Apocalyptic!",
            "Savage!",
            "Sick Skills!!",
            "Smokin' Sexy Style!!"
        };
        [SerializeField] private float baseMultiplier = 1f;
        [SerializeField] private float minMultiplierIncrease = 0.7f;
        [SerializeField] private float maxMultiplierIncrease = 1.9f;
        [SerializeField] private int maxRankLevel = 6; // Index max (7 rangs au total : 0-6)

        [Header("QTE Settings")]
        [SerializeField] private int minSequenceLength = 3;
        [SerializeField] private int maxSequenceLength = 6;
        [SerializeField] private float baseTimeLimit = 5f; // Temps de base pour la QTE
        [SerializeField] private float timeLimitReduction = 0.05f; // Réduction du temps à chaque succès (plus subtile)
        [SerializeField] private float minTimeLimit = 1.2f; // Temps minimum (plus difficile que 1s)

        [Header("UI References")]
        [SerializeField] private GameObject qteUIPanel;
        [SerializeField] private TextMeshProUGUI sequenceText;
        [SerializeField] private TextMeshProUGUI timerText; // Affiche le temps restant
        [SerializeField] private TextMeshProUGUI instructionText; // "Reproduisez la séquence!"
        [SerializeField] private Slider timeSlider; // Barre de progression du temps

        // Events
        public event Action<bool> OnQTEComplete;
        public event Action OnQTEStarted;

        private QTESequence _currentSequence;
        private List<ArrowDirection> _playerInput = new List<ArrowDirection>();
        private bool _isQTEActive = false;
        public float CurrentTimeLimit;
        private float _timeRemaining;
        private int _successfulQTECount = 0; // QTE consécutives réussies (reset sur échec)
        private int _totalQTECount = 0; // QTE totales effectuées (jamais reset)
        private float _currentScoreMultiplier = 1f;
        private int _currentRankLevel = 0; // 0 = Dismal, 6 = Smokin' Sexy Style!!

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (_isQTEActive)
            {
                CheckInput();
            }
        }

        public void StartQTE()
        {
            if (_isQTEActive)
            {
                Debug.LogWarning("QTE already active, ignoring StartQTE");
                return;
            }

            _isQTEActive = true;
            _playerInput.Clear();

            // Change vers StateStyle (arrêt du temps)
            if (StateMachine.Instance != null)
            {
                Debug.Log("Changing to StateStyle...");
                StateMachine.Instance.ChangeState(StateStyle.Instance);
            }

            // Génère une nouvelle séquence
            GenerateSequence();

            // Active l'UI
            if (qteUIPanel != null)
            {
                qteUIPanel.SetActive(true);
                Debug.Log("QTE UI Panel activated");
            }


            OnQTEStarted?.Invoke();

            // Démarre le timer
            StartCoroutine(QTETimer());
        }

        private void GenerateSequence()
        {
            _currentSequence = new QTESequence();
            _currentSequence.sequence = new List<ArrowDirection>();

            int length = UnityEngine.Random.Range(minSequenceLength, maxSequenceLength + 1);

            for (int i = 0; i < length; i++)
            {
                ArrowDirection randomDirection = (ArrowDirection)UnityEngine.Random.Range(0, 4);
                _currentSequence.sequence.Add(randomDirection);
            }

            // Ajuste la difficulté - réduit le temps progressivement basé sur le nombre TOTAL de QTE
            CurrentTimeLimit = baseTimeLimit - (_totalQTECount * timeLimitReduction);
            CurrentTimeLimit = Mathf.Max(CurrentTimeLimit, minTimeLimit);
            _timeRemaining = CurrentTimeLimit;


            UpdateUI();
        }

        private void CheckInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                ProcessInput(ArrowDirection.Up);
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                ProcessInput(ArrowDirection.Down);
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                ProcessInput(ArrowDirection.Left);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                ProcessInput(ArrowDirection.Right);
            }
        }

        private void ProcessInput(ArrowDirection input)
        {
            _playerInput.Add(input);

            // Vérifie si l'entrée est correcte
            if (_playerInput.Count > _currentSequence.sequence.Count)
            {
                // Trop d'entrées
                FailQTE();
                return;
            }

            // Vérifie si l'entrée actuelle est correcte
            if (_playerInput[_playerInput.Count - 1] != _currentSequence.sequence[_playerInput.Count - 1])
            {
                FailQTE();
                return;
            }

            // Si toute la séquence est correcte
            if (_playerInput.Count == _currentSequence.sequence.Count)
            {
                SuccessQTE();
            }
            else
            {
                // Met à jour l'affichage pour montrer la progression
                UpdateUI();
            }
        }

        private IEnumerator QTETimer()
        {
            _timeRemaining = CurrentTimeLimit;

            // Initialise le slider
            if (timeSlider != null)
            {
                timeSlider.maxValue = CurrentTimeLimit;
                timeSlider.value = CurrentTimeLimit;
            }

            // Compte à rebours avec mise à jour de l'UI
            while (_timeRemaining > 0 && _isQTEActive)
            {
                _timeRemaining -= Time.unscaledDeltaTime; // unscaledDeltaTime car timeScale = 0

                // Met à jour le texte
                if (timerText != null)
                {
                    timerText.text = $"<b>Temps:</b> {_timeRemaining:F1}s";

                    // Change la couleur si le temps est critique
                    if (_timeRemaining < 1f)
                    {
                        timerText.color = Color.red;
                    }
                    else if (_timeRemaining < 2f)
                    {
                        timerText.color = Color.yellow;
                    }
                    else
                    {
                        timerText.color = Color.white;
                    }
                }

                // Met à jour le slider
                if (timeSlider != null)
                {
                    timeSlider.value = _timeRemaining;

                    // Gradient progressif de vert à rouge basé sur le pourcentage de temps restant
                    Image fillImage = timeSlider.fillRect?.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        float timePercent = _timeRemaining / CurrentTimeLimit;

                        fillImage.color = Color.Lerp(Color.red, Color.green, timePercent);
                    }
                }

                yield return null;
            }

            // Si le timer atteint 0 et la QTE est toujours active, échec
            if (_isQTEActive)
            {
                FailQTE();
            }
        }

        private void SuccessQTE()
        {
            _isQTEActive = false;
            _successfulQTECount++; // QTE consécutives
            _totalQTECount++; // QTE totales (jamais reset)

            // Incrémente le multiplicateur et le rang si possible
            if (_currentRankLevel < maxRankLevel)
            {
                float multiplierIncrease = UnityEngine.Random.Range(minMultiplierIncrease, maxMultiplierIncrease);
                _currentScoreMultiplier += multiplierIncrease;
                _currentRankLevel++;

                Debug.Log($"Style Rank UP! {styleRanks[_currentRankLevel]} - Multiplicateur: {_currentScoreMultiplier:F2}x");
            }

            // Met à jour le multiplicateur dans GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetScoreMultiplier(_currentScoreMultiplier);
            }

            // Met à jour l'affichage visuel du multiplicateur dans UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateMultiplier(_currentScoreMultiplier, styleRanks[_currentRankLevel]);
            }

            if (qteUIPanel != null)
                qteUIPanel.SetActive(false);

            // Réinitialise la vélocité du player pour éviter les mouvements résiduels
            ResetPlayerVelocity();

            StateMachine.Instance.ChangeState(StatePlay.Instance);


            // GameManager.Instance.AddScore();
            OnQTEComplete?.Invoke(true);
            StopAllCoroutines();
        }

        private void FailQTE()
        {

            _isQTEActive = false;
            _successfulQTECount = 0; // Reset uniquement le compteur consécutif
                                     // _totalQTECount n'est PAS reset, la difficulté continue d'augmenter

            // Réinitialise le multiplicateur et le rang
            _currentScoreMultiplier = baseMultiplier;
            _currentRankLevel = 0;

            Debug.Log($"QTE ratée! Style Rank réinitialisé à {styleRanks[0]} - Multiplicateur: {_currentScoreMultiplier}x");

            // Met à jour le multiplicateur dans GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetScoreMultiplier(_currentScoreMultiplier);
            }

            // Met à jour l'affichage visuel du multiplicateur dans UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateMultiplier(_currentScoreMultiplier, styleRanks[0]);
            }

            if (qteUIPanel != null)
                qteUIPanel.SetActive(false);

            // Réinitialise la vélocité du player pour éviter les mouvements résiduels
            ResetPlayerVelocity();

            StateMachine.Instance.ChangeState(StatePlay.Instance);


            OnQTEComplete?.Invoke(false);
            StopAllCoroutines();
        }

        private void UpdateUI()
        {
            // Affiche la séquence cible
            if (sequenceText != null)
            {
                string sequence = "";
                for (int i = 0; i < _currentSequence.sequence.Count; i++)
                {
                    // Affiche l'input du joueur en vert si correct, sinon la séquence cible
                    if (i < _playerInput.Count)
                    {
                        sequence += $"<color=green>{GetArrowSymbol(_playerInput[i])}</color> ";
                    }
                    else
                    {
                        sequence += $"<color=white>{GetArrowSymbol(_currentSequence.sequence[i])}</color> ";
                    }
                }
                sequenceText.text = sequence;
            }

            // Affiche les instructions
            if (instructionText != null)
            {
                instructionText.text = "<b>Il est temps de montrer ton style !</b>";
            }

            // Timer initial
            if (timerText != null)
            {
                timerText.text = $"<b>Temps:</b> {CurrentTimeLimit:F1}s";
                timerText.color = Color.white;
            }

            // Slider initial
            if (timeSlider != null)
            {
                timeSlider.maxValue = CurrentTimeLimit;
                timeSlider.value = CurrentTimeLimit;
            }
        }

        private string GetArrowSymbol(ArrowDirection direction)
        {
            switch (direction)
            {
                case ArrowDirection.Up: return "↑";
                case ArrowDirection.Down: return "↓";
                case ArrowDirection.Left: return "←";
                case ArrowDirection.Right: return "→";
                default: return "?";
            }
        }

        private void ResetPlayerVelocity()
        {
            // Trouve le player et réinitialise sa vélocité pour éviter les mouvements résiduels
            if (GameManager.Instance != null && GameManager.Instance.GetPlayer() != null)
            {
                Player player = GameManager.Instance.GetPlayer();
                player.ResetVelocityToForward();

                Debug.Log("Player velocity reset after QTE");
            }
        }

        public bool IsQTEActive()
        {
            return _isQTEActive;
        }

        public int GetSuccessfulQTECount()
        {
            return _successfulQTECount;
        }

        public string GetCurrentStyleRank()
        {
            return styleRanks[_currentRankLevel];
        }

        public float GetCurrentMultiplier()
        {
            return _currentScoreMultiplier;
        }

        public int GetCurrentRankLevel()
        {
            return _currentRankLevel;
        }

        public void ResetState()
        {
            _isQTEActive = false;
            _successfulQTECount = 0;
            _totalQTECount = 0;
            _playerInput.Clear();

            if (qteUIPanel != null)
                qteUIPanel.SetActive(false);

            StopAllCoroutines();
            Debug.Log("QTEManager state reset");
        }
    }
}
