using System.Collections.Generic;
using UnityEngine;

namespace NoScope
{
    public class PipeGenerator : MonoBehaviour
    {
        public static PipeGenerator Instance { get; private set; }

        [Header("Pipe Generation Settings")]
        [SerializeField] private GameObject pipePrefab;
        [SerializeField] private Pipes initialScenePipe; // Pipe déjà présente dans la scène
        [SerializeField] private int initialPipeCount = 5;
        [SerializeField] private int maxActivePipes = 10;
        [SerializeField] private int pipesBeforeRemoval = 4; // Nombre de pipes à garder derrière le joueur
        [SerializeField] private float pipeSpacing = 10f; // Espacement supplémentaire entre pipes
        [SerializeField] private float pipeHeight = 5f;
        [SerializeField] private float horizontalVariation = 3f;

        private LinkedList<Pipes> _activePipes = new LinkedList<Pipes>();
        private Queue<Pipes> _pipePool = new Queue<Pipes>();
        private Vector3 _nextSpawnPosition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializePipePool();

            // Si pipe initiale existe, l'utiliser comme point de départ
            if (initialScenePipe != null)
            {
                _activePipes.AddLast(initialScenePipe);
                float pipeLength = Vector3.Distance(initialScenePipe.startPoint.position, initialScenePipe.endPoint.position);
                _nextSpawnPosition = initialScenePipe.endPoint.position + Vector3.forward * pipeSpacing;
                Debug.Log($"Using initial scene pipe at {initialScenePipe.transform.position}, length: {pipeLength}");

                // Génère les pipes suivantes (une de moins car on a déjà la première)
                for (int i = 0; i < initialPipeCount - 1; i++)
                {
                    SpawnNextPipe();
                }
            }
            else
            {
                _nextSpawnPosition = Vector3.zero; // Position initiale à l'origine
                GenerateInitialPipes();
            }
        }

        private void InitializePipePool()
        {
            // Créer un pool de pipes pour optimiser la mémoire
            for (int i = 0; i < maxActivePipes * 2; i++)
            {
                GameObject pipeObj = Instantiate(pipePrefab, Vector3.zero, Quaternion.identity, transform);
                Pipes pipe = pipeObj.GetComponent<Pipes>();
                pipe.Deactivate();
                _pipePool.Enqueue(pipe);
            }
        }

        private void GenerateInitialPipes()
        {
            for (int i = 0; i < initialPipeCount; i++)
            {
                SpawnNextPipe();
            }
        }

        public void SpawnNextPipe()
        {
            if (_pipePool.Count == 0)
            {
                Debug.LogWarning("Pool de pipes vide !");
                return;
            }

            Pipes newPipe = _pipePool.Dequeue();

            // Calcul de la position : aligne le startPoint de la nouvelle pipe avec _nextSpawnPosition
            // Force la hauteur Y à être identique pour éviter l'escalier
            Vector3 offset = newPipe.transform.position - newPipe.startPoint.position;
            Vector3 targetPosition = _nextSpawnPosition + offset;

            // Force la hauteur Y à celle de la première pipe pour alignement parfait
            if (_activePipes.Count > 0)
            {
                targetPosition.y = _activePipes.First.Value.transform.position.y;
            }

            newPipe.transform.position = targetPosition;
            newPipe.Activate();

            // Liaison dans la liste chaînée
            if (_activePipes.Count > 0)
            {
                _activePipes.Last.Value.nextPipe = newPipe;
            }

            _activePipes.AddLast(newPipe);

            Debug.Log($"Spawned pipe at {newPipe.transform.position}, active pipes: {_activePipes.Count}, pool remaining: {_pipePool.Count}");

            // Prépare la prochaine position basée sur l'endPoint de la pipe actuelle + espacement
            // Force également la hauteur Y pour la prochaine position
            _nextSpawnPosition = newPipe.endPoint.position + Vector3.forward * pipeSpacing;
            if (_activePipes.Count > 0)
            {
                _nextSpawnPosition.y = _activePipes.First.Value.endPoint.position.y;
            }
        }

        private void RemoveOldestPipe()
        {
            if (_activePipes.Count == 0) return;

            Pipes oldestPipe = _activePipes.First.Value;
            _activePipes.RemoveFirst();

            oldestPipe.Deactivate();
            _pipePool.Enqueue(oldestPipe);
        }

        public void CheckPlayerPosition(Vector3 playerPosition)
        {
            if (_activePipes.Count <= pipesBeforeRemoval) return; // Garde au minimum pipesBeforeRemoval pipes

            // Compte combien de pipes le joueur a dépassées
            int pipesPassedCount = 0;
            foreach (Pipes pipe in _activePipes)
            {
                if (pipe.IsPlayerPast(playerPosition))
                {
                    pipesPassedCount++;
                }
                else
                {
                    break; // Arrête dès qu'on trouve une pipe non dépassée
                }
            }

            // Si le joueur a dépassé plus de pipesBeforeRemoval pipes, on supprime les plus anciennes
            int pipesToRemove = pipesPassedCount - pipesBeforeRemoval;
            if (pipesToRemove > 0)
            {
                Debug.Log($"Player passed {pipesPassedCount} pipes, removing {pipesToRemove} old pipes");

                for (int i = 0; i < pipesToRemove; i++)
                {
                    // Spawn une nouvelle pipe pour chaque pipe supprimée
                    SpawnNextPipe();

                    // Supprime la plus ancienne
                    RemoveOldestPipe();
                }
            }
        }

        public Pipes GetCurrentPipe()
        {
            return _activePipes.Count > 0 ? _activePipes.First.Value : null;
        }

        public Pipes GetNextPipe()
        {
            var node = _activePipes.First?.Next;
            return node?.Value;
        }
    }
}
