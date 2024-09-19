using KinematicCharacterController;
using LostInTransit.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using UnityEngine.XR;

namespace LostInTransit.Equipments
{
    public sealed class AffixBlighted : LITEliteEquipment, IContentPackModifier
    {
        [RiskOfOptionsConfigureField(LITConfig.EQUIPS, configDescOverride = "Whether Teleporter Bosses should spawn as Blighted enemies.")]
        public static bool enableBlightedBosses = false;

        public override List<EliteDef> eliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;
        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;

        public override EquipmentDef equipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        private static GameObject _blightedBodyAttachment;
        private AssetCollection _blightedAssetCollection;

        public override bool Execute(EquipmentSlot slot)
        {
            if (slot.TryGetComponent<AffixBlightedBehaviour>(out var behaviour))
            {
                behaviour.RandomizeElites();
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GiveFakeBlightBuff;
            RoR2Application.onLateUpdate += UpdateBlightedDisplayMaterials;
        }

        private void UpdateBlightedDisplayMaterials()
        {
            foreach(var affixBlightedBehaviour in InstanceTracker.GetInstancesList<AffixBlightedBehaviour>())
            {
                affixBlightedBehaviour.InstanceUpdate();
            }
        }

        //If the killer has wake of vultures, and kills a blighted elite, it seems to give them the buff itself, but the buff doesnt sync the victim's elites properly, leaving a chance for permanent elite effects. Instead we'll give the player a "Fake" buff, which has the same icon but doesnt have any behaviours. I should probably figure out a better way to avoid this, but idk. -N
        private void GiveFakeBlightBuff(MonoMod.Cil.ILContext il)
        {
            var cursor = new ILCursor(il);

            ILLabel label = null;
            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloc(2),
                x => x.MatchLdloc(73),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out label));

            if (!flag)
            {
                LITLog.Fatal("Failed to implement fake blight buff with Wake of Vultures! Attackers who recieve the regular blight buff might get infinite duration elite buffs!");
                return;
            }

            cursor.Emit(OpCodes.Ldloc, 15);
            cursor.Emit(OpCodes.Ldloc, 73);
            cursor.Emit(OpCodes.Ldloc, 71);
            cursor.EmitDelegate<Func<CharacterBody, BuffIndex, float, bool>>((attackerBody, buffIndex, duration) =>
            {
                //If the buff is the regular blight buff, add the fake buff instead and return true
                if (buffIndex == LITContent.Buffs.bdAffixBlighted.buffIndex)
                {
                    attackerBody.AddTimedBuff(LITContent.Buffs.bdAffixBlightedFake, duration);
                    return true;
                }
                return false;
            });
            cursor.Emit(OpCodes.Brtrue, label);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acAffixBlighted", LITBundle.Equips);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _blightedAssetCollection = request.Asset;
            
            _eliteDefs = new List<EliteDef>
            {
                _blightedAssetCollection.FindAsset<ExtendedEliteDef>("Blighted")
            };

