using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class FiremansBoots : ItemBase
    {
        private const string token = "LIT_ITEM_FIREMANSBOOTS_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("FireBoots", LITBundle.Items);

        [ConfigurableField(ConfigName = "Chance to Ignite", ConfigDesc = "Chance to Ignite on Hit.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float igniteChance = 8f;

        [ConfigurableField(ConfigName = "Ignite Damage Coefficient", ConfigDesc = "Damage coefficient of dealt ignite debuffs.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float igniteCoef = 2.4f;

        public class FiremansBootsBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(FiremansBootsBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.FireBoots;

            /*public void Start()
            {
                body.fireTrail.
                body.fireTrail = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/FireTrail"), this.transform).GetComponent<DamageTrail>();
                body.fireTrail.transform.position = body.footPosition;
                body.fireTrail.owner = body.gameObject;
                body.fireTrail.radius = body.radius;
                body.fireTrail.damagePerSecond = body.damage * 1.5f;
            }*/


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
                            damageMultiplier = igniteCoef
                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                }
            }
        }
    }
}
