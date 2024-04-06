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
    public sealed class AffixBlighted : LITEliteEquipment
    {
        [RiskOfOptionsConfigureField(LITConfig.EQUIPS, ConfigDescOverride = "Whether Teleporter Bosses should spawn as Blighted enemies.")]
        public static bool enableBlightedBosses = false;

        public override List<EliteDef> EliteDefs => new List<EliteDef> { _eliteDef };
        private EliteDef _eliteDef;
        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        private static GameObject _blightedBodyAttachment;

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
            /*
             * ExtendedEliteDef - "Blighted" - Equips
             * EquipmentDef - "AffixBlighted" - Equips
             * ArtifactDef - "Prestige" Artifacts
             * BuffDef - "bdAffixBlightedFake" - Equips
             * GameObject - "BlightedBodyAttachment" - Equips
             */
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
        public class AffixBlightedBehaviour : MSU.BuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixBlighted;
            public BuffDef FirstEliteBuff => _first ? _second.eliteEquipmentDef.passiveBuffDef : null;
            public BuffDef SecondEliteBuff => _second ? _second.eliteEquipmentDef.passiveBuffDef : null;

            private EliteDef _first;
            private EliteDef _second;
            private NetworkedBodyAttachment _attachment;
            private BlightedBodyAttachment _blightedAttachment;
            private float _aiRandomizeEliteStopwatch;

            private void Awake()
            {
                _attachment = Instantiate(_blightedAttachment).GetComponent<NetworkedBodyAttachment>();
                _blightedAttachment = _attachment.GetComponent<BlightedBodyAttachment>();
                _attachment.gameObject.SetActive(false);
            }

            private void OnEnable()
            {
                if(_attachment.attachedBody != CharacterBody)
                {
                    _attachment.AttachToGameObjectAndSpawn(CharacterBody.gameObject);
                }

                if (_attachment.attached)
                    _attachment.gameObject.SetActive(true);
            }

            private void OnDisable()
            {
                if (_attachment.attached)
                    _attachment.gameObject.SetActive(false);

                if (FirstEliteBuff)
                    CharacterBody.RemoveBuff(FirstEliteBuff);
                if (SecondEliteBuff)
                    CharacterBody.RemoveBuff(SecondEliteBuff);
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

            private void FixedUpdate()
            {
                UpdateIndividual(ref _first, _blightedAttachment.FirstIndex);
                UpdateIndividual(ref _second, _blightedAttachment.SecondIndex);

                //Makes ai blighted elites shuffle their elites every 60 seconds.
                if (NetworkServer.active && !CharacterBody.isPlayerControlled)
                {
                    _aiRandomizeEliteStopwatch -= Time.fixedDeltaTime;
                    if (_aiRandomizeEliteStopwatch < 0)
                    {
                        _aiRandomizeEliteStopwatch = LITContent.Equipments.AffixBlighted.cooldown;
                        RandomizeElites();
                    }
                }
            }

            private void UpdateIndividual(ref EliteDef eliteDef, EliteIndex index)
            {
                //If we dont have an eliteDef, and the incoming index is not none, update our current eliteDef
                if (!eliteDef)
                {
                    if (index != EliteIndex.None)
                    {
                        eliteDef = EliteCatalog.GetEliteDef(index);
                        if (NetworkServer.active && eliteDef.eliteEquipmentDef.passiveBuffDef)
                        {
                            CharacterBody.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                        }
                    }
                }
                else  //However, if we do have an elitedef, and the incoming inddex is different, update buffs.
                {
                    if (eliteDef.eliteIndex == index)
                        return;

                    if (NetworkServer.active)
                        CharacterBody.RemoveBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);

                    eliteDef = EliteCatalog.GetEliteDef(index);

                    if (NetworkServer.active)
                        CharacterBody.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                }
            }

            private void OnDestroy()
            {
                if (_attachment)
                    Destroy(_attachment.gameObject);

                if (FirstEliteBuff)
                    CharacterBody.RemoveBuff(FirstEliteBuff);
                if (SecondEliteBuff)
                    CharacterBody.RemoveBuff(SecondEliteBuff);
            }
        }
    }
}
