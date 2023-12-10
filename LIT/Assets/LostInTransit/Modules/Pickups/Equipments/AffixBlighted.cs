using KinematicCharacterController;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace LostInTransit.Equipments
{
    public class AffixBlighted : EliteEquipmentBase
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips),
        };

        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("AffixBlighted", LITBundle.Equips);

        [ConfigurableField(ConfigName = "Boss Blighted", ConfigDesc = "Whether Teleporter Bosses should spawn as Blighted enemies.")]
        public static bool bossBlighted = false;


        public override void Initialize()
        {
            HG.ArrayUtils.ArrayAppend(ref LITContent.Instance.SerializableContentPack.artifactDefs, LITAssets.LoadAsset<ArtifactDef>("Prestige", LITBundle.Artifacts));
            HG.ArrayUtils.ArrayAppend(ref LITContent.Instance.SerializableContentPack.buffDefs, LITAssets.LoadAsset<BuffDef>("bdAffixBlightedFake", LITBundle.Equips));
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GiveFakeBlightBuff;
        }

        //If the killer has wake of vultures, and kills a blighted elite, it seems to give them the buff itself, but the buff doesnt sync the victim's elites properly, leaving a chance for permanent elite effects. Instead we'll give the player a "Fake" buff, which has the same icon but doesnt have any behaviours.
        private void GiveFakeBlightBuff(MonoMod.Cil.ILContext il)
        {
            var cursor = new ILCursor(il);

            ILLabel label = null;
            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloc(2),
                x => x.MatchLdloc(73),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out label));

            if(!flag)
            {
                LITLog.Fatal("Failed to implement fake blight buff with Wake of Vultures! Attackers who recieve the regular blight buff might get infinite duration elite buffs!");
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

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BlightStatIncrease>(stack);
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if(slot.TryGetComponent<Buffs.AffixBlighted.AffixBlightedBehaviour>(out var behaviour))
            {
                behaviour.RandomizeElites();
                return true;
            }
            return false;
        }

        //Stat increase is done via having this equipment, to avoid huge stat changes when using wake of vultures
        //plus, having this kind of power is pog, and if you get the drop then i think you've earned the powertrip -N
        public class BlightStatIncrease : CharacterBody.ItemBehavior
        {
            public void Start()
            {
                body.baseMaxHealth *= 7.0f;
                body.baseDamage *= 1.6f;
                body.baseMoveSpeed *= 1.1f;

                body.PerformAutoCalculateLevelStats();

                body.healthComponent.health = body.healthComponent.fullHealth;

                Util.PlaySound("BlightAppear", body.gameObject);

                CharacterModel cm = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                MotionTrailGenerator mtg = cm.mainSkinnedMeshRenderer.gameObject.AddComponent<MotionTrailGenerator>();

                mtg.On();
            }


            public void OnDestroy()
            {
                if(body.healthComponent.alive)
                {
                    body.baseMaxHealth /= 7.0f;
                    body.baseDamage /= 1.4f;
                    body.baseMoveSpeed /= 1.1f;
                    body.PerformAutoCalculateLevelStats();
                }
            }
        }
    }
}
