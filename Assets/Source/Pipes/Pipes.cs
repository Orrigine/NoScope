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

        [HideInInspector]
        public Pipes nextPipe;

        private bool _isActivated = false;
        private GameObject _spawnedRamp = null;

        public void Activate()
        {
            _isActivated = true;
            gameObject.SetActive(true);

            // Tente de spawner une rampe avec le pourcentage de chance
            TrySpawnRamp();
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

        public bool IsPlayerPast(Vector3 playerPosition)
        {
            // Vérifie si le joueur a dépassé l'endPoint de cette pipe sur l'axe Z
            return playerPosition.z > endPoint.position.z;
        }
    }
}
