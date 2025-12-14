using UnityEngine;

namespace NoScope
{
    public class SmallEnemy : EnemyBase
    {
        [Header("Small Enemy Settings")]
        [SerializeField] private float lifetime = 10f;

        protected override void Start()
        {
            base.Start();

            // Auto-destruction après un certain temps
            Destroy(gameObject, lifetime);
        }

        protected override void FollowPlayer()
        {
            base.FollowPlayer();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Inflige des dégâts au joueur
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    // Le joueur meurt au contact
                    Die();
                }
            }
        }

        protected override void Die()
        {
            base.Die();
        }
    }
}
