using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LITArchWisp
{
    public class LITArchWispCharacterMain : FlyState
    {
        public static string isMovingParamName;
        public static string flySpeedParamName;
        public static string forwardSpeedParamName;
        public static string rightSpeedParamName;

        private int _fwdSpeedID;
        private int _rightSpeedID;
        private int _isMovingID;
        private int _flySpeedParamName;
        private CharacterAnimatorWalkParamCalculator _animatorWalkParamCalculator;
        protected BodyAnimatorSmoothingParameters.SmoothingParameters smoothingParams;

        public override void OnEnter()
        {
            base.OnEnter();
            GetBodyAnimatorSmoothingParameters(out smoothingParams);
            _fwdSpeedID = Animator.StringToHash(forwardSpeedParamName);
            _rightSpeedID = Animator.StringToHash(rightSpeedParamName);
            _isMovingID = Animator.StringToHash(isMovingParamName);
            _flySpeedParamName = Animator.StringToHash(flySpeedParamName);
        }

        public override void Update()
        {
            base.Update();
            if (!isAuthority)
                return;

            var moveVector = inputBank.moveVector.normalized;

            var flySpeed = moveSpeedStat / characterBody.baseMoveSpeed;

            _animatorWalkParamCalculator.Update(inputBank.moveVector, transform.forward, in smoothingParams, Time.deltaTime);

            modelAnimator.SetBool(_isMovingID, moveVector != Vector3.zero);
            modelAnimator.SetFloat(_flySpeedParamName, flySpeed);

            modelAnimator.SetFloat(_fwdSpeedID, _animatorWalkParamCalculator.animatorWalkSpeed.x, smoothingParams.forwardSpeedSmoothDamp, Time.deltaTime);
            modelAnimator.SetFloat(_rightSpeedID, _animatorWalkParamCalculator.animatorWalkSpeed.y, smoothingParams.rightSpeedSmoothDamp, Time.deltaTime);
        }
    }
}
