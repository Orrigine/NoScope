using UnityEngine;

namespace NoScope
{
    public class WeaponChange : BoostBase
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {

        }

        protected override void ApplyBoost(Player player)
        {
            if (player == null) return;

            Weapon[] allWeapons = player.GetComponentsInChildren<Weapon>();
            if (allWeapons.Length < 2) return;

            Weapon randomWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
            player.EquipWeapon(randomWeapon);
        }

        // Update is called once per frame
        protected override void Update()
        {

        }
    }
}