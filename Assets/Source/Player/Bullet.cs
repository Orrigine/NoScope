using UnityEngine;

namespace NoScope
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private GameObject hitEffectPrefab;

        public void SetDamage(float d)
        {
            damage = d;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                // Inflige des dégâts à l'ennemi
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                // Effet de hit
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}
