using UnityEngine;

namespace NoScope
{
    public class SmallEnemy : EnemyBase
    {
        [Header("Mesh Settings")]
        [SerializeField] private GameObject[] zombieMeshPrefabs; // SM_Zombie_A, B, C, D
        [SerializeField] private Transform meshParent; // Parent où instancier le mesh (optionnel)

        [Header("Movement Behavior")]
        [SerializeField] private float convergenceDistance = 15f; // Distance à laquelle le zombie commence à converger vers le joueur
        [SerializeField] private float speedBoostChance = 0.2f; // 20% de chance d'avoir un boost de vitesse
        [SerializeField] private float speedBoostMultiplier = 2.5f; // Multiplicateur de vitesse pour les zombies boostés

        private Vector3 _spawnDirection; // Direction initiale du spawn
        private bool _isConverging = false; // Si le zombie converge vers le joueur

        protected override void Start()
        {
            base.Start();

            // Sauvegarde la direction de spawn (axe X positif car la map est tournée)
            _spawnDirection = Vector3.right;

            // Instancie aléatoirement un des meshes de zombie
            SpawnRandomZombieMesh();

            // Chance aléatoire d'avoir un boost de vitesse
            if (Random.value < speedBoostChance)
            {
                moveSpeed *= speedBoostMultiplier;
            }
        }

        private void SpawnRandomZombieMesh()
        {
            if (zombieMeshPrefabs == null || zombieMeshPrefabs.Length == 0)
            {
                Debug.LogWarning("[SmallEnemy] Aucun mesh de zombie assigné dans l'inspecteur!");
                return;
            }

            // Sélectionne un mesh aléatoire
            int randomIndex = Random.Range(0, zombieMeshPrefabs.Length);
            GameObject selectedMesh = zombieMeshPrefabs[randomIndex];

            if (selectedMesh != null)
            {
                // Détermine le parent (soit le transform spécifié, soit ce GameObject)
                Transform parent = meshParent != null ? meshParent : transform;

                // Instancie le mesh comme enfant
                GameObject meshInstance = Instantiate(selectedMesh, parent);
                meshInstance.transform.localPosition = Vector3.zero;
                meshInstance.transform.localRotation = Quaternion.identity;

            }
            else
            {
                Debug.LogWarning($"[SmallEnemy] Le mesh à l'index {randomIndex} est null!");
            }
        }

        protected override void FollowPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = player.transform.position;
            float distanceToPlayer = Vector3.Distance(currentPosition, targetPosition);

            // Détermine si le zombie doit converger vers le joueur
            if (distanceToPlayer <= convergenceDistance)
            {
                _isConverging = true;
            }

            Vector3 direction;

            if (_isConverging)
            {
                // Converge vers le joueur (comportement normal)
                targetPosition.y = currentPosition.y;
                direction = (targetPosition - currentPosition).normalized;

                // LookAt vers le joueur
                Vector3 lookAtTarget = player.transform.position;
                lookAtTarget.y = currentPosition.y;
                transform.LookAt(lookAtTarget);
            }
            else
            {
                // Continue dans la direction de spawn
                direction = _spawnDirection;

                // Garde la rotation initiale du spawn
                transform.rotation = Quaternion.LookRotation(_spawnDirection);
            }

            // Déplace dans la direction calculée
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Inflige des dégâts au joueur
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    // Damage handled by Player's trigger to ensure exactly one life unit is removed per collision.
                    // We keep this hook for future effects (knockback, sound), but avoid double-decrement.
                }
            }
        }

        protected override void Die()
        {
            // Ajoute des points au score avant de mourir
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScoreForEnemyKill();
            }
            else
            {
                Debug.LogError("[SmallEnemy] GameManager.Instance est null, impossible d'ajouter du score !");
            }

            base.Die();
        }
    }
}
