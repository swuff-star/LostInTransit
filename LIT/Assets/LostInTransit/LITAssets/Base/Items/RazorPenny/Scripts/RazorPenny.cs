using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;

namespace LostInTransit.Items
{
    public sealed class RazorPenny : LITItem
    {
        private const string TOKEN = "LIT_ITEM_RAZORPENNY_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra Crit added per penny.")]
        [FormatToken(TOKEN)]
        public static float pennyCrit = 4f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Gold gained on crit.")]
        [FormatToken(TOKEN, 1)]
        public static float goldPerCrit = 1f;

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<ItemDef>("RazorPenny", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }

        public class RazorPennyBehavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.RazorPenny;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += pennyCrit * stack;
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.damageInfo.crit == true)
                {
                    body.master.GiveMoney((uint)(stack * (Run.instance.stageClearCount + (goldPerCrit * 1f))));
                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, damageReport.victimBody.transform.position, UnityEngine.Vector3.up, true);
                }
            }
        }
    }
}
