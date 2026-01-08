using UnityEngine;

namespace NoScope
{
    public class WeaponChange : BoostBase
    {
        protected override void ApplyBoost(Player player, float boostDuration)
        {
            if (player == null) return;

            Weapon[] allWeapons = player.GetComponentsInChildren<Weapon>(true);
            if (allWeapons.Length < 2) return;

            Weapon currentWeapon = player.GetCurrentWeapon();

            Weapon randomWeapon;
            do
            {
                randomWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
            } while (randomWeapon == currentWeapon && allWeapons.Length > 1);

            player.EquipWeapon(randomWeapon);
        }

        protected override void RevertBoost(Player player)
        {
            if (player == null) return;

            player.SwitchToWeapon<BasicWeapon>();
        }

        protected override System.Action<Player> GetRevertAction()
        {
            // Retourne une action indépendante de l'instance pour rebasculer vers l'arme de base
            return (Player p) => { if (p != null) p.SwitchToWeapon<BasicWeapon>(); };
        }
    }
}