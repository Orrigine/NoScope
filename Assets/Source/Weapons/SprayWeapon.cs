using UnityEngine;

namespace NoScope
{
    /// <summary>
    /// Arme spray - tire en éventail vers l'arrière avec un angle de dispersion
    /// </summary>
    public class SprayWeapon : Weapon
    {
        [Header("Spray Settings")]
        [SerializeField] private int bulletsPerShot = 5; // Nombre de projectiles par tir
        [SerializeField] private float maxSpreadAngle = 70f; // Angle maximum de dispersion (en degrés)

        protected override void Start()
        {
            base.Start();

            // Cadence de tir plus élevée pour l'arme spray
            baseBulletsPerSecond = 3f;
            maxBulletsPerSecond = 15f;
            _currentBulletsPerSecond = baseBulletsPerSecond;
        }

        public override void Fire()
        {
            if (!CanFire() || bulletPrefab == null || gunPoints == null || gunPoints.Length == 0)
                return;

            // Ne tire pas pendant la QTE
            if (QTEManager.Instance != null && QTEManager.Instance.IsQTEActive())
                return;

            FireSpray();
            UpdateNextFireTime();
        }

        private void FireSpray()
        {
            foreach (Transform gunPoint in gunPoints)
            {
                if (gunPoint != null)
                {
                    // Tire plusieurs projectiles en éventail
                    for (int i = 0; i < bulletsPerShot; i++)
                    {
                        // Calcule l'angle de dispersion pour ce projectile
                        // Répartit uniformément les projectiles entre -maxSpreadAngle et +maxSpreadAngle
                        float angleOffset = Mathf.Lerp(-maxSpreadAngle, maxSpreadAngle, i / (float)(bulletsPerShot - 1));

                        // Si un seul projectile, tire droit
                        if (bulletsPerShot == 1)
                            angleOffset = 0f;

                        // Calcule la direction de tir avec rotation
                        // Rotation autour de l'axe Y (gauche-droite)
                        Quaternion rotation = gunPoint.rotation * Quaternion.Euler(0f, angleOffset, 0f);

                        // Instantie le projectile
                        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, rotation);

                        // Applique la vélocité
                        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                        if (bulletRb != null)
                        {
                            // Direction basée sur la rotation calculée (vers l'arrière avec l'offset)
                            Vector3 direction = rotation * Vector3.back;
                            bulletRb.linearVelocity = direction * bulletSpeed;
                        }

                        Destroy(bullet, bulletLifetime);
                    }
                }
            }
        }
    }
}
