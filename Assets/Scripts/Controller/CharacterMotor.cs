using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight
{
    public class CharacterMotor : MonoBehaviour, IManagedBehaviour
    {
        [SerializeField] Transform aimRotationTransform;



        private void Awake()
        {
            BehaviourManager.fixedUpdateClients += ManagedFixedUpdate;
            BehaviourManager.updateClients += ManagedUpdate;
            BehaviourManager.lateUpdateClients += ManagedLateUpdate;
        }

        public void ManagedFixedUpdate()
        {

        }

        public void ManagedLateUpdate()
        {

        }

        public void ManagedUpdate()
        {

        }
    }
}