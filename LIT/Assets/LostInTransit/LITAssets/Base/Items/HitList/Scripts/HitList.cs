using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;
using System.Linq;

namespace LostInTransit.Items
{
    public sealed class HitList : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_HITLIST_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base chance for enemies to spawn Marked.")]
        [FormatToken(TOKEN)]
        public static float markChance = 5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "% increase to base damage provided by buffs from this item.")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageBuffPower = 0.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Duration of the damage buff provided by Hit List.")]
        [FormatToken(TOKEN, 2)]
        public static float buffDuration = 20f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private AssetCollection _assetCollection;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += HandleDamageBuff;
            CharacterBody.onBodyStartGlobal += MarkHitList;
        }

        private void MarkHitList(CharacterBody body)
        {
            List<CharacterMaster> CharMasters(bool playersOnly = false)
            {
                return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
            }

            int hitListCount = 0;

            foreach (CharacterMaster chrm in CharMasters())
            {
                hitListCount += chrm?.inventory?.GetItemCount(LITContent.Items.HitList) ?? 0;
            }

            if (hitListCount == 0) return;


            if (Util.CheckRoll(Items.HitList.markChance + ((Items.HitList.markChance / 2) * (hitListCount - 1)), body.master))
            {
                body.SetBuffCount(LITContent.Buffs.bdHitListMarked.buffIndex, 1);
            }
        }

        private void HandleDamageBuff(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(LITContent.Buffs.bdHitListBuff);
            if (buffCount == 0)
                return;

            args.baseDamageAdd += sender.baseDamage * damageBuffPower * buffCount;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acHitList", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("HitList");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class HitListMarkedBehavior : BuffBehaviour, IOnKilledServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdHitListMarked;

            public void OnKilledServer(DamageReport damageReport)
            {
                if (!enabled)
                    return;

                if (damageReport.attackerBody)
                {
                    damageReport.attackerBody.AddTimedBuffAuthority(LITContent.Buffs.bdHitListBuff.buffIndex, buffDuration);
                }
            }
        }
    }
}