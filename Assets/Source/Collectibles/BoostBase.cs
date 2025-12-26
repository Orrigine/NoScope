using System;
using System.Collections;
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
        [SerializeField] protected float duration = 5f; // Durée du boost en secondes
        [SerializeField] protected float rotationSpeed = 50f; // Vitesse de rotation visuelle
        [SerializeField] protected float bobbingSpeed = 1f; // Vitesse d'oscillation verticale
        [SerializeField] protected float bobbingAmount = 0.3f; // Amplitude d'oscillation

        [Header("Audio")]
        [SerializeField] protected AudioClip pickupSound; // Son lors de la récupération

        private Vector3 _startPosition;
        private float _bobbingTimer;
        protected Player _boostedPlayer;

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

        private Coroutine _activeCoroutine;
        private bool _activeCoroutineOnManager = false;

        private void OnTriggerEnter(Collider other)
        {
            // Vérifie si c'est le joueur qui entre en collision
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    _boostedPlayer = player;

                    // Applique l'effet du boost
                    ApplyBoost(player, duration);

                    // Joue le son de récupération si disponible
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }

                    // Stop l'ancienne coroutine si elle existe et relance le timer
                    if (_activeCoroutine != null)
                    {
                        if (_activeCoroutineOnManager && GameManager.Instance != null)
                            GameManager.Instance.StopDelayed(_activeCoroutine);
                        else
                            StopCoroutine(_activeCoroutine);
                        _activeCoroutine = null;
                        _activeCoroutineOnManager = false;
                    }

                    // Prépare l'action de revert fournie par l'implémentation
                    var revertAction = GetRevertAction(); // Doit être totalement stateless

                    // Si GameManager est présent (objet persistant), exécute l'action différée dessus
                    if (GameManager.Instance != null)
                    {
                        _activeCoroutine = GameManager.Instance.RunDelayed(duration, () =>
                        {
                            try
                            {
                                revertAction?.Invoke(player);
                            }
                            catch (MissingReferenceException ex)
                            {
                                Debug.LogWarning($"[BoostBase] MissingReferenceException lors du revert : {ex.Message}");
                            }
                        });
                        _activeCoroutineOnManager = true;
                    }
                    else
                    {
                        // Fallback: coroutine stateless (méthode static) pour ne pas capturer 'this'
                        _activeCoroutine = StartCoroutine(DelayAndInvoke(duration, () =>
                        {
                            try
                            {
                                revertAction?.Invoke(player);
                            }
                            catch (MissingReferenceException ex)
                            {
                                Debug.LogWarning($"[BoostBase] MissingReferenceException lors du revert : {ex.Message}");
                            }
                        }));
                        _activeCoroutineOnManager = false;
                    }

                    // Cache le visuel du boost touché
                    foreach (var mesh in GetComponentsInChildren<MeshRenderer>())
                        mesh.enabled = false;
                    foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
                        sprite.enabled = false;
                }
            }
        }

        private static IEnumerator DelayAndInvoke(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BoostBase] Erreur lors de l'exécution de l'action différée: {ex}");
            }
        }

        /// <summary>
        /// Méthode abstraite que les classes enfants doivent implémenter.
        /// Définit l'effet spécifique du boost sur le joueur.
        /// </summary>
        /// <param name="player">Le joueur qui a récupéré le boost</param>
        /// <param name="boostDuration">La durée du boost en secondes</param>
        protected abstract void ApplyBoost(Player player, float boostDuration);

        /// <summary>
        /// Méthode abstraite pour annuler l'effet du boost.
        /// Appelée automatiquement après la durée du boost.
        /// </summary>
        /// <param name="player">Le joueur qui avait le boost</param>
        protected abstract void RevertBoost(Player player);

        /// <summary>
        /// Fournit une action indépendante de l'instance pour effectuer le revert.
        /// Utile pour exécuter le revert depuis un objet persistant si le collectible est détruit.
        /// </summary>
        /// <returns>Action qui prend le Player en paramètre</returns>
        protected abstract System.Action<Player> GetRevertAction();

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
