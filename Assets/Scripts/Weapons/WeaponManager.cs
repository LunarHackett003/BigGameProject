using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight.Weapons
{
    public class WeaponManager : MonoBehaviour, IManagedBehaviour
    {

        [SerializeField] List<BaseWeapon> weapons;
        [SerializeField] int weaponIndex;
        bool fireInput;
        private void Awake()
        {
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
        }
        public void SetFireInput(bool fireInput)
        {
            this.fireInput = fireInput;
        }

        public void ManagedFixedUpdate()
        {
        }

        public void ManagedLateUpdate()
        {
            if (weapons[weaponIndex])
            {
                weapons[weaponIndex].SetFireInput(fireInput);
            }
        }

        public void ManagedUpdate()
        {

        }
    }
}