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
        List<BaseWeapon.RaycastBullet> bullets = new();
        BaseWeapon.RaycastBullet b;
        public void AddBullet(BaseWeapon.RaycastBullet bullet)
        {
            Debug.Log("added bullet!");
            bullets.Add(bullet);
            if(bullet.speed > 50)
            {

            }
            else
            {
                StartCoroutine(bullet.BulletLerp());
            }
        }

        public void ManagedFixedUpdate()
        {
            for (int i = 0; i < bullets.Count;)
            {
                
                b = bullets[i];
                b.UpdateBullet();
                if (b.speed > 50)
                    b.SetBulletPosition();
                if (b.Lifetime <= 0)
                {
                    b.TerminateBullet();
                    bullets.Remove(b);
                }
                else
                    i++;
            }
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