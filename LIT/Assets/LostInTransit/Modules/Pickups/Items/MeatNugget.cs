using R2API;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using RoR2;
using LostInTransit.Components;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public sealed class MeatNugget : LITItem
    {
        private const string TOKEN = "LIT_ITEM_MEATNUGGET_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Proc chance for Meat Nugget.")]
        [FormatToken(TOKEN, 0)]
        public static float procChance = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount added to regen by nugget pickup.")]
        [FormatToken(TOKEN, 1)]
        public static float regenBonus = 1.6f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "If true, the regen buff duration can stack up to the number of Meat Nuggets you have.")]
        public static bool regenStacking = true;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base duration of the regen buff granted by dropped nuggets.")]
        [FormatToken(TOKEN, 2)]
        public static float baseDuration = 2;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra duration of the regen buff per stack of Meat Nugget.")]
        [FormatToken(TOKEN, 3)]
        public static float stackDuration = 1;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private static GameObject _meatNuggetPickup;
        private BuffDef _nuggetRegen;

        public override void Initialize()
        {
            var schmeat = Resources.Load<BuffDef>("buffdefs/MeatRegenBoost");
            _nuggetRegen.iconSprite = schmeat.iconSprite;
            _nuggetRegen.startSfx = schmeat.startSfx;

            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(_nuggetRegen);

            args.baseRegenAdd += (regenBonus + ((regenBonus / 5) * sender.level)) * buffCount;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "MeatNugget" - Items
             * GameObject - "MeatNuggetPickup - Items
             * BuffDef - "bdNuggetRegen" - Items
             */
            yield break;
        }

        public class MeatNuggetBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.MeatNugget;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                GameObject victim = damageReport.victim.gameObject;
                if (Util.CheckRoll(procChance * damageReport.damageInfo.procCoefficient, damageReport.attackerMaster))
                {
                    GameObject nugget = UnityEngine.Object.Instantiate<GameObject>(_meatNuggetPickup, victim.transform.position, UnityEngine.Random.rotation);
                    nugget.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
                    NuggetPickup nugbuff = nugget.GetComponentInChildren<NuggetPickup>();
                    nugbuff.BuffTimer = CalcDuration();
                    nugbuff.RegenMult = regenBonus;
                    if (regenStacking)
                    {
                        nugbuff.RegenStacks = stack;
                    }

                    NetworkServer.Spawn(nugget);
                    //meat hit vfx here (bleed?)
                    //meat drop sound here (???)
                }
            }
            private float CalcDuration()
            {
                float stackDuration = MeatNugget.stackDuration * (stack - 1);
                return baseDuration + stackDuration;
            }
        }
    }
}
