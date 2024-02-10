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

        [SerializeField] AnimationContainer animContainer;
        internal AnimationContainer AnimContainer { get { return animContainer; } }
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
        int timesFired;
        [SerializeField, Tooltip("How many times the weapon will be fired when pressing the trigger. Used for semi-automatic weapons by setting this to 1.")] int burstCount;
        [SerializeField] float timeBetweenBursts;
        [SerializeField, Tooltip("How many times the weapon can be fired before having to manually cycle the weapon. Useful for guns such as the quad-tube shotgun or something")] int shotsBeforeManualAction;
        [SerializeField, Tooltip("Whether or not the weapon has to be manually reset before it can fire again. Useful for bolt- and pump-action weapons.")] bool manualAction;
        WaitForSeconds wfs;
        public AudioSource gunshotSource;
        public AudioClip gunShotClip;
        public bool useVFXGraph;
        public ParticleSystem muzzleFlash;
        public VisualEffect vfxMuzzleFlash;

        CharacterMotor cm;
        public CharacterMotor CM {  get { return cm; } }
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
                        CheckFire();
                    }
                }
            }
        }

        internal void PerformManualAction()
        {
            fireDelay = false;
            timesFired = 0;
        }
        public virtual void ManagedLateUpdate()
        {

        }
        public virtual void ManagedUpdate()
        {

        }
        void CheckFire()
        {
            if(burstCount > 0)
            {
                StartCoroutine(BurstFire());
            }
            else
            {
                Fire();
            }
        }
        IEnumerator BurstFire()
        {
            WaitForFixedUpdate wff = new();
            wfs = new(postFireDelayTime);
            for (int i = 0; i < burstCount; i++)
            {
                Fire();
                yield return wfs;
            }
            yield return new WaitForSeconds(timeBetweenBursts);
            while (fireInput)
            {
                yield return wff;
            }
            firing = false;
            yield break;
        }

        void Fire()
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
            if (manualAction && timesFired >= shotsBeforeManualAction)
            {
                Debug.Log("Waiting for manual action!");
                fireDelay = true;
            }
            else
            {
                StartCoroutine(PostFireDelay());
            }
        }
        IEnumerator PostFireDelay()
        {
            wfs = new(postFireDelayTime);
            fireDelay = true;
            yield return wfs;
            fireDelay = false;
            yield break;
        }
        IEnumerator PreFireDelay()
        {
            firing = true;
            WaitForFixedUpdate wff = new();
            WaitForSeconds wfs = new(preFireDelayTime);
            yield return wfs;
            //We should fire here, regardless of if the player has released the trigger or not 
                CheckFire();
            if(burstCount > 0)
            {
                firing = false;
                yield return null;
            }
            yield return this.wfs;
            while (fireInput && !fireBlocked)
            {
                yield return wff;
                if (canFire)
                    CheckFire();
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