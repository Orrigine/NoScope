using UnityEngine;

namespace NoScope
{
    public class SmallEnemy : EnemyBase
    {

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
            // Ajoute des points au score avant de mourir
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScoreForEnemyKill();
            }
            
            base.Die();
        }
    }
}
