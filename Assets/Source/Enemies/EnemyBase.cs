using System;
using UnityEngine;

namespace NoScope
{
    public class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float damageToPlayer = 10f;

        [Header("Visual Feedback")]
        [SerializeField] protected GameObject damageEffectPrefab;
        [SerializeField] protected Transform healthBarTransform;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        // Méthode protégée pour que les enfants puissent invoquer l'événement
        protected void InvokeHealthChanged(float current, float max)
        {
            OnHealthChanged?.Invoke(current, max);
        }

        protected void InvokeDeath()
        {
            OnDeath?.Invoke();
        }

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            FollowPlayer();
        }

        protected virtual void FollowPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Calcule la direction mais ignore l'axe Y pour rester au sol
                Vector3 targetPosition = player.transform.position;
                Vector3 currentPosition = transform.position;

                // Garde la même hauteur Y
                targetPosition.y = currentPosition.y;

                Vector3 direction = (targetPosition - currentPosition).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                // LookAt aussi sans suivre l'axe Y
                Vector3 lookAtTarget = player.transform.position;
                lookAtTarget.y = currentPosition.y;
                transform.LookAt(lookAtTarget);
            }
        }

        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            InvokeHealthChanged(currentHealth, maxHealth);

            if (damageEffectPrefab != null)
            {
                Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        // Méthodes pour accéder aux stats
        public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public float GetMaxHealth() => maxHealth;
        public float GetCurrentHealth() => currentHealth;
        public float GetMoveSpeed() => moveSpeed;

        protected virtual void Die()
        {
            InvokeDeath();
            // Ne pas détruire immédiatement pour permettre aux effets de se jouer
            Destroy(gameObject, 0.5f);
        }
    }
}
