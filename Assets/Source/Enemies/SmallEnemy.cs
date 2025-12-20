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
                Debug.Log("[SmallEnemy] Appel de AddScoreForEnemyKill");
                GameManager.Instance.AddScoreForEnemyKill();
            }
            else
            {
                Debug.LogError("[SmallEnemy] GameManager.Instance est null, impossible d'ajouter du score !");
            }

            base.Die();
        }
    }
}
