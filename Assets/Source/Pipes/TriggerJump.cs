using UnityEngine;

namespace NoScope
{
    public class TriggerJump : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private Transform targetLandingPoint; // Point d'atterrissage sur la prochaine pipe
        [SerializeField] private float arcHeight = 2f; // Hauteur de l'arc parabolique
        [SerializeField] private bool useQTETimeForDuration = true; // Utilise le temps de la QTE comme durée de saut
        [SerializeField] private float manualJumpDuration = 3f; // Durée manuelle si useQTETimeForDuration = false

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"TriggerJump detected: {other.gameObject.name} with tag '{other.tag}'");

            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null && targetLandingPoint != null)
                {
                    Debug.Log("Player detected in TriggerJump - Starting launch sequence");

                    // Déclenche la QTE D'ABORD pour générer _currentTimeLimit
                    if (QTEManager.Instance != null)
                    {
                        QTEManager.Instance.StartQTE();
                    }

                    // Détermine la durée du saut APRÈS la génération de la QTE
                    float jumpDuration = manualJumpDuration;

                    if (useQTETimeForDuration && QTEManager.Instance != null)
                    {
                        // Récupère le temps réel de la QTE qui vient d'être générée
                        jumpDuration = QTEManager.Instance.CurrentTimeLimit;
                    }

                    // Calcule la vélocité pour atteindre le point cible avec un arc parabolique
                    Vector3 launchVelocity = CalculateJumpVelocity(
                        other.transform.position,
                        targetLandingPoint.position,
                        jumpDuration,
                        arcHeight
                    );

                    // Lance le joueur avec la vélocité calculée
                    player.LaunchWithVelocity(launchVelocity, jumpDuration);

                    Debug.Log($"Player launched! Velocity: {launchVelocity}, Duration: {jumpDuration}s, TimeScale: {Time.timeScale}");
                }
                else
                {
                    if (player == null) Debug.LogWarning("TriggerJump: Player component not found!");
                    if (targetLandingPoint == null) Debug.LogWarning("TriggerJump: targetLandingPoint not assigned!");
                }
            }
            else
            {
                Debug.Log($"TriggerJump: Object has wrong tag. Expected 'Player', got '{other.tag}'");
            }
        }



        private Vector3 CalculateJumpVelocity(Vector3 start, Vector3 target, float duration, float height)
        {
            // Calcule la vélocité horizontale (XZ)
            Vector3 horizontalDirection = new Vector3(target.x - start.x, 0, target.z - start.z);
            Vector3 horizontalVelocity = horizontalDirection / duration;

            // Calcule la vélocité verticale avec la formule standard de mouvement parabolique
            // v0 = (h - 0.5*g*t²) / t où h est la hauteur totale (différence + arc)
            float gravity = Physics.gravity.y; // Négatif
            float totalHeight = (target.y - start.y) + height; // Hauteur à atteindre

            // Formule: v0y = (2*h) / t - g*t/2
            float verticalVelocity = (2f * totalHeight) / duration - (gravity * duration) / 2f;

            return new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || targetLandingPoint == null) return;

            // Durée pour le gizmo
            float gizmoDuration = manualJumpDuration;
            if (useQTETimeForDuration && QTEManager.Instance != null && QTEManager.Instance.CurrentTimeLimit > 0)
            {
                gizmoDuration = QTEManager.Instance.CurrentTimeLimit;
            }

            // Dessine la trajectoire prévue
            Gizmos.color = Color.green;
            Vector3 previousPoint = transform.position;

            int segments = 20;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float time = gizmoDuration * t;

                Vector3 velocity = CalculateJumpVelocity(
                    transform.position,
                    targetLandingPoint.position,
                    gizmoDuration,
                    arcHeight
                );

                // Position à ce moment du saut
                Vector3 point = transform.position + velocity * time;
                point.y += -0.5f * Mathf.Abs(Physics.gravity.y) * time * time;

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Dessine le point d'atterrissage
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetLandingPoint.position, 0.5f);

            // Dessine le trigger
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>()?.size ?? Vector3.one);
        }
    }
}
