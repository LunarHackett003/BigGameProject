using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight.Weapons
{
    public class WeaponAnimationHelper : MonoBehaviour
    {
        [SerializeField] WeaponManager wm;
        public void ResetWeaponFire()
        {
            wm.CurrentWeapon.PerformManualAction();
        }
        public void SwitchWeapons()
        {
            wm.SwitchWeapon();
        }
    }
}