using EntityStates;
using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Equipments
{
    public sealed class AffixFrenzied : LITEliteEquipment, IContentPackModifier
    {
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private AssetCollection _assetCollection;
        private static GameObject _blinkReadyEffect;
        private static Type _stunState;
        private static Type _shockState;

        public override bool Execute(EquipmentSlot slot)
        {
            return FireActionStatic(slot.gameObject);
        }

        internal static bool FireActionStatic(GameObject bodyObj)
        {
            var bodyStateMachine = EntityStateMachine.FindByCustomName(bodyObj, "Body");
            var healthComponent = bodyObj.GetComponent<HealthComponent>();
            if (healthComponent.alive && bodyStateMachine)
            {
                //Todd Howard Voice: It just works.
                bodyStateMachine.SetNextState(new EntityStates.AffixFrenzied.FrenziedTeleport());
                return true;
            }

            return false;
        }

        public override void Initialize()
        {
            _stunState = typeof(StunState);
            _shockState = typeof(ShockState);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acAffixFrenzied", LITBundle.Equips);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _eliteDefs = new List<EliteDef>
            {
                _assetCollection.FindAsset<ExtendedEliteDef>("Frenzied"),
                _assetCollection.FindAsset<ExtendedEliteDef>("FrenziedHonor")
            };
            _equipmentDef = _assetCollection.FindAsset<EquipmentDef>("AffixFrenzied");
            _blinkReadyEffect = _assetCollection.FindAsset<GameObject>("EffectFrenziedTPReady");
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        //Rewrote this because i hate my old code -N
        public class AffixFrenziedBehaviour : BuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixFrenzied;
            private InputBankTest _inputBank;
            private EquipmentSlot _slot;
            private SetStateOnHurt _stateOnHurt;
            private EntityStateMachine _stateOnHurtTargetMachine;
            private GameObject _blinkReadyInstance;
            private float _aiCooldownStopwatch;

            protected void Awake()
            {
                _inputBank = GetComponent<InputBankTest>();
                _slot = GetComponent<EquipmentSlot>();
                _stateOnHurt = GetComponent<SetStateOnHurt>();

                if (_stateOnHurt)
                {
                    _stateOnHurtTargetMachine = _stateOnHurt.targetStateMachine;
                }
            }

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();

                CharacterBody.MarkAllStatsDirty();
            }

            private void FixedUpdate()
            {
                //AI case
                if (!CharacterBody.isPlayerControlled)
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
                if (!isAI)
                {
                    if (_slot.stock > 0)
                    {
                        if (!_blinkReadyInstance)
                        {
                            _blinkReadyInstance = Instantiate(_blinkReadyEffect, CharacterBody.aimOriginTransform ? CharacterBody.aimOriginTransform : transform);
                            _blinkReadyInstance.transform.localScale *= CharacterBody.radius;
                        }
                    }
                    else if (_blinkReadyInstance)
                        Destroy(_blinkReadyInstance);
                    return;
                }

                //AI case
                bool hasAffix = HasAffix();
                if (hasAffix && _slot.stock > 0 || !hasAffix && _aiCooldownStopwatch < 0)
                {
                    if (!_blinkReadyInstance)
                    {
                        _blinkReadyInstance = Instantiate(_blinkReadyEffect, CharacterBody.aimOriginTransform ? CharacterBody.aimOriginTransform : transform);
                        _blinkReadyInstance.transform.localScale *= CharacterBody.radius;
                    }
                }
                else if (_blinkReadyInstance)
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
                        if (NetworkServer.active)
                            _slot.ExecuteIfReady();
                        _aiCooldownStopwatch = 2;
                    }
                    else if (!HasAffix()) //We dont have the affix itself, teleport anyways and enter buff specific cooldown
                    {
                        Equipments.AffixFrenzied.FireActionStatic(gameObject);
                        _aiCooldownStopwatch = LITContent.Equipments.AffixFrenzied.cooldown;
                    }
                }

                bool isFrozen = CharacterBody.healthComponent.isInFrozenState;
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
                if (!enabled)
                    return;

                args.moveSpeedMultAdd += 0.5f;
                args.attackSpeedMultAdd += 0.5f;

                args.cooldownMultAdd -= 0.5f;
            }

            private bool HasAffix()
            {
                return _slot && _slot.equipmentIndex == LITContent.Equipments.AffixFrenzied.equipmentIndex;
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();

                if (_blinkReadyInstance)
                    Destroy(_blinkReadyInstance);
            }
        }
    }
}
