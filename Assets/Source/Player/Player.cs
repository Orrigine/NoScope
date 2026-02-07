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
        [Header("Damage")]
        [Tooltip("Seconds of invulnerability after taking damage to avoid multiple rapid hits")]
        [SerializeField] private float damageCooldown = 1.0f;

        // Timestamp of last received damage
        private float _lastDamageTime = -10f;

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

        [Header("Weapon")]
        [SerializeField] private Weapon currentWeapon; // L'arme actuelle du joueur

        [Header("References")]
        [SerializeField] private Rigidbody rb;

        // Events
        public event Action OnPlayerDie;
        // Notifie le nombre de vies actuel (0..3)
        public event Action<int> OnLifeChanged;

        // Private variables
        private float _currentSpeed;
        private bool _isJumping = false;

        private Tween _activeTween; // Tween DOTween actif
        private Vector3 _velocity;
        private int _consecutiveQTESuccesses = 0;
        private float _lastQTEStartTime = -10f; // Cooldown pour éviter les doubles appels
        private bool _isRecoveringFromFailure = false; // Indique si le joueur récupère d'un échec

        private void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            // Force la gravité et vérifie les contraintes
            rb.useGravity = true;


            _currentSpeed = baseSpeed;

            // Envoie l'état initial des vies au démarrage
            OnLifeChanged?.Invoke(Life);

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

            // Changement d'arme avec I et J (uniquement en mode debug)
            if (Keyboard.current != null)
            {
                // Vérifier si le DebugHUD existe et si le debug est actif
                DebugHUD debugHUD = FindFirstObjectByType<DebugHUD>();
                if (debugHUD != null && debugHUD.IsDebugActive)
                {
                    if (Keyboard.current.iKey.wasPressedThisFrame)
                    {
                        SwitchToWeapon<BasicWeapon>();
                    }
                    if (Keyboard.current.jKey.wasPressedThisFrame)
                    {
                        SwitchToWeapon<Shotgun>();
                    }
                }
            }

            // Ne tire pas et ne decay pas pendant la QTE
            bool isQTEActive = QTEManager.Instance != null && QTEManager.Instance.IsQTEActive();
            if (!isQTEActive)
            {
                // Tire avec l'arme actuelle
                if (currentWeapon != null)
                {
                    currentWeapon.Fire();
                }
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

            // Tue le tween précédent s'il existe
            _activeTween?.Kill();

            // Crée le tween avec un path parabolique (CatmullRom pour une courbe lisse)
            _activeTween = transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(Ease.Linear) // Vitesse constante pour un mouvement fluide et prévisible
                .SetUpdate(UpdateType.Normal, true) // useUnscaledTime = true pour ignorer timeScale
                .OnComplete(() =>
                {
                    _isJumping = false;
                    _activeTween = null;

                    // Réinitialise immédiatement la vélocité pour éviter le lag post-saut
                    _velocity = transform.forward * _currentSpeed;
                    _velocity.y = 0f;

                });

        }

        /// <summary>
        /// Change l'arme actuelle du joueur
        /// </summary>
        public void EquipWeapon(Weapon newWeapon)
        {
            currentWeapon = newWeapon;

        }

        /// <summary>
        /// Change vers une arme spécifique par son type
        /// </summary>
        public void SwitchToWeapon<T>() where T : Weapon
        {
            T weapon = GetComponentInChildren<T>(true); // true = inclut les objets désactivés
            if (weapon != null && weapon != currentWeapon)
            {
                EquipWeapon(weapon);
            }
            else
            {
                Debug.LogWarning($"[Player] Arme de type {typeof(T).Name} introuvable !");
            }
        }

        /// <summary>
        /// Récupère l'arme actuelle
        /// </summary>
        public Weapon GetCurrentWeapon() => currentWeapon;

        private void OnQTEComplete(bool success)
        {
            if (success)
            {
                _consecutiveQTESuccesses++;
                _isRecoveringFromFailure = false;

                // Calcule le bonus avec streak : bonus de base + (streak - 1) * multiplicateur
                float speedBonus = speedIncreasePerQTE + ((_consecutiveQTESuccesses - 1) * streakBonusMultiplier);

                // Augmente la vitesse avec bonus progressif
                _currentSpeed = Mathf.Min(_currentSpeed + speedBonus, maxSpeed);

                // Améliore le fire rate de l'arme avec bonus progressif
                if (currentWeapon != null)
                {
                    float fireRateBonus = (_consecutiveQTESuccesses - 1) * streakBonusMultiplier * 0.1f;
                    currentWeapon.IncreaseFireRate(fireRateBonus);
                }


            }
            else
            {
                _consecutiveQTESuccesses = 0;

                // Pénalité : perte de vitesse sur échec
                _currentSpeed = Mathf.Max(_currentSpeed - speedLossOnFailedQTE, minSpeed); // Ne peut pas descendre en dessous du minimum absolu
                _isRecoveringFromFailure = true;

                // Pénalité fire rate : divise par 2
                if (currentWeapon != null)
                {
                    currentWeapon.DivideFireRate();
                }


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

            // Gère le decay et la récupération du fire rate de l'arme
            if (currentWeapon != null)
            {
                currentWeapon.DecayFireRate();
                currentWeapon.RecoverFireRate();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore les bullets
            if (other.CompareTag("Bullet"))
                return;

            // Si c'est la masse ennemie, mort instantanée
            if (other.CompareTag("EnemyMass"))
            {
                Die();
                return;
            }

            if (other.CompareTag("Enemy"))
            {
                DecrementHealth();
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
            // S'assure que les vies sont à 0 et notifie
            Life = 0;
            OnLifeChanged?.Invoke(Life);

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

            if (rb != null)
            {
                rb.linearVelocity = _velocity;
                rb.angularVelocity = Vector3.zero;
            }

        }
        public void DecrementHealth()
        {
            // Respecte un cooldown pour éviter les hits répétés
            if (Time.time - _lastDamageTime < damageCooldown)
            {
                return;
            }

            _lastDamageTime = Time.time;

            Life = Mathf.Max(Life - 1, 0);

            // Notifie le changement de vies
            OnLifeChanged?.Invoke(Life);

            if (Life <= 0)
            {
                Die();
            }
        }


    }
}

