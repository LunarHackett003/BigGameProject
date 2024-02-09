using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Starlight.Weapons
{
    public class BaseWeapon : MonoBehaviour, IManagedBehaviour
    {
        private void Awake()
        {
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
            wfs = new(postFireDelayTime);
            cm = GetComponentInParent<CharacterMotor>();

        }

        bool fireInput;
        public void SetFireInput(bool fireInput)
        {
            this.fireInput = fireInput;
        }
        bool canFire;
        bool fireDelay;
        [SerializeField] float postFireDelayTime;
        [SerializeField] float preFireDelayTime;
        bool fireBlocked;
        bool firing;
        WaitForSeconds wfs;
        public AudioSource gunshotSource;
        public AudioClip gunShotClip;
        public bool useVFXGraph;
        public ParticleSystem muzzleFlash;
        public VisualEffect vfxMuzzleFlash;

        CharacterMotor cm;
        public float CM_Focus { get { return cm.Focus; } }
        [SerializeField] MinMaxVector3 linearRecoilMinMax, angularRecoilMinMax;
        [SerializeField] Vector3 linearRecoilMin, linearRecoilMax, angularRecoilMin, angularRecoilMax;
        public virtual void ManagedFixedUpdate()
        {
            canFire = !fireDelay;
            if (fireInput && !fireBlocked)
            {
                if (canFire)
                {
                    if(preFireDelayTime > 0 && !firing)
                    {
                        StartCoroutine(PreFireDelay());
                    }
                    else
                    {
                        Fire();
                    }
                }
            }
        }
        public virtual void ManagedLateUpdate()
        {

        }
        public virtual void ManagedUpdate()
        {

        }
        public void Fire()
        {
            if (useVFXGraph && vfxMuzzleFlash)
            {
                vfxMuzzleFlash.Play();
            }
            else if (muzzleFlash)
            {
                muzzleFlash.Play(true);
            }



            if (cm)
            {
                cm.ReceiveRecoilImpulse(linearRecoilMinMax, angularRecoilMinMax);
            }
            StartCoroutine(PostFireDelay());
        }
        public IEnumerator PostFireDelay()
        {
            wfs = new(postFireDelayTime);
            fireDelay = true;
            yield return wfs;
            fireDelay = false;
            yield break;
        }
        public IEnumerator PreFireDelay()
        {
            firing = true;
            WaitForFixedUpdate wff = new();
            WaitForSeconds wfs = new WaitForSeconds(preFireDelayTime);
            yield return wfs;
            //We should fire here, regardless of if the player has released the trigger or not 
            Fire();
            yield return this.wfs;
            while (fireInput && !fireBlocked)
            {
                yield return wff;
                if (canFire)
                    Fire();
                //If canFire is true, we'll fire again, and keep doing that till we either cant shoot anymore or 
            }
            yield return this.wfs;
            firing = false;
        }
        private void OnValidate()
        {
            linearRecoilMinMax = new()
            {
                min = linearRecoilMin,
                max = linearRecoilMax
            };
            angularRecoilMinMax = new()
            {
                min = angularRecoilMin,
                max = angularRecoilMax
            };
        }
    }
}