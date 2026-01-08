using UnityEngine;
using DG.Tweening;

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
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null && targetLandingPoint != null)
                {
                    // Déclenche la QTE D'ABORD pour générer _currentTimeLimit
                    if (QTEManager.Instance != null)
                    {
                        QTEManager.Instance.StartQTE();
                    }
                    else
                    {
                        Debug.LogError("[TriggerJump] QTEManager.Instance est NULL!");
                    }

                    // Détermine la durée du saut APRÈS la génération de la QTE
                    float jumpDuration = manualJumpDuration;

                    if (useQTETimeForDuration && QTEManager.Instance != null)
                    {
                        // Récupère le temps réel de la QTE qui vient d'être générée
                        jumpDuration = QTEManager.Instance.CurrentTimeLimit;
                    }

                    // Calcule les points de la trajectoire parabolique
                    Vector3 startPos = other.transform.position;
                    Vector3 endPos = targetLandingPoint.position;

                    // Crée une vraie parabole avec plusieurs points intermédiaires
                    int numPoints = 20; // Nombre de points pour la courbe (augmenté pour plus de précision)
                    Vector3[] path = new Vector3[numPoints];

                    float distance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(endPos.x, 0, endPos.z));

                    for (int i = 0; i < numPoints; i++)
                    {
                        float t = i / (float)(numPoints - 1); // 0 à 1

                        // Position horizontale (linéaire)
                        Vector3 point = Vector3.Lerp(startPos, endPos, t);

                        // Hauteur parabolique : atteint arcHeight au milieu (t=0.5)
                        // Formule: y = -4 * arcHeight * (t - 0.5)^2 + arcHeight
                        float parabola = -4f * arcHeight * Mathf.Pow(t - 0.5f, 2f) + arcHeight;
                        point.y += parabola;

                        path[i] = point;
                    }

                    // Lance le joueur avec DOTween
                    player.LaunchWithDOTween(path, jumpDuration);


                }
                else
                {
                    if (player == null) Debug.LogWarning("TriggerJump: Player component not found!");
                    if (targetLandingPoint == null) Debug.LogWarning("TriggerJump: targetLandingPoint not assigned!");
                }
            }
            else
            {

            }
        }



        private void OnDrawGizmos()
        {
            if (!drawGizmos || targetLandingPoint == null) return;

            // Dessine la trajectoire parabolique prévue (même formule que le saut réel)
            Gizmos.color = Color.green;

            Vector3 startPos = transform.position;
            Vector3 endPos = targetLandingPoint.position;

            int numPoints = 20;
            Vector3 previousPoint = startPos;

            for (int i = 1; i <= numPoints; i++)
            {
                float t = i / (float)numPoints;
                Vector3 point = Vector3.Lerp(startPos, endPos, t);

                // Formule parabolique identique au saut
                float parabola = -4f * arcHeight * Mathf.Pow(t - 0.5f, 2f) + arcHeight;
                point.y += parabola;

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Dessine le point d'atterrissage
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetLandingPoint.position, 0.5f);

            // Dessine le trigger 
            Gizmos.color = Color.yellow;
            MeshCollider meshCol = GetComponent<MeshCollider>();
            if (meshCol != null && meshCol.sharedMesh != null)
            {
                Gizmos.DrawWireMesh(meshCol.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }
    }
}
