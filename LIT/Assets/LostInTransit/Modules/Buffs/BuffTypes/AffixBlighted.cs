using LostInTransit.Components;
using Moonstorm;
using Moonstorm.Components;
using RoR2;
using UnityEngine;

namespace LostInTransit.Buffs
{
    //N- Buffs no longer have an "AddBehavior(ref CharacterBody body, int stacks)" method
    public sealed class AffixBlighted : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("AffixBlighted");

        //N- Buff behaviors now use moonstorm's base buff body behavior
        //The class itself is abstract and its in Moonstorm.Components
        public class AffixBlightedBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            //N- This attribute is needed for the system to automatically take care of adding the behavior or not depending on the buff stacks.
            //It NEEDS to be static, return a buffDef, and have this attribute.
            //If youre not sure wether the behavior needs to be on client, on server, or both, just set "useOnClient" and "useOnServer" to true.
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.AffixBlighted;


            public BlightedController MasterBehavior { get => body.masterObject?.GetComponent<BlightedController>(); }

            public BuffDef firstBuff;

            public BuffDef secondBuff;

            private GameObject SmokeEffect = LITAssets.LoadAsset<GameObject>("BlightSmoke");

            private float stopwatch;
            private static float checkTimer = 0.5f;

            public void Start()
            {
                MasterBehavior.enabled = true;
                MasterBehavior.BuffBehavior = this;

                body.onSkillActivatedServer += RemoveBuff;
            }

            private void RemoveBuff(GenericSkill obj = null)
            {
                //if (body.hasCloakBuff)
                    //body.RemoveBuff(RoR2Content.Buffs.Cloak);
                //SpawnEffect();
                stopwatch = 0f;
            }

            public void SetElites(int index1, int index2)
            {
                EliteDef firstElite = EliteCatalog.GetEliteDef((EliteIndex)index1);
                EliteDef secondElite = EliteCatalog.GetEliteDef((EliteIndex)index2);

                //Dont replace the buff if theyre a match.
                if (firstBuff != firstElite.eliteEquipmentDef.passiveBuffDef)
                {
                    body.RemoveBuff(firstBuff);
                    firstBuff = firstElite.eliteEquipmentDef.passiveBuffDef;
                    body.AddBuff(firstBuff);
                }

                if (secondBuff != secondElite.eliteEquipmentDef.passiveBuffDef)
                {
                    body.RemoveBuff(secondBuff);
                    secondBuff = secondElite.eliteEquipmentDef.passiveBuffDef;
                    body.AddBuff(secondBuff);
                }
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > checkTimer && !body.HasBuff(RoR2Content.Buffs.Cloak))
                {
                    //body.AddBuff(RoR2Content.Buffs.Cloak);
                }
                else if (Util.CheckRoll(1))
                {
                    stopwatch = 0; //Doing this to ensure they're visible for a moment
                    //RemoveBuff();
                    //SpawnEffect();
                }
            }

            private void SpawnEffect()
            {
                EffectData data = new EffectData
                {
                    scale = body.radius * 0.75f,
                    origin = body.transform.position
                };
                EffectManager.SpawnEffect(SmokeEffect, data, true);
                //Util.PlaySound("BlightAppear", body.gameObject); too obnoxious
            }

            public void RecalculateStatsStart() { }

            public void RecalculateStatsEnd()
            {
                //Reduces cooldowns by 50%
                if (body.skillLocator.primary)
                    body.skillLocator.primary.cooldownScale *= 0.7f;
                if (body.skillLocator.secondary)
                    body.skillLocator.secondary.cooldownScale *= 0.7f;
                if (body.skillLocator.utility)
                    body.skillLocator.utility.cooldownScale *= 0.7f;
                if (body.skillLocator.special)
                    body.skillLocator.special.cooldownScale *= 0.7f;
                //Is there a reason you subtract CDR instead of multiply? If two things gave 0.5 CDR like this, then it'd have 0 CDR... Right?
                //Also, if these need nerfed, I say this is the first thing to go.
                //Neb: i supose i can change it to multiply by 0.5 if they need nerfed. we'll see in the future.
            }

            public void OnDestroy()
            {
                if (MasterBehavior)
                    MasterBehavior.enabled = false;
                if (body)
                {
                    body?.RemoveBuff(firstBuff);
                    body?.RemoveBuff(secondBuff);

                    body.onSkillActivatedServer -= RemoveBuff;

                    //body?.RemoveBuff(RoR2Content.Buffs.Cloak);
                }
            }
        }
    }
}