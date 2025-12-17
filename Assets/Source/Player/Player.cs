using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Runtime.CompilerServices;

namespace NoScope
{
    public class Player : MonoBehaviour
    {

        [Header("Values")]
        [SerializeField] public int Life = 3;

        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float speedIncreasePerQTE = 2f;
        [SerializeField] private float speedDecayRate = 0.5f;
        [SerializeField] private float lateralSpeed = 5f;
        [SerializeField] private float lateralRange = 3f;

        [Header("QTE Failure Penalty")]
        [SerializeField] private float speedLossOnFailedQTE = 3f; // Perte de vitesse sur échec
        [SerializeField] private float speedRecoveryRate = 1f; // Récupération de vitesse par seconde

        [Header("Consecutive Success Bonus")]
        [SerializeField] private float streakBonusMultiplier = 0.5f; // Bonus additionnel par succès consécutif

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float airTime = 1f;

        [Header("Shooting Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform[] gunPoints;
        [SerializeField] private float baseBulletsPerSecond = 2f; // Nombre de projectiles par seconde (base)
        [SerializeField] private float maxBulletsPerSecond = 10f; // Nombre maximum de projectiles par seconde
        [SerializeField] private float minBulletsPerSecond = 1f; // Nombre minimum de projectiles par seconde (limite absolue)
        [SerializeField] private float fireRateIncreasePerQTE = 0.5f; // Augmentation en projectiles/sec par QTE
        [SerializeField] private float fireRateDecayRate = 0.1f; // Réduction en projectiles/sec par seconde

        [Header("References")]
        [SerializeField] private Rigidbody rb;

        // Events
        public event Action OnPlayerDie;

        // Private variables
        private float _currentSpeed;
        private float _currentBulletsPerSecond; // Projectiles par seconde (plus élevé = plus rapide)
        private float _nextFireTime;
        private bool _isJumping = false;
        private float _jumpTimer = 0f;
        private float _jumpDuration = 0f; // Durée du saut en cours
        private Tween _activeTween; // Tween DOTween actif
        private Vector3 _velocity;
        private int _consecutiveQTESuccesses = 0;
        private float _lastQTEStartTime = -10f; // Cooldown pour éviter les doubles appels
        private bool _isRecoveringFromFailure = false; // Indique si le joueur récupère d'un échec
        private bool _isRecoveringFireRate = false; // Indique si le fire rate récupère après division

        private void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            // Force la gravité et vérifie les contraintes
            rb.useGravity = true;
            Debug.Log($"Player Start: useGravity={rb.useGravity}, constraints={rb.constraints}, mass={rb.mass}");

            _currentSpeed = baseSpeed;
            _currentBulletsPerSecond = baseBulletsPerSecond;

            // S'abonne aux événements QTE
            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.OnQTEComplete += OnQTEComplete;
            }
        }

        private void OnDestroy()
        {
            // Nettoie le tween avant destruction
            _activeTween?.Kill();

            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.OnQTEComplete -= OnQTEComplete;
            }
        }

        private void Update()
        {
            // Ne fait rien si le jeu est en pause
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            {
                return;
            }

            // Le mouvement continue pendant la QTE pour permettre le saut
            Move();

            // Ne tire pas et ne decay pas pendant la QTE
            bool isQTEActive = QTEManager.Instance != null && QTEManager.Instance.IsQTEActive();
            if (!isQTEActive)
            {
                Shoot();
                DecayStats();
            }
        }

        private void Move()
        {
            // Pendant le saut, DOTween gère tout le mouvement
            if (_isJumping)
            {
                return;
            }

            // Mouvement normal hors saut
            _velocity = transform.forward * _currentSpeed;

            // Mouvement latéral (Q/A pour gauche, D pour droite)
            float horizontalInput = 0f;

            // Utilise Q/A et D (supporte QWERTY et AZERTY)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed || Keyboard.current.aKey.isPressed)
                {
                    horizontalInput = -1f;
                }
                else if (Keyboard.current.dKey.isPressed)
                {
                    horizontalInput = 1f;
                }
            }

