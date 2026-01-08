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

        // Events pour les inputs (supprimés s'ils ne sont pas utilisés)

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
                HandlePause();
            }

            // Restart (R key)
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                // Appelle directement l'action de restart via GameManager si présent
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
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
        // Ces helpers étaient inutilisés dans le projet et ont été retirés.
    }
}
