using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace NoScope
{
    /// <summary>
    /// Affiche des informations de debug à l'écran
    /// Utile pendant le développement
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private GameObject debugPanel; // Panel contenant le TextMeshProUGUI

        /// <summary>
        /// Propriété publique pour vérifier si le debug est actif
        /// </summary>
        public bool IsDebugActive => showDebugInfo;

        private void Start()
        {
            UpdateDebugVisibility();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                showDebugInfo = !showDebugInfo;
                UpdateDebugVisibility();
            }

            if (showDebugInfo && debugText != null)
            {
                UpdateDebugInfo();
            }
        }

        private void UpdateDebugVisibility()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(showDebugInfo);
            }
            else if (debugText != null)
            {
                debugText.gameObject.SetActive(showDebugInfo);
            }
        }

        private void UpdateDebugInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // FPS
            float fps = 1f / Time.deltaTime;
            sb.AppendLine($"<b>FPS:</b> {fps:F1}");
            sb.AppendLine();

            // Game State
            if (GameManager.Instance != null)
            {
                sb.AppendLine($"<b>Game State:</b> {(GameManager.Instance.IsGameStarted() ? "Playing" : "Not Started")}");
                sb.AppendLine($"<b>Paused:</b> {GameManager.Instance.IsGamePaused()}");
                sb.AppendLine($"<b>Game Time:</b> {GameManager.Instance.GetGameTime():F1}s");
                sb.AppendLine($"<b>Score:</b> {GameManager.Instance.GetScore()}");
                sb.AppendLine();
            }

            // Player Info
            if (GameManager.Instance != null && GameManager.Instance.GetPlayer() != null)
            {
                Player player = GameManager.Instance.GetPlayer();
                sb.AppendLine($"<b>Player Speed:</b> {player.GetCurrentSpeed():F2}");
                sb.AppendLine($"<b>Is Jumping:</b> {player.IsJumping()}");
                sb.AppendLine();
            }

            // Enemy Info
            if (GameManager.Instance != null && GameManager.Instance.GetEnemyMass() != null)
            {
                EnemyMass enemy = GameManager.Instance.GetEnemyMass();
                sb.AppendLine($"<b>Enemy HP:</b> {enemy.GetHealthPercentage() * 100:F1}%");

                // Distance Player-Enemy
                if (GameManager.Instance.GetPlayer() != null)
                {
                    Player player = GameManager.Instance.GetPlayer();
                    float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                    sb.AppendLine($"<b>Distance Player-Enemy:</b> {distance:F2}m");
                }
                sb.AppendLine();
            }

            // QTE Info
            if (QTEManager.Instance != null)
            {
                sb.AppendLine($"<b>QTE Active:</b> {QTEManager.Instance.IsQTEActive()}");
                sb.AppendLine($"<b>QTE Successes:</b> {QTEManager.Instance.GetSuccessfulQTECount()}");
                sb.AppendLine();
            }

            // State Machine Info
            if (StateMachine.Instance != null && StateMachine.Instance.GetCurrentState() != null)
            {
                sb.AppendLine($"<b>Current State:</b> {StateMachine.Instance.GetCurrentState().GetType().Name}");
                sb.AppendLine();
            }

            // Controls
            sb.AppendLine("<color=black>[P] Toggle Debug | [ESC] Pause | [R] Restart</color>");
            sb.AppendLine("<color=black>[I] Basic Weapon | [J] Spray Weapon</color>");

            debugText.text = sb.ToString();
        }
    }
}
