using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlight
{
    public class CharacterMotor : MonoBehaviour, IManagedBehaviour
    {
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
            aimRotationTransform.position = aimRotationAnchor.position;
            lookAngle += lookInput * lookSpeed * Time.fixedDeltaTime;
            lookAngle.y = Mathf.Clamp(lookAngle.y, -lookPitchClamp, lookPitchClamp);
            aimRotationTransform.localRotation = Quaternion.Euler(lookAngle.y, 0, 0);
            aimRotationTransform.localRotation *= Quaternion.Euler(0, 0, currentLeanAngle);
            transform.localRotation = Quaternion.Euler(0, lookAngle.x, 0);
            lookAngle.x %= 360;
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
    }
}