            // Applique mouvement latéral
            if (horizontalInput != 0f)
            {
                _velocity += transform.right * horizontalInput * lateralSpeed;
            }

            // Applique la vélocité au rigidbody (seulement hors saut)
            rb.linearVelocity = _velocity;

            // NOTE: La génération de pipes est maintenant gérée par des triggers sur chaque Ground
            // Plus besoin de vérifier à chaque frame
            // if (PipeGenerator.Instance != null)
            // {
            //     PipeGenerator.Instance.CheckPlayerPosition(transform.position);
            // }
        }

        public void Jump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _velocity.y = jumpForce;
            }
        }

        /// <summary>
        /// Lance le joueur sur une trajectoire parabolique avec DOTween
        /// </summary>
        public void LaunchWithDOTween(Vector3[] path, float duration)
        {
            if (_isJumping)
            {
                Debug.LogWarning("LaunchWithDOTween called but player is already jumping!");
                return;
            }

            _isJumping = true;
            _jumpTimer = 0f;
            _jumpDuration = duration;

            // Tue le tween précédent s'il existe
            _activeTween?.Kill();

            // Crée le tween avec un path parabolique (Linear pour suivre exactement les points)
            _activeTween = transform.DOPath(path, duration, PathType.Linear)
                .SetEase(Ease.Linear) // Linear pour une vitesse constante le long du path
                .SetUpdate(UpdateType.Normal, true) // useUnscaledTime = true pour ignorer timeScale
                .OnComplete(() =>
                {
                    _isJumping = false;
                    _jumpTimer = 0f;
                    _activeTween = null;
                    Debug.Log("Jump completed via DOTween");
                });

            Debug.Log($"DOTween jump started: duration={duration:F2}s, path points={path.Length}");
        }

        private void Shoot()
        {
            // Ne tire pas pendant la QTE
            if (QTEManager.Instance != null && QTEManager.Instance.IsQTEActive())
                return;

            if (Time.time >= _nextFireTime)
            {
                FireBullets();
                // Convertit bullets/sec en intervalle de temps (1 / bullets par seconde = secondes par bullet)
                float fireInterval = 1f / Mathf.Max(_currentBulletsPerSecond, 0.1f);
                _nextFireTime = Time.time + fireInterval;
            }
        }

        private void FireBullets()
        {
            if (bulletPrefab == null || gunPoints == null || gunPoints.Length == 0)
                return;

            foreach (Transform gunPoint in gunPoints)
            {
                if (gunPoint != null)
                {
                    GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation);

                    // Ajoute une vélocité au projectile (vers l'arrière)
                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        bulletRb.linearVelocity = -transform.forward * 20f;
                    }

                    Destroy(bullet, 5f);
                }
            }
        }

        private void OnQTEComplete(bool success)
        {
            if (success)
            {
                _consecutiveQTESuccesses++;
                _isRecoveringFromFailure = false;

                // Calcule le bonus avec streak : bonus de base + (streak - 1) * multiplicateur
                float speedBonus = speedIncreasePerQTE + ((_consecutiveQTESuccesses - 1) * streakBonusMultiplier);
                float fireRateBonus = fireRateIncreasePerQTE + ((_consecutiveQTESuccesses - 1) * streakBonusMultiplier * 0.1f);

                // Augmente la vitesse avec bonus progressif
                _currentSpeed = Mathf.Min(_currentSpeed + speedBonus, maxSpeed);

                // Améliore le fire rate avec bonus progressif
                _currentBulletsPerSecond = Mathf.Min(_currentBulletsPerSecond + fireRateBonus, maxBulletsPerSecond);

                Debug.Log($"QTE Success #{_consecutiveQTESuccesses}! Speed: {_currentSpeed} (+{speedBonus}), Bullets/sec: {_currentBulletsPerSecond} (+{fireRateBonus})");
            }
            else
            {
                _consecutiveQTESuccesses = 0;

                // Pénalité : perte de vitesse sur échec
                _currentSpeed = Mathf.Max(_currentSpeed - speedLossOnFailedQTE, minSpeed); // Ne peut pas descendre en dessous du minimum absolu
                _isRecoveringFromFailure = true;

                // Pénalité fire rate : divise par 2
                _currentBulletsPerSecond = Mathf.Max(_currentBulletsPerSecond / 2f, minBulletsPerSecond); // Ne descend pas en dessous du minimum absolu
                _isRecoveringFireRate = true;

                Debug.Log($"QTE Failed! Speed reduced to: {_currentSpeed}, Fire rate divided by 2: {_currentBulletsPerSecond}");
            }
        }

        private void DecayStats()
        {
            // Si le joueur récupère d'un échec, ramène la vitesse vers la base
            if (_isRecoveringFromFailure && _currentSpeed < baseSpeed)
            {
                _currentSpeed = Mathf.Min(_currentSpeed + speedRecoveryRate * Time.deltaTime, baseSpeed);

                // Arrête la récupération une fois la vitesse de base atteinte
                if (_currentSpeed >= baseSpeed)
                {
                    _isRecoveringFromFailure = false;
                }
            }
            // Sinon, diminue progressivement les bonus
            else if (_currentSpeed > baseSpeed)
            {
                _currentSpeed = Mathf.Max(_currentSpeed - speedDecayRate * Time.deltaTime, baseSpeed);
            }

            // Récupération du fire rate après échec QTE
            if (_isRecoveringFireRate && _currentBulletsPerSecond < baseBulletsPerSecond)
            {
                _currentBulletsPerSecond = Mathf.Min(_currentBulletsPerSecond + speedRecoveryRate * 0.2f * Time.deltaTime, baseBulletsPerSecond);

                // Arrête la récupération une fois le fire rate de base atteint
                if (_currentBulletsPerSecond >= baseBulletsPerSecond)
                {
                    _isRecoveringFireRate = false;
                }
            }
            // Decay dégressif du fire rate (plus le fire rate est élevé, plus le decay est rapide)
            else if (_currentBulletsPerSecond > baseBulletsPerSecond)
            {
                float excessFireRate = _currentBulletsPerSecond - baseBulletsPerSecond;
                float decayMultiplier = 1f + (excessFireRate / maxBulletsPerSecond); // Le decay augmente avec l'excès

                _currentBulletsPerSecond = Mathf.Max(
                    _currentBulletsPerSecond - fireRateDecayRate * decayMultiplier * Time.deltaTime,
                    baseBulletsPerSecond
                );
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore les bullets
            if (other.CompareTag("Bullet"))
                return;

            if (other.CompareTag("Enemy"))
            {
                Die();
            }
            else if (other.CompareTag("TriggerJump"))
            {

                // Cooldown pour éviter les doubles appels (trigger multiple frames)
                if (Time.time - _lastQTEStartTime < 2f)
                {
                    return;
                }

                // Auto-jump à la fin du pipe
                Jump();

                // Déclenche la QTE
                if (QTEManager.Instance != null && !QTEManager.Instance.IsQTEActive())
                {
                    _lastQTEStartTime = Time.time;
                    QTEManager.Instance.StartQTE();
                }
               
            }
        }

        private void Die()
        {
            OnPlayerDie?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseGame();
            }
        }

        public float GetCurrentSpeed()
        {
            return _currentSpeed;
        }

        public bool IsJumping()
        {
            return _isJumping;
        }

        public void ResetVelocityToForward()
        {
            // Réinitialise la vélocité pour mouvement forward uniquement
            // Utilisé après une QTE pour éviter les mouvements résiduels
            _velocity = transform.forward * _currentSpeed;
            _velocity.y = 0f;


            _isJumping = false;
            _jumpTimer = 0f;

            if (rb != null)
            {
                rb.linearVelocity = _velocity;
                rb.angularVelocity = Vector3.zero;
            }

        }
        public void DecrementHealth()
        {
            if (Life > 0)
            {
                Life--;
            }
            else
            {
                Die();
            }
        }
    }
}

