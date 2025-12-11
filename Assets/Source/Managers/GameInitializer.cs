using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Script pour tester rapidement le jeu dans l'éditeur
    /// Attacher ce script à un GameObject dans la scène
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject stateMachinePrefab;
        [SerializeField] private GameObject qteManagerPrefab;
        [SerializeField] private GameObject pipeGeneratorPrefab;
        [SerializeField] private GameObject uiManagerPrefab;

        [Header("Auto Start")]
        [SerializeField] private bool autoStartGame = false;

        private void Awake()
        {
            // Initialise les managers s'ils n'existent pas déjà
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }

            if (StateMachine.Instance == null && stateMachinePrefab != null)
            {
                Instantiate(stateMachinePrefab);
            }

            if (QTEManager.Instance == null && qteManagerPrefab != null)
            {
                Instantiate(qteManagerPrefab);
            }

            if (PipeGenerator.Instance == null && pipeGeneratorPrefab != null)
            {
                Instantiate(pipeGeneratorPrefab);
            }

            if (UIManager.Instance == null && uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
        }

        private void Start()
        {
            if (autoStartGame && GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
