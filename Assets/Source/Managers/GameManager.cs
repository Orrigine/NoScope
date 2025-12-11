using System;
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

        [Header("Scene References (Optional)")]
        [SerializeField] private Player existingPlayer; // Si déjà dans la scène
        [SerializeField] private EnemyMass existingEnemyMass; // Si déjà dans la scène

        private Player _currentPlayer;
        private EnemyMass _currentEnemyMass;
        private float _gameTime = 0f;
        private int _score = 0;

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

        private void Start()
        {
            // Initialisation
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

            // Utilise le joueur existant ou spawn un nouveau
            if (existingPlayer != null)
            {
                _currentPlayer = existingPlayer;
                Debug.Log("Using existing Player from scene");
            }
            else if (playerPrefab != null)
            {
                GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                _currentPlayer = playerObj.GetComponent<Player>();
                Debug.Log("Spawned new Player from prefab");
            }

            if (_currentPlayer != null)
            {
                _currentPlayer.OnPlayerDie += OnPlayerDeath;
            }

            // Utilise la masse ennemie existante ou spawn une nouvelle
            if (existingEnemyMass != null)
            {
                _currentEnemyMass = existingEnemyMass;
                Debug.Log("Using existing EnemyMass from scene");
            }
            else if (enemyMassPrefab != null)
            {
                Vector3 spawnPos = Vector3.back * 30f; // Derrière le joueur
                GameObject enemyObj = Instantiate(enemyMassPrefab, spawnPos, Quaternion.identity);
                _currentEnemyMass = enemyObj.GetComponent<EnemyMass>();
                Debug.Log("Spawned new EnemyMass from prefab");
            }

            // Change l'état vers Play
            if (StateMachine.Instance != null)
            {
                StateMachine.Instance.ChangeState(StatePlay.Instance);
            }
            OnGameStart?.Invoke();
            Debug.Log("Game Started!");
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
            Debug.Log("Game Paused!");
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
            Debug.Log("Game Resumed!");
        }

        public void LoseGame()
        {
            if (isGameOver) return;

            isGameOver = true;
            isGameStarted = false;

            OnGameLose?.Invoke();
            Debug.Log("Game Over - You Lost!");

            // Optionnel: Arrêter le temps
            // Time.timeScale = 0f;
        }

        public void WinGame()
        {
            if (isGameOver) return;

            isGameOver = true;
            isGameStarted = false;

            OnGameWin?.Invoke();
            Debug.Log("You Win!");
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
            Debug.Log("Quitting Game...");

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
