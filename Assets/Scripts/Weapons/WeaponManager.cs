using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starlight.Weapons.Animation;
namespace Starlight.Weapons
{
    public class WeaponManager : MonoBehaviour, IManagedBehaviour
    {
        CharacterMotor cm;
        Animator animator;
        AnimatorOverrideController aoc;
        [SerializeField] List<BaseWeapon> weapons;
        internal BaseWeapon CurrentWeapon { get { return weapons[weaponIndex]; } }
        [SerializeField] int weaponIndex;
        bool fireInput;
        protected AnimationClipOverrides clipOverrides;
        [SerializeField] internal Transform fireTransform;
        private void Awake()
        {
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
            cm = GetComponent<CharacterMotor>();
            animator = cm.GetAnimator;

            //We need to set up the override controller that'll do all the good shit for us
            //Create a new AnimatorOverrideController
            aoc = new(animator.runtimeAnimatorController);
            //Now we need to set the runtimeAnimatorController for our Animator to our AOC, so we can play about with the animations.
            animator.runtimeAnimatorController = aoc;
            //We need to get our list of overrides now
            clipOverrides = new AnimationClipOverrides(aoc.overridesCount);
            aoc.GetOverrides(clipOverrides);


            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
            weapons[weaponIndex].gameObject.SetActive(true);
            SetVariables();
        }
        private void Start()
        {
            //Now we call SetAnimations() in order to actually set those clips.
            SetAnimations();
        }
        internal void SetFireInput(bool fireInput)
        {
            this.fireInput = fireInput;
        }

        public void ManagedFixedUpdate()
        {
        }

        public void ManagedLateUpdate()
        {
            if (weapons[weaponIndex])
            {
                weapons[weaponIndex].SetFireInput(fireInput);
            }
        }
        public void ManagedUpdate()
        {

        }
        internal void SwitchWeapon()
        {
            weapons[weaponIndex].gameObject.SetActive(false);
            weaponIndex++;
            weaponIndex %= weapons.Count;
            weapons[weaponIndex].gameObject.SetActive(true);
            SetAnimations();
            SetVariables();
        }
        internal void SetVariables()
        {
            cm.SetFocusSpeed(CurrentWeapon.FocusSpeed);
            cm.focusCameraTarget = CurrentWeapon.focusTransform;
            cm.linearBobMultiplier = CurrentWeapon.linearBobMultiplier;
            cm.angularBobMultiplier = CurrentWeapon.angularBobMultiplier;
            cm.linearBobSpeed = CurrentWeapon.linearBobSpeed;
            cm.angularBobSpeed = CurrentWeapon.angularBobSpeed;
            cm.recoilDecay = CurrentWeapon.recoilDecay;
            cm.recoilLerpSpeed = CurrentWeapon.recoilLerpSpeed;
            cm.idleBobLerpSpeedLinear = CurrentWeapon.idleBobLerpSpeedLinear;
            cm.idleBobLerpSpeedAngular = CurrentWeapon.idleBobLerpSpeedAngular;
            cm.movingBobLerpSpeedLinear = CurrentWeapon.movingBobLerpSpeedLinear;
            cm.movingBobLerpSpeedAngular = CurrentWeapon.movingBobLerpSpeedAngular;
            cm.movingBobLerpScaleLinear = CurrentWeapon.movingBobLerpScaleLinear;
            cm.movingBobLerpScaleAngular = CurrentWeapon.movingBobLerpScaleAngular;
            cm.idleBobLerpScaleLinear = CurrentWeapon.idleBobLerpScaleLinear;
            cm.idleBobLerpScaleAngular = CurrentWeapon.idleBobLerpScaleAngular;
            cm.bobFocusMultiplier = CurrentWeapon.bobFocusMultiplier;
        }
        internal void SetAnimations()
        {
            AnimationContainer ac = CurrentWeapon.AnimContainer;
            for (int i = 0; i < ac.clipList.Count; i++)
            {
                clipOverrides[ac.clipList[i].name] = ac.clipList[i].clip;
            }
            aoc.ApplyOverrides(clipOverrides);
        }
    }
}