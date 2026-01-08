using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Classe de base abstraite pour toutes les armes
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("Weapon Stats")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected float baseBulletsPerSecond = 2f; // Cadence de tir de base
        [SerializeField] protected float maxBulletsPerSecond = 10f; // Cadence maximale
        [SerializeField] protected float minBulletsPerSecond = 1f; // Cadence minimale
        [SerializeField] protected float fireRateIncreasePerQTE = 0.5f; // Augmentation par QTE
        [SerializeField] protected float fireRateDecayRate = 0.1f; // Réduction par seconde
        
        [Header("Projectile Settings")]
        [SerializeField] protected GameObject bulletPrefab;
        [SerializeField] protected Transform[] gunPoints;
        [SerializeField] protected float bulletSpeed = 20f;
        [SerializeField] protected float bulletLifetime = 5f;

        // État actuel de l'arme
        protected float _currentBulletsPerSecond;
        protected float _nextFireTime;
        protected bool _isRecoveringFireRate = false;

        protected virtual void Start()
        {
            _currentBulletsPerSecond = baseBulletsPerSecond;
        }

        /// <summary>
        /// Méthode abstraite pour tirer - implémentée par chaque arme
        /// </summary>
        public abstract void Fire();

        /// <summary>
        /// Vérifie si l'arme peut tirer (cooldown)
        /// </summary>
        public bool CanFire()
        {
            return Time.time >= _nextFireTime;
        }

        /// <summary>
        /// Met à jour le prochain temps de tir
        /// </summary>
        protected void UpdateNextFireTime()
        {
            float fireInterval = 1f / Mathf.Max(_currentBulletsPerSecond, 0.1f);
            _nextFireTime = Time.time + fireInterval;
        }

        /// <summary>
        /// Augmente la cadence de tir (appelé lors d'une QTE réussie)
        /// </summary>
        public virtual void IncreaseFireRate(float bonus = 0f)
        {
            float increase = fireRateIncreasePerQTE + bonus;
            _currentBulletsPerSecond = Mathf.Min(_currentBulletsPerSecond + increase, maxBulletsPerSecond);
            _isRecoveringFireRate = false;
        }

        /// <summary>
        /// Diminue la cadence de tir progressivement
        /// </summary>
        public virtual void DecayFireRate()
        {
            if (_currentBulletsPerSecond > baseBulletsPerSecond)
            {
                _currentBulletsPerSecond = Mathf.Max(
                    _currentBulletsPerSecond - fireRateDecayRate * Time.deltaTime,
                    baseBulletsPerSecond
                );
            }
        }

        /// <summary>
        /// Divise la cadence de tir (appelé lors d'une QTE ratée)
        /// </summary>
        public virtual void DivideFireRate()
        {
            _currentBulletsPerSecond = Mathf.Max(_currentBulletsPerSecond / 2f, minBulletsPerSecond);
            _isRecoveringFireRate = true;
        }

        /// <summary>
        /// Récupère progressivement la cadence après une division
        /// </summary>
        public virtual void RecoverFireRate()
        {
            if (_isRecoveringFireRate && _currentBulletsPerSecond < baseBulletsPerSecond)
            {
                _currentBulletsPerSecond = Mathf.Min(
                    _currentBulletsPerSecond + fireRateDecayRate * Time.deltaTime,
                    baseBulletsPerSecond
                );

                if (_currentBulletsPerSecond >= baseBulletsPerSecond)
                {
                    _isRecoveringFireRate = false;
                }
            }
        }

        // Getters
        public float GetCurrentFireRate() => _currentBulletsPerSecond;
        public string GetWeaponName() => weaponName;
    }
}
