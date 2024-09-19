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
using System.Collections.Generic;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public sealed class MeatNugget : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_MEATNUGGET_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Proc chance for Meat Nugget.")]
        [FormatToken(TOKEN, 0)]
        public static float procChance = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Amount added to regen by nugget pickup.")]
        [FormatToken(TOKEN, 1)]
        public static float regenBonus = 1.6f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "If true, the regen buff duration can stack up to the number of Meat Nuggets you have.")]
        public static bool regenStacking = true;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Base duration of the regen buff granted by dropped nuggets.")]
        [FormatToken(TOKEN, 2)]
        public static float baseDuration = 2;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Extra duration of the regen buff per stack of Meat Nugget.")]
        [FormatToken(TOKEN, 3)]
        public static float stackDuration = 1;

        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;
        public override ItemDef itemDef => _itemDef;
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
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acMeatNugget", LITBundle.Items);

            request.StartLoad();

            while (!request.IsComplete)
                yield return null;

            var collection = request.Asset;

            _itemDef = collection.FindAsset<ItemDef>("MeatNugget");
            _meatNuggetPickup = collection.FindAsset<GameObject>("MeatNuggetPickup");
            _nuggetRegen = collection.FindAsset<BuffDef>("bdNuggetRegen");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.networkedObjectPrefabs.AddSingle(_meatNuggetPickup);
            contentPack.buffDefs.AddSingle(_nuggetRegen);
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