            _equipmentDef = _blightedAssetCollection.FindAsset<EquipmentDef>("AffixBlighted");
            _blightedBodyAttachment = _blightedAssetCollection.FindAsset<GameObject>("BlightedBodyAttachment");
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            body.AddItemBehavior<BlightStatIncrease>(0);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddItemBehavior<BlightStatIncrease>(1);
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_blightedAssetCollection);
        }

        //Stat increase is done via having this equipment, to avoid huge stat changes when using wake of vultures
        //plus, having this kind of power is pog, and if you get the drop then i think you've earned the powertrip -N
        public class BlightStatIncrease : CharacterBody.ItemBehavior
        {
            private float origHealth;
            private float origDamage;
            private float origMoveSpeed;
            public void Start()
            {
                origHealth = body.baseMaxHealth;
                origDamage = body.baseDamage;
                origMoveSpeed = body.baseMoveSpeed;

                body.baseMaxHealth *= 13.5f;
                body.baseDamage *= 1.6f;
                body.baseMoveSpeed *= 1.2f;

                body.PerformAutoCalculateLevelStats();
                body.MarkAllStatsDirty();

                body.healthComponent.health = body.healthComponent.fullHealth;

                Util.PlaySound("BlightAppear", body.gameObject);

                CharacterModel cm = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                MotionTrailGenerator mtg = cm.mainSkinnedMeshRenderer.gameObject.AddComponent<MotionTrailGenerator>();

                mtg.On();
            }


            public void OnDestroy()
            {
                if (body.healthComponent.alive)
                {
                    body.baseMaxHealth = origHealth;
                    body.baseDamage = origDamage;
                    body.baseMoveSpeed = origMoveSpeed;
                    body.PerformAutoCalculateLevelStats();
                    body.MarkAllStatsDirty();
                }
            }
        }

        //Makes sure the body attachment gets attached whenever the buff is active, and also handles changing the blighted elite's buffdefs.
        public class AffixBlightedBehaviour : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixBlighted;
            public BuffDef FirstEliteBuff => _first ? _first.eliteEquipmentDef.passiveBuffDef : null;
            public BuffDef SecondEliteBuff => _second ? _second.eliteEquipmentDef.passiveBuffDef : null;

            private EliteDef _first;
            private EliteDef _second;
            private NetworkedBodyAttachment _attachment;
            private BlightedBodyAttachment _blightedAttachment;
            private float _aiRandomizeEliteStopwatch;

            private List<CharacterModel.ParentedPrefabDisplay> _firstDisplays = new List<CharacterModel.ParentedPrefabDisplay>();
            private List<CharacterModel.ParentedPrefabDisplay> _secondDisplays = new List<CharacterModel.ParentedPrefabDisplay>();
            private CharacterModel _characterModel;
            private ChildLocator _childLocator;
            private bool _supportsMultipleDisplays;
            protected override void Awake()
            {
                base.Awake();
                _attachment = Instantiate(_blightedBodyAttachment).GetComponent<NetworkedBodyAttachment>();
                _blightedAttachment = _attachment.GetComponent<BlightedBodyAttachment>();
                _attachment.gameObject.SetActive(false);
            }

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                var characterBody = GetComponent<CharacterBody>();
                if (_attachment.attachedBody != characterBody)
                {
                    _attachment.AttachToGameObjectAndSpawn(characterBody.gameObject);
                }

                var modelTransform = characterBody.modelLocator ? characterBody.modelLocator.modelTransform : null;
                if(modelTransform)
                {
                    _characterModel = modelTransform.GetComponent<CharacterModel>();
                    _childLocator = modelTransform.GetComponent<ChildLocator>();
                    _supportsMultipleDisplays = _characterModel && _childLocator && _characterModel.itemDisplayRuleSet;
                }

                if (_attachment.attached)
                    _attachment.gameObject.SetActive(true);

                InstanceTracker.Add(this);
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();
                var characterBody = GetComponent<CharacterBody>();

                if (_attachment.attached)
                    _attachment.gameObject.SetActive(false);

                if (FirstEliteBuff)
                    characterBody.RemoveBuff(FirstEliteBuff);
                if (SecondEliteBuff)
                    characterBody.RemoveBuff(SecondEliteBuff);

                UndoDisplays(_firstDisplays);
                UndoDisplays(_secondDisplays);

                InstanceTracker.Remove(this);
            }

            private void Start()
            {
                _aiRandomizeEliteStopwatch = LITContent.Equipments.AffixBlighted.cooldown;
            }

            public void RandomizeElites()
            {
                if (!NetworkServer.active)
                    return;

                _blightedAttachment.RandomizeElites();
            }

            internal void InstanceUpdate()
            {
                if (!_supportsMultipleDisplays || !_characterModel)
                    return;

                if (!_characterModel.materialsDirty)
                    return;

                Color value = Color.black;
                if(characterBody && characterBody.healthComponent)
                {
                    float num = Mathf.Clamp01(1f - characterBody.healthComponent.timeSinceLastHit / CharacterModel.hitFlashDuration);
                    float num2 = Mathf.Pow(Mathf.Clamp01(1f - characterBody.healthComponent.timeSinceLastHeal / CharacterModel.healFlashDuration), 0.5f);
                    value = ((!(num2 > num)) ? (((characterBody.healthComponent.shield > 0f) ? CharacterModel.hitFlashShieldColor : CharacterModel.hitFlashBaseColor) * num) : (CharacterModel.healFlashColor * num2));
                }
                for(int i = 0; i < _firstDisplays.Count; i++)
                {
                    UpdateSingle(_firstDisplays[i].itemDisplay);
                }
                for(int i = 0; i < _secondDisplays.Count; i++)
                {
                    UpdateSingle(_secondDisplays[i].itemDisplay);
                }

                void UpdateSingle(ItemDisplay itemDisplay)
                {
                    for(int i = 0; i < itemDisplay.rendererInfos.Length; i++)
                    {
                        Renderer renderer = itemDisplay.rendererInfos[i].renderer;
                        renderer.GetPropertyBlock(_characterModel.propertyStorage);
                        _characterModel.propertyStorage.SetColor(CommonShaderProperties._FlashColor, value);
                        _characterModel.propertyStorage.SetFloat(CommonShaderProperties._Fade, _characterModel.fade);
                        renderer.SetPropertyBlock(_characterModel.propertyStorage);
                    }
                }
            }

            private void FixedUpdate()
            {
                UpdateIndividual(ref _first, _firstDisplays, _blightedAttachment.firstIndex);
                UpdateIndividual(ref _second, _secondDisplays, _blightedAttachment.secondIndex);

                //Makes ai blighted elites shuffle their elites every 60 seconds.
                if (NetworkServer.active && !characterBody.isPlayerControlled)
                {
                    _aiRandomizeEliteStopwatch -= Time.fixedDeltaTime;
                    if (_aiRandomizeEliteStopwatch < 0)
                    {
                        _aiRandomizeEliteStopwatch = LITContent.Equipments.AffixBlighted.cooldown;
                        RandomizeElites();
                    }
                }
            }

            private void UpdateIndividual(ref EliteDef eliteDef, List<CharacterModel.ParentedPrefabDisplay> displays, EliteIndex index)
            {
                //If we dont have an eliteDef, and the incoming index is not none, update our current eliteDef
                if (!eliteDef)
                {
                    if (index != EliteIndex.None)
                    {
                        eliteDef = EliteCatalog.GetEliteDef(index);

                        if(_supportsMultipleDisplays)
                            ActivateDisplays(eliteDef, displays);
    
                        if (NetworkServer.active && eliteDef.eliteEquipmentDef.passiveBuffDef)
                        {
                            characterBody.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                        }
                    }
                }
                else  //However, if we do have an elitedef, and the incoming inddex is different, update buffs.
                {
                    if (eliteDef.eliteIndex == index)
                        return;

                    if(_supportsMultipleDisplays)
                        UndoDisplays(displays);
    
                    if (NetworkServer.active)
                        characterBody.RemoveBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);

                    eliteDef = EliteCatalog.GetEliteDef(index);

                    if(_supportsMultipleDisplays)
                        ActivateDisplays(eliteDef, displays);
    
                    if (NetworkServer.active)
                        characterBody.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                }
            }
            
            private void UndoDisplays(List<CharacterModel.ParentedPrefabDisplay> displays)
            {
                for(int i = displays.Count - 1; i >= 0; i--)
                {
                    displays[i].Undo();
                    displays.RemoveAt(i);
                }
            }

            private void ActivateDisplays(EliteDef eliteDef, List<CharacterModel.ParentedPrefabDisplay> container)
            {
                DisplayRuleGroup group = _characterModel.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(eliteDef.eliteEquipmentDef.equipmentIndex);
                
                if(group.rules == null)
                {
                    return;
                }
                for(int i = 0; i < group.rules.Length; i++)
                {
                    ItemDisplayRule rule = group.rules[i];
                    Transform transform = _childLocator.FindChild(rule.childName);
                    if (!transform)
                        continue;

                    CharacterModel.ParentedPrefabDisplay display = default(CharacterModel.ParentedPrefabDisplay);
                    display.itemIndex = ItemIndex.None;
                    display.equipmentIndex = eliteDef.eliteEquipmentDef.equipmentIndex;
                    display.Apply(_characterModel, rule.followerPrefab, transform, rule.localPos, Quaternion.Euler(rule.localAngles), rule.localScale);
                    container.Add(display);
                }
            }
        }
    }
}
