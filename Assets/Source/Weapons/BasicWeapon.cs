using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Arme de base - tir simple vers l'arrière
    /// </summary>
    public class BasicWeapon : Weapon
    {
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
                    // Tire vers l'arrière
                    GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation);

                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        bulletRb.linearVelocity = -gunPoint.forward * bulletSpeed;
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
    }
}
