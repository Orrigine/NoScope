using UnityEngine;
using UnityEngine.InputSystem;

namespace NoScope
{
    /// <summary>
    /// Gestionnaire centralisé des inputs pour faciliter le changement de contrôles
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        // Events pour les inputs
        public event System.Action OnPausePressed;
        public event System.Action OnRestartPressed;

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

        private void Update()
        {
            CheckInputs();
        }

        private void CheckInputs()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnPausePressed?.Invoke();
                HandlePause();
            }

            // Restart (R key)
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                OnRestartPressed?.Invoke();
            }
        }

        private void HandlePause()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.IsGamePaused())
                {
                    GameManager.Instance.ResumeGame();
                }
                else if (GameManager.Instance.IsGameStarted())
                {
                    GameManager.Instance.PauseGame();
                }
            }
        }

        // Méthodes utilitaires pour vérifier les inputs (QTE)
        public bool GetUpArrow() => Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame;
        public bool GetDownArrow() => Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame;
        public bool GetLeftArrow() => Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame;
        public bool GetRightArrow() => Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame;
    }
}
