using UnityEngine;

namespace NoScope
{
    public class SmallEnemy : EnemyBase
    {
        [Header("Mesh Settings")]
        [SerializeField] private GameObject[] zombieMeshPrefabs; // SM_Zombie_A, B, C, D
        [SerializeField] private Transform meshParent; // Parent où instancier le mesh (optionnel)

        protected override void Start()
        {
            base.Start();

            // Instancie aléatoirement un des meshes de zombie
            SpawnRandomZombieMesh();
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
            base.FollowPlayer();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Inflige des dégâts au joueur
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.DecrementHealth();
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
