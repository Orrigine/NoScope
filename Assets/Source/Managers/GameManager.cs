using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using NoScope.States;

namespace NoScope
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private bool isGameStarted = false;
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private bool isGameOver = false;

        [Header("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyMassPrefab;

        private Player _currentPlayer;
        private EnemyMass _currentEnemyMass;
        private float _gameTime = 0f;
        private int _score = 0;
        private float _currentScoreMultiplier = 1f;

        // Events
        public event Action OnGameStart;
        public event Action OnGamePause;
        public event Action OnGameResume;
        public event Action OnGameLose;
        public event Action OnGameWin;
        public event Action<int> OnScoreChanged;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Si c'est la scène de jeu, démarre automatiquement
            if (scene.name == "Game" && !isGameStarted)
            {
                StartGame();
            }
        }

        private void Start()
        {
            // Si on est déjà dans Game au lancement (pas de MainMenu), démarre
            if (SceneManager.GetActiveScene().name == "Game" && !isGameStarted)
            {
                StartGame();
            }
        }

        private void Update()
        {
            // Restart avec la touche R
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }

            if (isGameStarted && !isGamePaused && !isGameOver)
            {
                _gameTime += Time.deltaTime;
            }
        }

        private void RestartScene()
        {
            // Réinitialise les managers avant de recharger
            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.ResetState();
            }

            if (PipeGenerator.Instance != null)
            {
                PipeGenerator.Instance.ResetState();
            }

            // Réinitialise l'état du jeu
            isGameStarted = false;
            isGamePaused = false;
            isGameOver = false;
            _gameTime = 0f;
            _score = 0;

            // Recharge la scène active
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void StartGame()
        {
            if (isGameStarted) return;

            isGameStarted = true;
            isGamePaused = false;
            isGameOver = false;
            _gameTime = 0f;
            _score = 0;

            // Trouve le joueur dans la scène active
            _currentPlayer = FindFirstObjectByType<Player>();

            // Si pas trouvé, spawn un nouveau
            if (_currentPlayer == null && playerPrefab != null)
            {

                GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                _currentPlayer = playerObj.GetComponent<Player>();
            }

            if (_currentPlayer != null)
            {
                _currentPlayer.OnPlayerDie += OnPlayerDeath;
            }
            else
            {
                Debug.LogError("[GameManager] Impossible de trouver ou créer le Player !");
            }

            // Trouve la masse ennemie dans la scène active
            _currentEnemyMass = FindFirstObjectByType<EnemyMass>();

            // Si pas trouvé, spawn une nouvelle
            if (_currentEnemyMass == null && enemyMassPrefab != null)
            {

                Vector3 spawnPos = Vector3.back * 30f;
                GameObject enemyObj = Instantiate(enemyMassPrefab, spawnPos, Quaternion.identity);
                _currentEnemyMass = enemyObj.GetComponent<EnemyMass>();
            }

            // Change l'état vers Play
            if (StateMachine.Instance != null)
            {
                StateMachine.Instance.ChangeState(StatePlay.Instance);
            }


            OnGameStart?.Invoke();
        }

        public void PauseGame()
        {
            if (!isGameStarted || isGamePaused || isGameOver) return;

            isGamePaused = true;
            Time.timeScale = 0f;

            // Change l'état vers Paused
            if (StateMachine.Instance != null)
            {
                StateMachine.Instance.ChangeState(StatePaused.Instance);
            }
            OnGamePause?.Invoke();

        }

        public void ResumeGame()
        {
            if (!isGamePaused) return;

            isGamePaused = false;
            Time.timeScale = 1f;

            // Retour à l'état Play
            if (StateMachine.Instance != null)
            {
                StateMachine.Instance.ChangeState(StatePlay.Instance);
            }
            OnGameResume?.Invoke();

        }

        public void LoseGame()
        {
            if (isGameOver) return;

            isGameOver = true;
            isGameStarted = false;

            // Marque le jeu comme terminé et en pause
            isGamePaused = true;

            OnGameLose?.Invoke();

            // Arrête complètement la simulation
            Time.timeScale = 0f;
        }

        public void WinGame()
        {
            if (isGameOver) return;

            isGameOver = true;
            isGameStarted = false;

            OnGameWin?.Invoke();

        }

        public void RestartGame()
        {
            // Reset le temps
            Time.timeScale = 1f;

            // Recharge la scène actuelle
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {


#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void StopTime()
        {
            Time.timeScale = 0f;
        }

        // Exécute une action différée sur l'instance persistante du GameManager.
        // Retourne le Coroutine pour permettre l'annulation.
        public Coroutine RunDelayed(float seconds, Action action)
        {
            if (action == null) return null;
            return StartCoroutine(RunDelayedCoroutine(seconds, action));
        }

        public void StopDelayed(Coroutine coroutine)
        {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
        }

        private IEnumerator RunDelayedCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Erreur dans RunDelayed action: {ex}");
            }
        }

        public void ResumeTime()
        {
            Time.timeScale = 1f;
        }

        public void StyleMoment()
        {
            // Moment de style (slow motion par exemple)
            if (StateMachine.Instance != null)
            {
                StateMachine.Instance.ChangeState(StateStyle.Instance);
            }
        }

        private void OnPlayerDeath()
        {
            LoseGame();
        }

        public void AddScore(int points)
        {
            _score += points;

            OnScoreChanged?.Invoke(_score);
        }

        public void AddScoreForEnemyKill()
        {
            // Score de base aléatoire entre 13 et 69
            int baseScore = UnityEngine.Random.Range(13, 70);

            // Applique le multiplicateur
            int finalScore = Mathf.RoundToInt(baseScore * _currentScoreMultiplier);

            AddScore(finalScore);


        }
        public void SetScoreMultiplier(float multiplier)
        {
            _currentScoreMultiplier = multiplier;
        }

        public float GetScoreMultiplier()
        {
            return _currentScoreMultiplier;
        }
        // Getters
        public bool IsGameStarted() => isGameStarted;
        public bool IsGamePaused() => isGamePaused;
        public bool IsGameOver() => isGameOver;
        public float GetGameTime() => _gameTime;
        public int GetScore() => _score;
        public Player GetPlayer() => _currentPlayer;
        public EnemyMass GetEnemyMass() => _currentEnemyMass;
    }
}
