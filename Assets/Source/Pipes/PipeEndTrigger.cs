using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Trigger placé à la fin d'un Ground pour générer le suivant
    /// </summary>
    public class PipeEndTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[PipeEndTrigger] OnTriggerEnter détecté avec {other.gameObject.name}, tag: {other.tag}");

            // Quand le Player entre dans le trigger, génère le prochain Ground
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[PipeEndTrigger] Player détecté !");

                if (PipeGenerator.Instance != null)
                {
                    Debug.Log($"[PipeEndTrigger] Appel de SpawnNextPipe()");
                    PipeGenerator.Instance.SpawnNextPipe();
                }
                else
                {
                    Debug.LogError($"[PipeEndTrigger] PipeGenerator.Instance est NULL !");
                }
            }
            else
            {
                Debug.Log($"[PipeEndTrigger] Pas le bon tag. Attendu: 'Player', Reçu: '{other.tag}'");
            }
        }
    }
}
