using Starlight.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlight
{
    public class CharacterMotor : MonoBehaviour, IManagedBehaviour
    {


        [SerializeField] WeaponManager wm;

        [SerializeField] Transform aimRotationTransform;
        [SerializeField] Transform aimRotationAnchor;

        [SerializeField, Header("Aiming")] Vector2 lookInput;
        [SerializeField] Vector2 lookSpeed;
        [SerializeField] Vector2 lookAngle;
        [SerializeField] float currentLeanAngle;
        [SerializeField] float leanBounds, leanSpeed;
        [SerializeField] float lookPitchClamp = 75;
        [SerializeField] float leanInput;
        float leanDampVelocity;
        [SerializeField, Space(10), Header("Movement")] Vector2 moveInput;
        [SerializeField] Vector2 dampedMovement;
        Vector2 dampVelocity;
        [SerializeField] float movementRampSpeed, moveSpeedMultiplier;

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
        [SerializeField] float recoilLerpSpeed;
        [SerializeField] float recoilDecay;
        Vector3 currentLinearRecoil, currentAngularRecoil;
        Vector3 linearRecoilTarget, angularRecoilTarget;
        [SerializeField] Vector3 linearRecoilClamp, angularRecoilClamp;
        private void Awake()
        {
            BehaviourManager.fixedUpdateClients += ManagedFixedUpdate;
            BehaviourManager.updateClients += ManagedUpdate;
            BehaviourManager.lateUpdateClients += ManagedLateUpdate;
        }

        public void ManagedFixedUpdate()
        {
            dampedMovement = Vector2.SmoothDamp(dampedMovement, moveInput, ref dampVelocity, movementRampSpeed);
            transform.Translate(moveSpeedMultiplier * Time.fixedDeltaTime * new Vector3(dampedMovement.x, 0, dampedMovement.y), Space.Self);
            currentLeanAngle = Mathf.SmoothDamp(currentLeanAngle, leanInput * leanBounds, ref leanDampVelocity, leanSpeed);
        }

        public void ManagedLateUpdate()
        {
            AimRotation();
            AimSway();

        }

        void AimRotation()
        {

            aimRotationTransform.position = aimRotationAnchor.position;
            lookAngle += lookInput * lookSpeed * Time.fixedDeltaTime;
            lookAngle.y = Mathf.Clamp(lookAngle.y, -lookPitchClamp, lookPitchClamp);
            lookDelta = oldLook - lookAngle;
            aimRotationTransform.localRotation = Quaternion.Euler(lookAngle.y, 0, 0);
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
            currentLookSway = Vector2.Lerp(currentLookSway, lookSwayMultiplied, Time.smoothDeltaTime * lookSwayLerpSpeed);
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
    }
}