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
            internal float Lifetime, startLifetime;
            internal Vector3 startPosition, velocity;
            internal float gravityModifier;
            Vector3 pos;
            Transform bulletObject;
            float minDamage, maxDamage;
            float maxDistance;
            GameObject impactEffect;
            internal RaycastBullet(float lifetime, Vector3 startPosition, Vector3 velocity, float gravityModifier, Transform bulletObject, float minDamage, float maxDamage, GameObject impactEffect = null)
            {
                startLifetime = lifetime;
                Lifetime = lifetime;
                this.startPosition = startPosition;
                this.velocity = velocity;
                this.gravityModifier = gravityModifier;
                this.bulletObject = bulletObject;
                this.minDamage = minDamage;
                this.maxDamage = maxDamage;
                maxDistance = velocity.magnitude * Time.fixedDeltaTime;
                pos = startPosition;
                this.impactEffect = impactEffect;
            }
            internal void TerminateBullet()
            {
                Destroy(bulletObject.gameObject, 2f);

            }
            internal void BulletHit(RaycastHit hit)
            {
                TerminateBullet();
                if (impactEffect)
                {
                    var go = Instantiate(impactEffect, hit.point, Quaternion.identity);
                    go.transform.up = hit.normal;
                    Destroy(go, 10f);
                }
            }
            internal void UpdateBullet()
            {
                //Create a new ray
                Ray r = new()
                {
                    origin = pos,
                    direction = velocity
                };
                //perform the raycast
                if (Physics.Raycast(r,out RaycastHit hit, maxDistance, WeaponConstants.instance.bulletLayermask))
                {
                    Debug.DrawRay(r.origin, r.direction, Color.green, WeaponConstants.instance.bulletDebugRayTime);
                    Lifetime = 0;
                    pos = hit.point;
                    if (bulletObject)
                        bulletObject.position = pos;
                    BulletHit(hit);
                }
                else
                {
                    Debug.DrawRay(r.origin, r.direction, Color.red, WeaponConstants.instance.bulletDebugRayTime);
                    Lifetime -= Time.fixedDeltaTime;
                    pos += velocity * Time.fixedDeltaTime;
                    velocity += Time.fixedDeltaTime * gravityModifier * Physics.gravity;
                    if (bulletObject)
                        bulletObject.position = pos;
                }

            }
        }
        private void Awake()
        {
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
            wfs = new(postFireDelayTime);
            cm = GetComponentInParent<CharacterMotor>(true);

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
        [SerializeField] Vector2 baseSpread;
        [SerializeField] Vector2 hipFireSpread;
        [SerializeField] float spreadIncrement, spreadDecay, maxSpread, currentSpread;
        [SerializeField] bool mustFocusToFire;
        [SerializeField] internal UnityEvent fireEvent, reloadEvent;
        [SerializeField] GameObject bulletImpactEffect;
        [SerializeField] internal int fireIterations;
        [SerializeField] internal bool hasOptic;

        [SerializeField] internal AnimationCurve damageCurve;
        [SerializeField] internal float ricochetDotThreshold;
        [SerializeField] internal float maxRandomAngleForRicochet;
        [SerializeField, Range(0, 1)] internal float ricochetChance;
        [SerializeField, Range(0, 1)] internal float penetrateChance;
        [SerializeField] internal float penetrateDotThreshold;
        [SerializeField] internal float penetrateMaxThickness;
        [SerializeField] internal int maxPenetrateTries;
        [SerializeField] internal float damageMultiplierPerPenetration;
        public virtual void ManagedFixedUpdate()
        {
            canFire = !fireDelay && !firing;
            currentSpread = Mathf.Clamp(currentSpread -= Time.fixedDeltaTime * spreadDecay, 0, maxSpread);
            if (mustFocusToFire & CM_Focus < 1)
                return;
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
                Fire(cm.wm.fireTransform.position, maxPenetrateTries);
            }
        }
        IEnumerator BurstFire()
        {
            WaitForFixedUpdate wff = new();
            wfs = new(postFireDelayTime);
            firing = true;
            for (int i = 0; i < burstCount; i++)
            {
                Fire(cm.wm.fireTransform.position, maxPenetrateTries);
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
        /// <summary>
        /// Fires a hitscan bullet
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <param name="penetrateTriesLeft"></param>
        /// <param name="hasRicocheted"></param>
        /// <returns>Position hit by the bullet</returns>
        protected Vector3 Fire(Vector3 point, int penetrateTriesLeft, bool hasRicocheted = false)
        {

            cm.GetAnimator.Play("Fire", -1, 0);
            cm.ReceiveRecoilImpulse(linearRecoilMinMax, angularRecoilMinMax);
            Transform t = cm.wm.fireTransform;
            for (int i = 0; i < fireIterations; i++)
            {
                Vector3 direction = GetSpread(t).normalized;
                currentSpread += spreadIncrement / fireIterations;
                if (projectile)
                {
                    var go = Instantiate(projectile, fireFromPosition.position, Quaternion.identity);
                    go.transform.forward = cm.wm.fireTransform.forward;
                    Destroy(go, bulletLifetime);
                }
                else
                {
                    BulletTracer newTracer = Instantiate(bulletObject, fireFromPosition.position, Quaternion.identity).GetComponent<BulletTracer>();
                    newTracer.bulletSpeed = bulletSpeed;
                    int currentPenetrateTries = penetrateTriesLeft;
                    bool ricocheted = false;
                    if (Physics.Raycast(point, direction, out RaycastHit hit, bulletSpeed, WeaponConstants.instance.bulletLayermask))
                    {
                        Debug.DrawLine(point, hit.point, Color.green, 0.3f);
                        if (hit.collider.attachedRigidbody)
                        {

                        }
                        float dot = Vector3.Dot(hit.normal, direction) * -1;
                        if (!hasRicocheted)
                        {
                            if (ricochetDotThreshold != 0 && dot < ricochetDotThreshold)
                            {
                                if (Random.value < ricochetChance)
                                {
                                    Fire(hit.point, Vector3.Reflect(direction, hit.normal), penetrateTriesLeft, newTracer, true);
                                    ricocheted = true;
                                }
                            }
                        }
                        if (currentPenetrateTries > 0 && dot > penetrateDotThreshold && !(ricocheted || hasRicocheted))
                        {
                            Vector3 penetratePoint = CanPenetrate(hit.point, direction);
                            if (penetratePoint != Vector3.zero)
                            {
                                if (Random.value < penetrateChance)
                                {
                                    currentPenetrateTries--;
                                    Fire(penetratePoint, direction, currentPenetrateTries, newTracer, true);
                                }
                            }
                        }
                    }
                    else
                        Debug.DrawRay(point, direction * bulletSpeed, Color.red, 0.3f);
                    Vector3 endPos = hit.collider ? hit.point : point + (direction * bulletSpeed);
                    newTracer.AddPosition(endPos);
                }
            }
                return Vector3.zero;
        }
        /// <summary>
        /// Fire method for ricocheting/penetrating bullets
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <param name="penetrateTriesLeft"></param>
        /// <param name="hasRicocheted"></param>
        /// <returns></returns>
        protected Vector3 Fire(Vector3 point, Vector3 direction, int penetrateTriesLeft, BulletTracer tracer, bool hasRicocheted = false)
        {
            int currentPenetrateTries = penetrateTriesLeft;
            bool ricocheted = false;
            if (Physics.Raycast(point, direction, out RaycastHit hit, bulletSpeed, WeaponConstants.instance.bulletLayermask))
            {
                Debug.DrawLine(point, hit.point, Color.green, 0.3f);
                if (hit.collider.attachedRigidbody)
                {

                }
                float dot = Vector3.Dot(hit.normal, direction) * -1;
                if (!hasRicocheted)
                {
                    if (ricochetDotThreshold != 0 && dot < ricochetDotThreshold)
                    {
                        if (Random.value < ricochetChance)
                        {
                            Fire(hit.point, Vector3.Reflect(direction, hit.normal), penetrateTriesLeft, tracer, true);
                            ricocheted = true;
                        }
                    }
                }
                if (currentPenetrateTries > 0 && dot > penetrateDotThreshold && !(ricocheted || hasRicocheted))
                {
                    Vector3 penetratePoint = CanPenetrate(hit.point, direction);
                    if (penetratePoint != Vector3.zero)
                    {
                        if (Random.value < penetrateChance)
                        {
                            currentPenetrateTries--;
                            Fire(penetratePoint, direction, currentPenetrateTries, tracer, true);
                        }
                    }
                }
            }
            else
                Debug.DrawRay(point, direction * bulletSpeed, Color.red, 0.3f);
            Vector3 endPos = hit.collider ? hit.point : point + (direction * bulletSpeed);
            tracer.AddPosition(endPos);
            return endPos;
        }
        Vector3 CanPenetrate(Vector3 impact, Vector3 direction)
        {
            Vector3 rayfirepos = impact + (direction * penetrateMaxThickness);
            if (Physics.Raycast(rayfirepos, -direction, out RaycastHit hit, penetrateMaxThickness, WeaponConstants.instance.bulletLayermask))
            {

                Debug.DrawRay(rayfirepos, direction, Color.green, 0.5f);
                return hit.point;

            }
            else
                Debug.DrawRay(rayfirepos, direction, Color.red, 0.5f);
            return Vector3.zero;
        }

        Vector3 SpreadVector(Vector3 vec)
        {
            Vector3 vecOut = Vector3.zero;
            vecOut.x = Random.Range(-vec.x, vec.x);
            vecOut.y = Random.Range(-vec.y, vec.y);
            return vecOut + Vector3.forward;
        }
        Vector3 GetSpread(Transform t)
        {
            Vector3 spreadDirection = t.TransformDirection(SpreadVector(baseSpread) + Vector3.forward * bulletSpeed);
            Vector3 addSpread = t.TransformDirection(SpreadVector(hipFireSpread * currentSpread) + Vector3.forward * bulletSpeed) * (1 - CM_Focus);
            return spreadDirection + addSpread;
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