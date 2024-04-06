using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Items
{
#if DEBUG
    public sealed class TelescopicSight : LITItem
    {
        private const string TOKEN = "LIT_ITEM_TELESCOPICSIGHT_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base proc chance for Telescopic Sight.")]
        [FormatToken(TOKEN)]
        public static float baseProcChance = 1f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra proc chance per stack of sights.")]
        [FormatToken(TOKEN, 1)]
        public static float procChancePerStack = 0.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Whether Telescopic Sight's instant kill should have a cooldown.")]
        public static bool enableCooldown = true;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Percentage of max health that's dealt to set exceptions when activated on them.")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float exceptionHealthPercentage = 0.2f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Cooldown between Telescopic Sight activations.")]
        [FormatToken(TOKEN, 3)]
        public static float cooldownDuration = 20f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Seconds removed from cooldown per stack.")]
        [FormatToken(TOKEN, 4)]
        public static float cooldownReductio = 2f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Whether Telescopic Sight should instakill elites.")]
        public static bool instakillElites = true;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Whether Telescopic Sight should instakill boss monsters.")]
        public static bool instakillBosses = false;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "TelescopicSight" - Items
             */
            yield break;
        }

        public class TelescopicSightBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.TelescopicSight;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (damageInfo.dotIndex == DotController.DotIndex.None)
                {
                    if (Util.CheckRoll(CalcChance() * damageInfo.procCoefficient))
                    {
                        //if (enableCooldown) 
                        //body.AddCooldownBuff(LITContent.Buffs.bdTeleSightCD, CalcCooldown());

                        var flag = ChooseWetherToInstakill(victimHealthComponent.body);
                        if (flag)
                        {
                            damageInfo.damage = victimHealthComponent.body.maxHealth * 4;
                        }
                        else
                        {
                            damageInfo.damage = victimHealthComponent.body.maxHealth * exceptionHealthPercentage;
                        }
                        Util.PlaySound("TeleSightProc", body.gameObject);
                    }
                }
            }
            private float CalcChance()
            {
                float stackChance = procChancePerStack * (stack - 1);
                return baseProcChance + stackChance;
            }
            private float CalcCooldown()
            {
                //Yknow, we should NEVER reach a cooldown of 0, so this caps the cooldown at around 10 seconds.
                return cooldownDuration - ((1 - 1 / (1 + 0.25f * (stack - 1))) * 10);
                //Agreeable.
            }
            /*
             * This code dictates wether the body can be instakilled or not, based off the config and the CharacterBody.
             * By default, only normal enemies & elites should be instakillable, Bosses are treated as exceptions.
             */
            //Hey, English lesson: "whether". There's no need for the 'h', but it's there for... some reason.
            //I fucking hate english.
            private bool ChooseWetherToInstakill(CharacterBody body)
            {
                if (body.isChampion)
                {
                    return instakillBosses;
                }
                if (body.isElite)
                {
                    return instakillElites;
                }
                return true;
            }
        }
    }
#endif
}
