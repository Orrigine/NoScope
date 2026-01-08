using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Arme quadruple - tire 4 lignes parallèles vers l'arrière
    /// 2 lignes de chaque côté
    /// </summary>
    public class TripleWeapon : Weapon
    {
        [Header("Quad Weapon Settings")]
        [SerializeField] private float innerOffset = 0.25f; // Distance des lignes intérieures
        [SerializeField] private float outerOffset = 0.75f; // Distance des lignes extérieures

        public override void Fire()
        {
            if (!CanFire() || bulletPrefab == null || gunPoints == null || gunPoints.Length == 0)
                return;

            // Ne tire pas pendant la QTE
            if (QTEManager.Instance != null && QTEManager.Instance.IsQTEActive())
                return;

            FireBullets();
            UpdateNextFireTime();
        }

        private void FireBullets()
        {
            foreach (Transform gunPoint in gunPoints)
            {
                if (gunPoint != null)
                {
                    // Ligne intérieure gauche
                    Vector3 innerLeft = gunPoint.position + gunPoint.right * -innerOffset;
                    SpawnBullet(innerLeft, gunPoint.rotation);

                    // Ligne intérieure droite
                    Vector3 innerRight = gunPoint.position + gunPoint.right * innerOffset;
                    SpawnBullet(innerRight, gunPoint.rotation);

                    // Ligne extérieure gauche
                    Vector3 outerLeft = gunPoint.position + gunPoint.right * -outerOffset;
                    SpawnBullet(outerLeft, gunPoint.rotation);

                    // Ligne extérieure droite
                    Vector3 outerRight = gunPoint.position + gunPoint.right * outerOffset;
                    SpawnBullet(outerRight, gunPoint.rotation);
                }
            }
        }

        private void SpawnBullet(Vector3 position, Quaternion rotation)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, rotation);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = -bullet.transform.forward * bulletSpeed;
            }

            // Set projectile damage if supported
            var bulletComp = bullet.GetComponent<NoScope.Bullet>();
            if (bulletComp != null)
            {
                bulletComp.SetDamage(bulletDamage);
            }

            Destroy(bullet, bulletLifetime);
        }
    }
}
