using Cinemachine;
using Starlight.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlight
{
    public class CharacterMotor : MonoBehaviour, IManagedBehaviour
    {


        [SerializeField] internal WeaponManager wm;

        [SerializeField] Transform aimRotationTransform;
        [SerializeField] Transform aimRotationAnchor;

        [SerializeField, Header("Aiming")] Vector2 lookInput;
        [SerializeField] Vector2 lookSpeed;
        [SerializeField] Vector2 lookAngle;
        [SerializeField] Vector2 aimRecoilInfluence;
        [SerializeField] Vector2 aimRecoilMultiplier;
        [SerializeField] float currentLeanAngle;
        [SerializeField] float leanBounds, leanSpeed;
        [SerializeField] float lookPitchClamp = 75;
        [SerializeField] float leanInput;
        float leanDampVelocity;
        [SerializeField, Space(10), Header("Movement")] Vector2 moveInput;
        [SerializeField] Vector2 dampedMovement;
        Vector2 dampVelocity;
        [SerializeField] float movementRampSpeed; 
        [SerializeField] Vector2 moveSpeedMultiplier;
        

        [SerializeField, Header("Look Sway")] Transform lookSwayTransform;
        [SerializeField] float lookSwayLerpSpeed;
        [SerializeField] Vector2 lookSwayScale;
        [SerializeField] float lookSwayRotationMultiplier, lookSwayTranslationMultiplier;
        [SerializeField] float lookSwayADSMultiplier;
        [SerializeField] Vector2 lookDelta;
        [SerializeField] Vector2 oldLook;
        [SerializeField] Vector2 currentLookSway;
        [SerializeField] Vector2 maxLookSway;

        [SerializeField, Header("View Recoil")] float globalRecoilMultiplier;
        [SerializeField] float recoilRotationMultiplier;
        [SerializeField] float recoilTranslationMultiplier;
        [SerializeField] internal float recoilLerpSpeed;
        [SerializeField] internal float recoilDecay;
        Vector3 currentLinearRecoil, currentAngularRecoil;
        Vector3 linearRecoilTarget, angularRecoilTarget;
        [SerializeField] Vector3 linearRecoilClamp, angularRecoilClamp;
        [SerializeField, Header("Animation")] Animator animator;
        [SerializeField] float movementAnimDampTime;

        [SerializeField, Header("Arms Bobbing")] Transform armBobbingTransform;
        [SerializeField] internal Vector3 linearBobSpeed, angularBobSpeed;
        Vector3 timeAngular, timeLinear;
        [SerializeField] internal Vector3 linearBobMultiplier, angularBobMultiplier;
        [SerializeField] Vector3 currentLinearBob, currentAngularBob;
        [SerializeField] float linearBobLerpSpeed, angularBobLerpSpeed;
        [SerializeField] float stationaryBobPower;
        [SerializeField] float bobTransformLeanMultiplier;
        [SerializeField] internal float bobFocusMultiplier;
        [SerializeField, Tooltip("An additive position based on the positive movement direction for x and y input. Inverted for negative inputs.")] Vector3 moveBasedPositionAdditive, moveBasedRotationAdditive;
        Vector3 movementAddPos, movementAddRot;
        [SerializeField, Tooltip("Should the bob update do the maths in fixed update and then lerp in late update?")] bool bobUseFixedUpdate;
        [SerializeField, Range(-5, 5)] float bobRecoilInfluence;
        [SerializeField, Range(-5, 5)] float bobSwayInfluence;
        [SerializeField] internal float idleBobLerpSpeedLinear, idleBobLerpSpeedAngular, movingBobLerpSpeedLinear, movingBobLerpSpeedAngular, idleBobLerpScaleLinear, movingBobLerpScaleLinear, idleBobLerpScaleAngular, movingBobLerpScaleAngular;
        float linearAngle, angularAngle;

        [SerializeField, Header("Focusing")] CinemachineVirtualCamera playerCam;
        bool focusInput;
        [SerializeField] float currentFocus, focusSpeed, unfocusedFOV, focusedFOV;
        internal void SetFocusSpeed(float speed)
        {
            focusSpeed = speed;
        }
        [SerializeField] internal Transform focusCameraTarget, focusCameraReceiver;
        [SerializeField] float focusInAnimationSpeedMultiplier = 1, focusOutAnimationSpeedMultiplier = 1;
        [SerializeField] float zoomLevel;
        [SerializeField] float zoomAimSlowingCoefficient;
        float zoomAimSlow;
        public void SetZoomLevel(float zoomLevel)
        {
            this.zoomLevel = zoomLevel;
            zoomAimSlow = zoomLevel * zoomAimSlowingCoefficient;
        }
        public void SetFocusTarget(Transform target)
        {
            focusCameraTarget = target;
        }
        public void SetFocusedFOV(float fov)
        {
            focusedFOV = fov;
        }
        public void SetUnfocusedFOV(float fov)
        {
            unfocusedFOV = fov;
        }

        //Public Get Methods
        public float MoveSpeed
        { get { return moveInputSize; } }
        public Animator GetAnimator
        { get { return animator; } }
        public float Focus { get { return currentFocus; } }
        float moveInputSize;
        [SerializeField] float cameraBobMultiplier;
        private void Awake()
        {
            BehaviourManager.fixedUpdateClients += ManagedFixedUpdate;
            BehaviourManager.updateClients += ManagedUpdate;
            BehaviourManager.lateUpdateClients += ManagedLateUpdate;
            animator.Update(0);
        }

        public void ManagedFixedUpdate()
        {
            Movement();
            if (bobUseFixedUpdate)
                ArmsBobbingMaths();
            currentFocus = Mathf.Clamp01(currentFocus + Time.fixedDeltaTime * focusSpeed * (focusInput ? 1 : -1));
            animator.SetFloat("Focus", currentFocus);
        }
        void ArmsBobbingMaths()
        {
            float moveInfluence = moveInputSize + stationaryBobPower;
            float movingBobScaleLerp = Mathf.Lerp(idleBobLerpScaleLinear, movingBobLerpScaleLinear, moveInfluence);
            float movingBobAngularScaleLerp = Mathf.Lerp(idleBobLerpScaleAngular, movingBobLerpScaleAngular, moveInfluence);
            linearAngle += Time.fixedDeltaTime * Mathf.Lerp(idleBobLerpSpeedLinear, movingBobLerpSpeedLinear, moveInfluence);
            angularAngle += Time.fixedDeltaTime * Mathf.Lerp(idleBobLerpSpeedAngular, movingBobLerpSpeedAngular, moveInfluence);
            timeLinear = new Vector2()
            {
                x = Mathf.Sin(linearAngle * linearBobSpeed.x) * movingBobScaleLerp,
                y = Mathf.Cos(linearAngle* linearBobSpeed.y) * movingBobScaleLerp
            };
            timeAngular = new Vector2()
            {
                x = Mathf.Sin(angularAngle * angularBobSpeed.x) *  movingBobAngularScaleLerp,
                y = Mathf.Cos(angularAngle * angularBobSpeed.y) * movingBobAngularScaleLerp
            };
            float delta = bobUseFixedUpdate ? Time.fixedDeltaTime : Time.smoothDeltaTime;
            float focuslerp = Mathf.Lerp(1, bobFocusMultiplier, currentFocus);
            movementAddPos = moveBasedPositionAdditive.ScaleReturn(new Vector3(dampedMovement.x, 0, dampedMovement.y));
            movementAddRot = moveBasedRotationAdditive.ScaleReturn(new Vector3(dampedMovement.y, dampedMovement.x, dampedMovement.x) );
            currentLinearBob = Vector3.Lerp(currentLinearBob, (timeLinear.ScaleReturn(linearBobMultiplier) + movementAddPos) * movingBobScaleLerp, delta * linearBobLerpSpeed) * focuslerp;
            currentAngularBob = Vector3.Lerp(currentAngularBob, (timeAngular.ScaleReturn(angularBobMultiplier) + movementAddRot) * movingBobAngularScaleLerp, delta * angularBobLerpSpeed ) * focuslerp;
        }

        void Movement()
        {
            dampedMovement = Vector2.SmoothDamp(dampedMovement, moveInput, ref dampVelocity, movementRampSpeed);
            transform.Translate(Time.fixedDeltaTime * new Vector3(dampedMovement.x * moveSpeedMultiplier.x, 0, dampedMovement.y * moveSpeedMultiplier.y), Space.Self);
            currentLeanAngle = Mathf.SmoothDamp(currentLeanAngle, leanInput * leanBounds, ref leanDampVelocity, leanSpeed);
            animator.SetFloat("Horizontal", dampedMovement.x);
            animator.SetFloat("Vertical", dampedMovement.y);
            moveInputSize = dampedMovement.sqrMagnitude;
        }

        public void ManagedLateUpdate()
        {
            UpdateCamera();
            AimRotation();
            AimSway();
            if(!bobUseFixedUpdate)
                ArmsBobbingMaths();

            ArmsBobbingLerp();
        }
        void UpdateCamera()
        {
            if (playerCam)
            {
                playerCam.m_Lens.FieldOfView = Mathf.Lerp(unfocusedFOV, focusedFOV, currentFocus);
            }
            focusCameraReceiver.position = Vector3.Lerp(focusCameraReceiver.parent.position, focusCameraTarget.position - focusCameraTarget.rotation * (currentLinearBob * cameraBobMultiplier), currentFocus);
        }

        void ArmsBobbingLerp()
        {
            armBobbingTransform.SetLocalPositionAndRotation(currentLinearBob + ((Vector3)currentLookSway * bobSwayInfluence) + (currentLinearRecoil * bobRecoilInfluence), 
                Quaternion.Euler(new Vector3(currentAngularBob.x, currentAngularBob.y, currentAngularBob.z + (currentLeanAngle * bobTransformLeanMultiplier)) + 
                (currentAngularRecoil * bobRecoilInfluence) + ((Vector3)currentLookSway * bobSwayInfluence)));
        }

        void AimRotation()
        {

            aimRotationTransform.position = aimRotationAnchor.position;
            lookAngle += (lookInput * Time.fixedDeltaTime * lookSpeed) / Mathf.Lerp(1, 1 + (zoomLevel * zoomAimSlowingCoefficient), currentFocus);
            lookAngle.y = Mathf.Clamp(lookAngle.y, -lookPitchClamp, lookPitchClamp);
            aimRecoilInfluence = currentAngularRecoil * aimRecoilMultiplier;
            lookDelta = oldLook - lookAngle;
            aimRotationTransform.localRotation = Quaternion.Euler(Mathf.Clamp(lookAngle.y + aimRecoilInfluence.x, -lookPitchClamp, lookPitchClamp), aimRecoilInfluence.y , 0);
            aimRotationTransform.localRotation *= Quaternion.Euler(0, 0, currentLeanAngle);
            transform.localRotation = Quaternion.Euler(0, lookAngle.x, 0);
            lookAngle.x %= 360;
            oldLook = lookAngle;
        }
        void AimSway()
        {
            float recoilDecaySpeed = Time.smoothDeltaTime * recoilDecay;
            //First we do the look sway maths
            Vector2 lookSwayMultiplied = ((lookDelta * lookSwayScale) * lookSwayRotationMultiplier).ClampThis(-maxLookSway, maxLookSway);
            //Then we do the view recoil maths. This will be used for anything from weapon recoil to being hit.
            linearRecoilTarget = Vector3.Lerp(linearRecoilTarget, Vector3.zero, recoilDecaySpeed);
            angularRecoilTarget = Vector3.Lerp(angularRecoilTarget, Vector3.zero, recoilDecaySpeed);
            Vector3 linearRecoilMult = (linearRecoilTarget * recoilTranslationMultiplier).ClampThis(-linearRecoilClamp, linearRecoilClamp);
            Vector3 angularRecoilMult = (angularRecoilTarget * recoilRotationMultiplier).ClampThis(-angularRecoilClamp, angularRecoilClamp);
            currentLinearRecoil = Vector3.Lerp(currentLinearRecoil, linearRecoilMult, Time.smoothDeltaTime * recoilLerpSpeed);
            currentAngularRecoil = Vector3.Lerp(currentAngularRecoil, angularRecoilMult, Time.smoothDeltaTime * recoilLerpSpeed);
            currentLookSway = Vector2.Lerp(currentLookSway, lookSwayMultiplied, Time.smoothDeltaTime * lookSwayLerpSpeed) * Mathf.Lerp(1, lookSwayADSMultiplier, currentFocus);
            lookSwayTransform.SetLocalPositionAndRotation((Vector3)(-currentLookSway * lookSwayTranslationMultiplier) + currentLinearRecoil, Quaternion.Euler((Vector3)currentLookSway + currentAngularRecoil));
        }

        public void ManagedUpdate()
        {

        }

        public void GetLeanInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                float val = context.ReadValue<float>();
                //If we're already leaning this way, we want to un-lean
                leanInput = leanInput == val ? 0 : val;
            }
        }
        public void GetMoveInput(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        public void GetLookInput(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        public void GetFireInput(InputAction.CallbackContext context)
        {
            wm.SetFireInput(context.ReadValue<float>() > 0.3f);
        }
        public void ReceiveRecoilImpulse(MinMaxVector3 linear, MinMaxVector3 angular)
        {
            linearRecoilTarget += linear;
            angularRecoilTarget += angular;
        }
        public void GetFocusInput(InputAction.CallbackContext context)
        {
            focusInput = context.ReadValue<float>() > 0.3f;
        }
        public void GetSwitchInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                animator.CrossFade("Unequip", 0.2f);
            }
        }
    }
}