
using MSU;
using RoR2;
using UnityEngine;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using R2API;
using System.Runtime.CompilerServices;

namespace LostInTransit.Items
{
    public sealed class Thallium : LITItem, IContentPackModifier
    {
        public const string TOKEN = "LIT_ITEM_THALLIUM_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Chance to afflict Thallium Poisoning.")]
        [FormatToken(TOKEN, 0)]
        public static float procChance = 10f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Total damage of Thallium, as a percentage of the victim's damage. Halved after the first stack")]
        [FormatToken(TOKEN, 1)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.DivideByN, 2, 2)]
        public static float totalDamage = 500f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "How much the victim is slowed by.")]
        [FormatToken(TOKEN, 3)]
        public static float slowMultiplier = 75f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of time needed to deal the full damage. By default, increases with stacks. Minimum 1.")]
        public static int poisonDuration = 4;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Poison is Fixed Duration", ConfigDescOverride = "If enabled, stacks increase the damage per tick instead of the total duration")]
        public static bool noTimeToDie = false;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private static DotBuffDef _dotBuffDef;
        public static DotController.DotIndex ThalliumPoison => _dotBuffDef.DotIndex;

        public override void Initialize()
        {
            _dotBuffDef.Init();

            _dotBuffDef.DotDef.damageColorIndex = DamageColorIndex.DeathMark;
            R2API.RecalculateStatsAPI.GetStatCoefficients += HandleSlow;
        }

        private void HandleSlow(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(LITContent.Buffs.dbdThalliumPoison))
                args.moveSpeedReductionMultAdd += slowMultiplier;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acThallium", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            var collection = request.Asset;

            _itemDef = collection.FindAsset<ItemDef>("Thallium");
            _dotBuffDef = collection.FindAsset<DotBuffDef>("dbdThalliumPoison");
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_dotBuffDef);
        }

        public class ThalliumBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = false)]
            public static ItemDef GetItemDef() => LITContent.Items.Thallium;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                var attacker = damageReport.attacker;
                var victim = damageReport.victim;
                var dotController = DotController.FindDotController(victim.gameObject);
                bool flag = false;
                if (dotController)
                    flag = dotController.HasDotActive(ThalliumPoison);

                if (Util.CheckRoll(procChance * damageReport.damageInfo.procCoefficient) && !flag)
                {
                    float newDuration = Mathf.Max(poisonDuration, 1f);
                    float newDamage = (totalDamage / 100) * (1 + ((stack - 1) / 2));
                    if (!noTimeToDie)
                        newDuration += (stack - 1) * 2;
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker.gameObject,
                        victimObject = victim.gameObject,
                        dotIndex = ThalliumPoison,
                        duration = newDuration,
                        //G - dividing by attacker damage = 1, then multiply by victim damage for corrected damage
                        damageMultiplier = (damageReport.victimBody.damage / damageReport.attackerBody.damage) * (newDamage / newDuration)
                    };
                    DotController.InflictDot(ref dotInfo);
                    Util.PlaySound("ThalliumProc", body.gameObject);
                }
            }
        }
    }
}
