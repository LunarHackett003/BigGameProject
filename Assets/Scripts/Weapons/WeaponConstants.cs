using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight.Weapons
{
    public class WeaponConstants
        : MonoBehaviour, IManagedBehaviour
    {
        public static WeaponConstants instance;
        public LayerMask bulletLayermask;
        public float bulletDebugRayTime;
        public float cleanupImpactTime;

        public void ManagedFixedUpdate()
        {

        }

        public void ManagedLateUpdate()
        {

        }

        public void ManagedUpdate()
        {
        }

        private void Awake()
        {
            instance = this;
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
        }

    }
}