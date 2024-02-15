using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Starlight.Weapons.Animation;
namespace Starlight.Weapons
{
    [SaveDuringPlay]
    public class BaseWeapon : MonoBehaviour, IManagedBehaviour
    {
        [System.Serializable]
        public class RaycastBullet
        {
            public RaycastBullet(Vector3 startPos, float speed, Vector3 direction, float lifetime, float gravity, Transform bulletObject, float maxDamage, float minDamage, Vector3 objectStartPosition)
            {
                Debug.DrawRay(startPos, direction * speed);
                lastPos = startPos;
                this.speed = speed;
                this.direction = direction.normalized;
                lifetimeRemaining = lifetime;
                this.gravity = gravity;
                this.bulletObject = bulletObject;
                this.minDamage = minDamage;
                this.maxDamage = maxDamage;
                this.bulletObject.position = objectStartPosition;
                velocity = direction * speed * Time.fixedDeltaTime;
                nextPos = lastPos + velocity;
            }

            Vector3 lastPos;
            Vector3 nextPos;
            internal float speed;
            Vector3 direction;
            Vector3 velocity;
            float gravity;
            float lifetimeRemaining;
            Transform bulletObject;
            GameObject impactEffect;
            float maxDamage, minDamage;
            public float Lifetime { get { return lifetimeRemaining; } }
            public void UpdateBullet()
            {
                float deltaTime = Time.fixedDeltaTime;
                Debug.Log("updating a bullet!");
                velocity += Time.fixedDeltaTime * gravity * Physics.gravity;
                nextPos += velocity;
                Ray r = new()
                {
                    origin = lastPos,
                    direction = velocity.normalized
                };
                if (Physics.Raycast(r, out RaycastHit hit, speed * deltaTime, WeaponConstants.instance.bulletLayermask))
                {
                    Debug.DrawRay(r.origin, velocity, Color.green, WeaponConstants.instance.bulletDebugRayTime, false);
                    lifetimeRemaining = 0;
                    if (impactEffect)
                    {
                        GameObject go = Object.Instantiate(impactEffect, hit.point, Quaternion.identity);
                        go.transform.up = hit.normal;
                        Object.Destroy(go, 10);
                    }
                    nextPos = hit.point;
                    TerminateBullet();
                }
                else
                {
                    Debug.DrawRay(r.origin, velocity, Color.red, WeaponConstants.instance.bulletDebugRayTime, false);
                    lifetimeRemaining -= deltaTime;
                }
                lastPos = nextPos;
            }
            internal void TerminateBullet()
            {
                if (bulletObject)
                {
                    bulletObject.position = nextPos;
                    Destroy(bulletObject.gameObject, 1f);
                }
            }
            internal IEnumerator BulletLerp()
            {
                if (bulletObject)
                {
                    var w = new WaitForEndOfFrame();
                    while (lifetimeRemaining > 0)
                    {
                        bulletObject.position = Vector3.Lerp(lastPos, nextPos, Time.smoothDeltaTime * speed);
                        yield return w;
                    }
                }
                else
                {

                }
            }
            internal void SetBulletPosition()
            {
                bulletObject.position = nextPos;
            }
        }
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

        [SerializeField] internal Vector3 linearBobSpeed, angularBobSpeed;
        [SerializeField] internal Vector3 linearBobMultiplier, angularBobMultiplier;
        [SerializeField] internal float recoilLerpSpeed;
        [SerializeField] internal float recoilDecay;
        [SerializeField] internal float idleBobLerpSpeedLinear, idleBobLerpSpeedAngular, movingBobLerpSpeedLinear, movingBobLerpSpeedAngular, idleBobLerpScaleLinear, movingBobLerpScaleLinear, idleBobLerpScaleAngular, movingBobLerpScaleAngular;
        [SerializeField] internal float bobFocusMultiplier;
        internal CharacterMotor cm;
        internal CharacterMotor CM {  get { return cm; } }
        internal float CM_Focus { get { return cm.Focus; } }
        [SerializeField] float adsSpeed;
        internal float FocusSpeed { get { return adsSpeed; } }
        [SerializeField] internal Transform focusTransform;
        [SerializeField] MinMaxVector3 linearRecoilMinMax, angularRecoilMinMax;
        [SerializeField] Vector3 linearRecoilMin, linearRecoilMax, angularRecoilMin, angularRecoilMax;

        [SerializeField, Header("Bullet/Projectile"), Tooltip("If there is a projectile specified, the gun will fire a projectile. Otherwise, a bullet will be fired.")] public GameObject projectile;
        [SerializeField] public GameObject bulletObject;
        [SerializeField, Tooltip("This is where a bullet will spawn its object at.")] Transform fireFromPosition;
        [SerializeField] float bulletSpeed, bulletLifetime, bulletGravity, bulletMaxDamage,bulletMinDamage;

        [SerializeField] internal UnityEvent fireEvent, reloadEvent;
        public virtual void ManagedFixedUpdate()
        {
            canFire = !fireDelay && !firing;
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
            firing = true;
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
            timesFired++;
            cm.GetAnimator.Play("Fire", 0, 0);
            if (useVFXGraph && vfxMuzzleFlash)
            {
                vfxMuzzleFlash.Play();
            }
            else if (muzzleFlash)
            {
                muzzleFlash.Play(true);
            }
            //We're firing a projectile, so we don't want to shoot any bullets here
            if (projectile)
            {
                GameObject go = Instantiate(projectile, cm.wm.fireTransform.position + cm.wm.fireTransform.forward * 0.4f, Quaternion.identity);
                go.transform.forward = cm.wm.fireTransform.forward;
                if(projectile.TryGetComponent<Projectile>(out var proj))
                {

                }
                Destroy(go, bulletLifetime);
            }
            else
            {
                RaycastBullet b = new(cm.wm.fireTransform.position + (cm.wm.fireTransform.forward * 0.4f), bulletSpeed, cm.wm.fireTransform.forward,
                    bulletLifetime, bulletGravity, Instantiate(bulletObject).transform, bulletMaxDamage, bulletMinDamage, fireFromPosition.position);
                WeaponConstants.instance.AddBullet(b);
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
            if (cm)
            {
                cm.wm.SetVariables();
            }
        }
        
    }

}