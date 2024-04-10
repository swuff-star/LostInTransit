using MSU;
using RoR2;
using System;
using RoR2.Items;
using R2API;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class EnergyCell : LITItem
    {
        private const string TOKEN = "LIT_ITEM_ENERGYCELL_DESC";
        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Maximum amount of attack speed per item held.")]
        public static float maxAttackSpeedPerCell = 0.4f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
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
            var assetRequest = LITAssets.LoadAssetAsync<ItemDef>("EnergyCell", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _itemDef = assetRequest.Asset;
        }

        public class EnergyCellBehavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.EnergyCell;

            public float healthFraction;
            private HealthComponent _healthComponent;

            private void Start()
            {
                _healthComponent = body.healthComponent;
            }

            public void OnTakeDamageServer(DamageReport _)
            {
                body.MarkAllStatsDirty();
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseAttackSpeedAdd += body.baseAttackSpeed * (1 - healthFraction) * (float)Math.Pow(maxAttackSpeedPerCell, 1 / stack);
            }

            private void FixedUpdate()
            {
                float combinedHealthFraction = _healthComponent.combinedHealthFraction;
                healthFraction = combinedHealthFraction - 0.1f;
                if (combinedHealthFraction > 0.9f)
                {
                    healthFraction = 1;
                }
                else if (combinedHealthFraction < 0f)
                {
                    healthFraction = 0;
                }
            }
        }
    }
}
