using Moonstorm;
using R2API;
using RoR2;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LostInTransit.Items
{
    public class CoolantCell : VoidItemBase
    {
        public override ItemDef ItemDef => LITAssets.LoadAsset<ItemDef>("CoolantCell", LITBundle.Items);

        public static float baseValue = 0.2f;
        public static float stackingValue = 0.4f;
        public static float maxValue = 0.75f;

        public override IEnumerable<ItemDef> LoadItemsToInfect()
        {
            return new List<ItemDef>
            {
                LITAssets.LoadAsset<ItemDef>("EnergyCell", LITBundle.Items)
            };
        }

        public class CoolantCellBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => LITContent.Items.CoolantCell;

            private float _healthFraction;
            private HealthComponent _healthComponent;

            private void Start()
            {
                _healthComponent = body.healthComponent;
            }
            private void FixedUpdate()
            {
                float combinedHealthFraction = _healthComponent.combinedHealthFraction;
                _healthFraction = combinedHealthFraction - 0.1f;
                if (combinedHealthFraction > 0.9f)
                {
                    _healthFraction = 1;
                }
                else if (combinedHealthFraction < 0f)
                {
                    _healthFraction = 0;
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                body.MarkAllStatsDirty();
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                var value = MSUtil.InverseHyperbolicScaling(baseValue, stackingValue, maxValue, stack);
                args.cooldownMultAdd -= Util.Remap(1 - _healthFraction, 0, 1, 0, value);
            }
        }
    }
}