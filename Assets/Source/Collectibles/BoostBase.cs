using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Classe abstraite pour les boosts que le joueur peut récupérer par collision.
    /// Gère la logique de collision et la destruction après utilisation.
    /// Les classes enfants doivent implémenter l'effet spécifique du boost.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class BoostBase : MonoBehaviour
    {
        [Header("Boost Settings")]
        [SerializeField] protected float rotationSpeed = 50f; // Vitesse de rotation visuelle
        [SerializeField] protected float bobbingSpeed = 1f; // Vitesse d'oscillation verticale
        [SerializeField] protected float bobbingAmount = 0.3f; // Amplitude d'oscillation

        [Header("Audio")]
        [SerializeField] protected AudioClip pickupSound; // Son lors de la récupération

        private Vector3 _startPosition;
        private float _bobbingTimer;

        protected virtual void Start()
        {
            _startPosition = transform.position;

            // S'assure que le collider est en trigger
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        protected virtual void Update()
        {
            // Rotation visuelle
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Oscillation verticale (bobbing)
            _bobbingTimer += Time.deltaTime * bobbingSpeed;
            float newY = _startPosition.y + Mathf.Sin(_bobbingTimer) * bobbingAmount;
            transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Vérifie si c'est le joueur qui entre en collision
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    // Applique l'effet du boost
                    ApplyBoost(player);

                    // Joue le son de récupération si disponible
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }

                    // Détruit le boost après utilisation
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Méthode abstraite que les classes enfants doivent implémenter.
        /// Définit l'effet spécifique du boost sur le joueur.
        /// </summary>
        /// <param name="player">Le joueur qui a récupéré le boost</param>
        protected abstract void ApplyBoost(Player player);

        /// <summary>
        /// Optionnel: Appelé dans OnDrawGizmos pour visualiser le boost dans l'éditeur
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
