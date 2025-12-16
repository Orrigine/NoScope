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
                    player.DecrementHealth();
                }
            }
        }

        protected override void Die()
        {
            base.Die();
        }
    }
}
