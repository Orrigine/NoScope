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

            // Quand le Player entre dans le trigger, génère le prochain Ground
            if (other.CompareTag("Player"))
            {
                if (PipeGenerator.Instance != null)
                {
                    PipeGenerator.Instance.SpawnNextPipe();
                    PipeGenerator.Instance.AskRemoveOldestPipe();
                }
                else
                {
                    Debug.LogError("[PipeEndTrigger] PipeGenerator.Instance est null !");
                }
            }
        }
    }
}
