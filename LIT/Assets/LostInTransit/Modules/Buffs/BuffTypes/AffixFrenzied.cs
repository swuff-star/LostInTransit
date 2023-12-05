using Moonstorm;
using Moonstorm.Components;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;
using R2API;
using EntityStates;

namespace LostInTransit.Buffs
{
    public class AffixFrenzied : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdAffixFrenzied", LITBundle.Equips);

        private static GameObject _blinkReadyEffect;
        private static Type _stunState;
        private static Type _shockState;
        public override void Initialize()
        {
            base.Initialize();
            _blinkReadyEffect = LITAssets.LoadAsset<GameObject>("EffectFrenziedTPReady", LITBundle.Equips);
            _stunState = typeof(StunState);
            _shockState = typeof(ShockState);
        }

        //Rewrote this because i hate my old code -N
        public class AffixFrenziedBehaviour : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation()]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixFrenzied;
            private InputBankTest _inputBank;
            private EquipmentSlot _slot;
            private SetStateOnHurt _stateOnHurt;
            private EntityStateMachine _stateOnHurtTargetMachine;
            private GameObject _blinkReadyInstance;
            private float _aiCooldownStopwatch;

            protected override void Awake()
            {
                base.Awake();
                _inputBank = GetComponent<InputBankTest>();
                _slot = GetComponent<EquipmentSlot>();
                _stateOnHurt = GetComponent<SetStateOnHurt>();

                if(_stateOnHurt)
                {
                    _stateOnHurtTargetMachine = _stateOnHurt.targetStateMachine;
                }
            }

            private void Start()
            {
                body.MarkAllStatsDirty();
            }

            private void FixedUpdate()
            {
                //AI case
                if(!body.isPlayerControlled)
                {
                    AIFixedUpdate();
                    UpdateVisuals(true);
                    return;
                }

                //Player case

                //Dont upate visuals if our slot is not affix frenzied.
                if (!HasAffix())
                    return;

                UpdateVisuals(false);
            }

            private void UpdateVisuals(bool isAI)
            {
                //Player case
                if(!isAI)
                {
                    if (_slot.stock > 0)
                    {
                        if(!_blinkReadyInstance)
                        {
                            _blinkReadyInstance = Instantiate(_blinkReadyEffect, body.aimOriginTransform ? body.aimOriginTransform : transform);
                            _blinkReadyInstance.transform.localScale *= body.radius;
                        }
                    }
                    else if (_blinkReadyInstance)
                        Destroy(_blinkReadyInstance);
                    return;
                }

                //AI case
                bool hasAffix = HasAffix();
                if(hasAffix && _slot.stock > 0 || !hasAffix && _aiCooldownStopwatch < 0)
                {
                    if(!_blinkReadyInstance)
                    {
                        _blinkReadyInstance = Instantiate(_blinkReadyEffect, body.aimOriginTransform ? body.aimOriginTransform : transform);
                        _blinkReadyInstance.transform.localScale *= body.radius;
                    }
                }
                else if(_blinkReadyInstance)
                    Destroy(_blinkReadyInstance);
            }

            //Related code is ran only if the body is controlled dby an AI
            private void AIFixedUpdate()
            {
                //AI Case, we check the dedicated buff related cooldown.
                if (!(_aiCooldownStopwatch < 0))
                    _aiCooldownStopwatch -= Time.fixedDeltaTime;

                if (_aiCooldownStopwatch <= 0 && Util.HasEffectiveAuthority(gameObject))
                {
                    //If somehow the ai has more equip stocks, we can make themm tp every 2 seconds as long as they have stocks, idk soundsd funny lmao -N
                    if (HasAffix() && _slot.stock > 0)
                    {
                        if(NetworkServer.active)
                            _slot.ExecuteIfReady();
                        _aiCooldownStopwatch = 2;
                    }
                    else if(!HasAffix()) //We dont have the affix itself, teleport anyways and enter buff specific cooldown
                    {
                        Equipments.AffixFrenzied.FireActionStatic(gameObject);
                        _aiCooldownStopwatch = LITContent.Equipments.AffixFrenzied.cooldown;
                    }
                }

                bool isFrozen = body.healthComponent.isInFrozenState;
                bool isStunned = false;
                if (_stateOnHurt)
                {
                    Type currentTargetMachineState = _stateOnHurtTargetMachine.state.GetType();
                    isStunned = currentTargetMachineState == _stunState || currentTargetMachineState == _shockState;
                }

                if (isFrozen || isStunned)
                {
                    _aiCooldownStopwatch = 4;
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += 0.5f;
                args.attackSpeedMultAdd += 0.5f;

                args.cooldownMultAdd -= 0.5f;
            }

            private bool HasAffix()
            {
                return _slot && _slot.equipmentIndex == LITContent.Equipments.AffixFrenzied.equipmentIndex;
            }

            private void OnDestroy()
            {
                if (_blinkReadyInstance)
                    Destroy(_blinkReadyInstance);
            }
        }
    }
}
