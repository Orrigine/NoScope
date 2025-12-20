using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoScope
{
    public class EnemyMass : EnemyBase
    {
        [Header("Mass Enemy Settings")]
        [SerializeField] private GameObject smallEnemyPrefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxSmallEnemies = 5;
        [SerializeField] private Transform zombieSpawnArea; // Zone de spawn des zombies

        [Header("Speed Boost Settings")]
        [SerializeField] private float speedIncreasePerFailedQTE = 1f; // Augmentation de vitesse par QTE ratéex
        [SerializeField] private float speedDecayRate = 0.05f; // Décroissance de vitesse par seconde 

        [Header("Catch-up Settings")]
        [SerializeField] private float maxDistanceBeforeCatchUp = 30f; // Distance max avant que l'ennemi rattrape
        [SerializeField] private float catchUpSpeedMultiplier = 1.5f; // Multiplicateur de vitesse quand il rattrape (réduit pour sentir la perte)

        private float _baseSpeed; // Vitesse de départ

        private List<GameObject> _activeSmallEnemies = new List<GameObject>();
        private Transform _playerTransform;
        private float _nextSpawnTime;

        protected override void Start()
        {
            base.Start();

            // Sauvegarde la vitesse de base
            _baseSpeed = moveSpeed;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            // S'abonne aux événements QTE
            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.OnQTEComplete += OnQTEComplete;
            }
        }

        private void OnDestroy()
        {
            // Se désabonne des événements
            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.OnQTEComplete -= OnQTEComplete;
            }
        }

        private void OnQTEComplete(bool success)
        {
            // Augmente la vitesse seulement si la QTE est ratée
            if (!success)
            {
                IncreaseSpeed();
            }
        }

        protected override void Update()
        {
            base.Update();

            // Ajuste la vitesse en fonction de la distance au joueur
            AdjustSpeedBasedOnDistance();

            // Decay de vitesse (revient lentement vers la vitesse de base)
            DecaySpeed();

            // Spawne des petites unités périodiquement
            if (Time.time >= _nextSpawnTime && _activeSmallEnemies.Count < maxSmallEnemies)
            {
                SpawnSmallEnemy();
                _nextSpawnTime = Time.time + spawnInterval;
            }

            // Nettoie les ennemis détruits
            _activeSmallEnemies.RemoveAll(e => e == null);
        }

        private void AdjustSpeedBasedOnDistance()
        {
            if (_playerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            // Si le joueur est trop loin, l'ennemi accélère pour rattraper
            if (distanceToPlayer > maxDistanceBeforeCatchUp)
            {
                // Applique directement la vitesse de rattrapage
                moveSpeed = _baseSpeed * catchUpSpeedMultiplier;
                Debug.Log($"Catching up! Applied speed: {moveSpeed}");
            }
        }

        private void DecaySpeed()
        {
            // Si la vitesse est supérieure à la vitesse de base, décroît lentement
            if (moveSpeed > _baseSpeed)
            {
                moveSpeed = Mathf.Max(moveSpeed - speedDecayRate * Time.deltaTime, _baseSpeed);
            }
        }

        private void SpawnSmallEnemy()
        {
            if (smallEnemyPrefab == null) return;

            Vector3 spawnPosition;

            // Si ZombieSpawnArea est assignée, spawn dans sa zone
            if (zombieSpawnArea != null)
            {
                // Récupère le BoxCollider ou SphereCollider de la zone de spawn
                Collider spawnCollider = zombieSpawnArea.GetComponent<Collider>();

                if (spawnCollider != null)
                {
                    // Génère une position aléatoire dans les bounds du collider
                    Bounds bounds = spawnCollider.bounds;
                    spawnPosition = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x),
                        zombieSpawnArea.position.y, // Garde la hauteur de la zone
                        Random.Range(bounds.min.z, bounds.max.z)
                    );
                }
                else
                {
                    // Fallback: spawn directement à la position de la zone
                    spawnPosition = zombieSpawnArea.position;
                    Debug.LogWarning("[EnemyMass] ZombieSpawnArea n'a pas de Collider, spawn à sa position exacte");
                }
            }
            else
            {
                // Fallback: spawn autour de l'EnemyMass (ancien comportement)
                spawnPosition = transform.position + Random.insideUnitSphere * 5f;
                spawnPosition.y = transform.position.y;
                Debug.LogWarning("[EnemyMass] ZombieSpawnArea non assignée, spawn autour de l'EnemyMass");
            }

            GameObject smallEnemy = Instantiate(smallEnemyPrefab, spawnPosition, Quaternion.identity);
            _activeSmallEnemies.Add(smallEnemy);
        }

        protected override void Die()
        {
            // Détruit toutes les petites unités
            foreach (var enemy in _activeSmallEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _activeSmallEnemies.Clear();

            base.Die();

            // Déclenche la victoire
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
        }

        public void IncreaseSpeed()
        {
            moveSpeed += speedIncreasePerFailedQTE;
        }
    }
}
