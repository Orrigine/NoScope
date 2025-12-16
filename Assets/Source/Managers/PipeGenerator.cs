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
                _nextSpawnPosition = initialScenePipe.endPoint.position + Vector3.forward * pipeSpacing;
                Debug.Log($"Using initial scene pipe at {initialScenePipe.transform.position}");

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

            // Place la pipe de sorte que son startPoint soit exactement à _nextSpawnPosition
            // Calcule la différence entre la position du transform et celle du startPoint
            Vector3 offset = newPipe.startPoint.position - newPipe.transform.position;
            Vector3 targetPosition = _nextSpawnPosition - offset;

            newPipe.transform.position = targetPosition;
            newPipe.Activate();

            // Liaison dans la liste chaînée
            if (_activePipes.Count > 0)
            {
                _activePipes.Last.Value.nextPipe = newPipe;
            }

            _activePipes.AddLast(newPipe);

            Debug.Log($"Spawned pipe at {newPipe.transform.position}, startPoint: {newPipe.startPoint.position}, endPoint: {newPipe.endPoint.position}, next spawn: {_nextSpawnPosition}");

            // Prépare la prochaine position = endPoint de cette pipe + espacement
            _nextSpawnPosition = newPipe.endPoint.position + Vector3.forward * pipeSpacing;
        }

        public void AskRemoveOldestPipe()
        {
            if (_activePipes.Count <= pipesBeforeRemoval)
            {
                Debug.Log($"Pas assez de pipes pour en supprimer. Actif: {_activePipes.Count}, Minimum requis: {pipesBeforeRemoval + 1}");
                return;
            }

            RemoveOldestPipe();
        }

        private void RemoveOldestPipe()
        {
            if (_activePipes.Count == 0) return;

            Pipes oldestPipe = _activePipes.First.Value;
            _activePipes.RemoveFirst();

            oldestPipe.Deactivate();
            _pipePool.Enqueue(oldestPipe);
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
