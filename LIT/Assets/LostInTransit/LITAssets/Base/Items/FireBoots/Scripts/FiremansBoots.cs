using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;

namespace LostInTransit.Items
{
#if DEBUG
    public sealed class FiremansBoots : LITItem
    {
        private const string TOKEN = "LIT_ITEM_FIREMANSBOOTS_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Chance to Ignite on Hit.")]
        [FormatToken(TOKEN)]
        public static float igniteChance = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Damage coefficient of dealt ignite debuffs.")]
        [FormatToken(TOKEN, 1)]
        public static float igniteDamageCoefficient = 2.4f;

        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;
        public override ItemDef itemDef => _itemDef;
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
            var assetRequest = LITAssets.LoadAssetAsync<ItemDef>("FireBoots", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _itemDef = assetRequest.Asset;
            yield break;
        }

        public class FiremansBootsBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(FiremansBootsBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.FireBoots;

            public void Start()
            {
                body.fireTrail = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/FireTrail"), this.transform).GetComponent<DamageTrail>();
                body.fireTrail.transform.position = body.footPosition;
                body.fireTrail.owner = body.gameObject;
                body.fireTrail.radius = body.radius;
                body.fireTrail.damagePerSecond = body.damage * 1.5f;
            }


            public void OnDestroy()
            {
                body.fireTrail.active = false;
                UnityEngine.Object.Destroy(body.fireTrail.gameObject);
                body.fireTrail = null;
            }

            

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (Util.CheckRoll(igniteChance, body.master))
                {
                    if (damageReport.damageInfo.procCoefficient > 0)
                    {
                        var dotInfo = new InflictDotInfo()
                        {
                            attackerObject = body.gameObject,
                            victimObject = damageReport.victim.gameObject,
                            dotIndex = DotController.DotIndex.Burn,
                            duration = damageReport.damageInfo.procCoefficient * 4f,
                            damageMultiplier = igniteDamageCoefficient
                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                }
            }
        }
    }
#endif
}
