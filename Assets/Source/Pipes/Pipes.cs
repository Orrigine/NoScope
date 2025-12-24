using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoScope
{
    public class Pipes : MonoBehaviour
    {
        [Header("Ground Settings")]
        public Transform startPoint;
        public Transform endPoint;
        public BoxCollider rampSpawnArea; // Zone où la rampe peut spawn

        [Header("Ramp Settings")]
        [SerializeField] private GameObject rampPrefab; // Prefab de la rampe avec TriggerJump
        [SerializeField, Range(0f, 100f)] private float rampSpawnChance = 50f; // Pourcentage de chance de spawn

        [Header("Boost Settings")]
        [SerializeField] private GameObject[] boostPrefabs; // Liste des prefabs de boosts possibles
        [SerializeField, Range(0f, 100f)] private float boostSpawnChance = 30f; // Pourcentage de chance de spawn d'un boost
        [SerializeField] private int maxBoostsPerPipe = 2; // Nombre maximum de boosts par pipe

        [HideInInspector]
        public Pipes nextPipe;

        private bool _isActivated = false;
        private GameObject _spawnedRamp = null;
        private List<GameObject> _spawnedBoosts = new List<GameObject>();

        public void Activate()
        {
            _isActivated = true;
            gameObject.SetActive(true);

            // Tente de spawner une rampe avec le pourcentage de chance
            TrySpawnRamp();

            // Tente de spawner des boosts
            TrySpawnBoosts();
        }

        public void Deactivate()
        {
            _isActivated = false;
            gameObject.SetActive(false);

            // Détruit la rampe si elle existe
            if (_spawnedRamp != null)
            {
                Destroy(_spawnedRamp);
                _spawnedRamp = null;
            }

            // Détruit tous les boosts
            foreach (GameObject boost in _spawnedBoosts)
            {
                if (boost != null)
                {
                    Destroy(boost);
                }
            }
            _spawnedBoosts.Clear();
        }

        private void TrySpawnRamp()
        {
            // Vérifie si on doit spawner une rampe (pourcentage de chance)
            if (rampPrefab == null || rampSpawnArea == null)
            {
                return;
            }

            // Génère un nombre aléatoire entre 0 et 100
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            if (randomValue <= rampSpawnChance)
            {
                SpawnRamp();
            }
        }

        private void SpawnRamp()
        {
            if (_spawnedRamp != null)
            {
                Destroy(_spawnedRamp);
            }

            // Calcule une position aléatoire dans la zone définie par le BoxCollider
            Vector3 spawnAreaSize = rampSpawnArea.size;

            // Position aléatoire dans la zone (axes X et Z) en local
            float randomX = UnityEngine.Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomZ = UnityEngine.Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);

            // Crée la position locale relative au centre du BoxCollider
            Vector3 localPosition = new Vector3(randomX, 0f, randomZ);

            // Transforme en position monde (le center du BoxCollider est déjà pris en compte par Transform)
            Vector3 worldPosition = rampSpawnArea.transform.TransformPoint(rampSpawnArea.center + localPosition);

            // Spawne la rampe SANS parent pour garder son scale et rotation du prefab
            // Le Ground garde juste une référence pour la détruire plus tard
            _spawnedRamp = Instantiate(rampPrefab, worldPosition, rampPrefab.transform.rotation);
        }

        /// <summary>
        /// Tente de spawner des boosts sur le ground (pas sur les rampes)
        /// </summary>
        private void TrySpawnBoosts()
        {
            if (boostPrefabs == null || boostPrefabs.Length == 0 || rampSpawnArea == null)
            {
                return;
            }

            // Génère un nombre aléatoire entre 0 et 100
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            if (randomValue <= boostSpawnChance)
            {
                // Détermine le nombre de boosts à spawner
                int boostCount = UnityEngine.Random.Range(1, maxBoostsPerPipe + 1);

                for (int i = 0; i < boostCount; i++)
                {
                    SpawnBoost();
                }
            }
        }

        /// <summary>
        /// Spawne un boost aléatoire dans la zone du ground, en évitant la rampe si elle existe
        /// </summary>
        private void SpawnBoost()
        {
            // Sélectionne un prefab de boost aléatoire
            GameObject randomBoostPrefab = boostPrefabs[UnityEngine.Random.Range(0, boostPrefabs.Length)];

            if (randomBoostPrefab == null)
            {
                return;
            }

            // Calcule une position aléatoire dans la zone définie par le BoxCollider
            Vector3 spawnAreaSize = rampSpawnArea.size;

            // Position aléatoire dans la zone (axes X et Z) en local
            float randomX = UnityEngine.Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomZ = UnityEngine.Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);

            // Crée la position locale relative au centre du BoxCollider
            // Ajoute un offset en Y pour que le boost soit légèrement au-dessus du ground
            Vector3 localPosition = new Vector3(randomX, 1f, randomZ);

            // Transforme en position monde
            Vector3 worldPosition = rampSpawnArea.transform.TransformPoint(rampSpawnArea.center + localPosition);

            // Vérifie qu'on ne spawn pas trop près de la rampe si elle existe
            if (_spawnedRamp != null)
            {
                float distanceToRamp = Vector3.Distance(worldPosition, _spawnedRamp.transform.position);

                // Si trop proche de la rampe, essaie une autre position (récursion limitée)
                if (distanceToRamp < 3f && _spawnedBoosts.Count < maxBoostsPerPipe * 3)
                {
                    SpawnBoost();
                    return;
                }
            }

            // Spawne le boost
            GameObject spawnedBoost = Instantiate(randomBoostPrefab, worldPosition, randomBoostPrefab.transform.rotation);
            _spawnedBoosts.Add(spawnedBoost);
        }
    }
}
