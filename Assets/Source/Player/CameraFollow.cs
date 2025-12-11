using UnityEngine;
using UnityEngine.InputSystem;

namespace NoScope
{
    /// <summary>
    /// Caméra qui suit le joueur avec un offset configurable et rotation souris
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
        [SerializeField] private float smoothSpeed = 0.125f;

        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 10f;
        [SerializeField] private float verticalRotationLimit = 45f; // Limite de rotation verticale

        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 0.3f;

        private Vector3 _initialOffset;
        private float _shakeTimeRemaining = 0f;
        private Vector3 _shakeOffset = Vector3.zero;
        private float _currentHorizontalRotation = 0f; // Rotation horizontale actuelle
        private float _currentVerticalRotation = 0f; // Rotation verticale actuelle
        private Vector3 _velocity = Vector3.zero; // Pour SmoothDamp

        private void Start()
        {
            _initialOffset = offset;

            // Si pas de target assigné, cherche le joueur
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // S'abonne aux events pour le screen shake
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameLose += () => ShakeCamera(0.5f, 0.5f);
                GameManager.Instance.OnGameWin += () => ShakeCamera(0.3f, 0.3f);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                // Essaie de retrouver le joueur
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    return;
                }
            }

            // Ne gère pas les inputs de souris pendant la pause ou la QTE
            bool isGamePaused = GameManager.Instance != null && GameManager.Instance.IsGamePaused();
            bool isQTEActive = QTEManager.Instance != null && QTEManager.Instance.IsQTEActive();

            if (!isGamePaused && !isQTEActive)
            {
                HandleMouseLook();
            }

            FollowTarget();
            UpdateCameraShake();
        }

        private void HandleMouseLook()
        {
            // Récupère le mouvement de la souris (horizontal et vertical)
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
                float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

                _currentHorizontalRotation += mouseX;
                _currentVerticalRotation -= mouseY; // Inverse pour comportement naturel

                // Limite la rotation verticale
                _currentVerticalRotation = Mathf.Clamp(_currentVerticalRotation, -verticalRotationLimit, verticalRotationLimit);
            }
        }

        private void FollowTarget()
        {
            // Calcule l'offset avec rotation horizontale et verticale
            Quaternion rotation = Quaternion.Euler(_currentVerticalRotation, _currentHorizontalRotation, 0);
            Vector3 rotatedOffset = rotation * offset;

            Vector3 desiredPosition = target.position + rotatedOffset + _shakeOffset;

            // Utilise SmoothDamp au lieu de Lerp pour un mouvement plus stable
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothSpeed);

            // Rotation smooth vers le target
            Vector3 directionToTarget = target.position - transform.position;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * 10f);
            }
        }

        private void UpdateCameraShake()
        {
            if (_shakeTimeRemaining > 0)
            {
                _shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                _shakeTimeRemaining -= Time.deltaTime;
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }
        }

        public void ShakeCamera(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
            _shakeTimeRemaining = duration;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
    }
}
