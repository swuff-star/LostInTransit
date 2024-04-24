
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Items
{
#if DEBUG
    public class BlessedDice : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_BLESSEDDICE_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base duration of buff after using a shrine.")]
        [FormatToken(TOKEN, 0)]
        public static float baseBuffDuration = 10f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Added duration of buff per stack of Dice.")]
        [FormatToken(TOKEN, 1)]
        public static float buffStackDuration = 5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Barrier/Temp HP gained while you have the shield buff, as a percentage of max health")]
        public static float barrierAmount = 50f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Barrier decay rate during buff", ConfigDescOverride = "Rate at which barrier decays while you have the Shield buff, as a percentage of normal decay rate.")]
        public static float barrierDecayRate = 0f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS,  ConfigDescOverride = "Armor added while you have the armor buff.")]
        public static float armorBonus = 50f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Move speed added while you have the move speed buff, in percent.")]
        public static float movementSpeedBonus = 50f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Attack speed added while you have the attack speed buff, in percent.")]
        public static float attackBonus = 50f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Critical strike chance added while you have the critical strike buff, in percent.")]
        public static float criticalChanceBonus = 20f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Luck added while you have the luck buff. (Whole numbers only)")]
        public static uint luckAmountBonus = 1;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Weighted Rolls", ConfigDescOverride = "Make all buffs equally likely, instead of weighted for balance")]
        public static bool fairRolls = false;

        private AssetCollection _assetCollection;
        public override void Initialize()
        {
            GlobalEventManager.OnInteractionsGlobal += GiveDiceBuff;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterMaster.OnInventoryChanged += IncreaseLuck;
        }

        private void IncreaseLuck(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            var body = self.GetBody();
            if (!body)
                return;

            self.luck += luckAmountBonus * body.GetBuffCount(LITContent.Buffs.bdDiceLuck);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int armorCount = sender.GetBuffCount(LITContent.Buffs.bdDiceArmor);
            int attackCount = sender.GetBuffCount(LITContent.Buffs.bdDiceAtk);
            int moveCount = sender.GetBuffCount(LITContent.Buffs.bdDiceMove);
            int critCount = sender.GetBuffCount(LITContent.Buffs.bdDiceCrit);

            args.armorAdd += armorBonus * armorCount;
            args.attackSpeedMultAdd += (attackBonus / 100) * attackCount;
            args.moveSpeedMultAdd += (movementSpeedBonus / 100) * moveCount;
            args.critAdd += criticalChanceBonus * critCount;
        }

        private void GiveDiceBuff(Interactor arg1, IInteractable arg2, GameObject arg3)
        {
            if (!NetworkServer.active)
                return;

            if (!MSUtil.IsInteractableValidForSpawns(arg3))
                return;

            if (!arg1.TryGetComponent<CharacterBody>(out var body))
                return;

            var itemCount = body.GetItemCount(_itemDef);
            if (itemCount == 0)
                return;

            AddBuffToBody(body, itemCount);
        }

        private void AddBuffToBody(CharacterBody body, int itemCount)
        {
            int weight = 7;
            if (!fairRolls)
            {
                weight += 5;
            }
            BuffDef buff = null;
            int rng = Run.instance.runRNG.RangeInt(1, weight);
            switch (rng)
            {
                case 1:
                    buff = LITContent.Buffs.bdDiceLuck;
                    break;
                case 2:
                case 7:
                    buff = LITContent.Buffs.bdDiceCrit;
                    break;
                case 3:
                case 8:
                    buff = LITContent.Buffs.bdDiceAtk;
                    break;
                case 4:
                case 9:
                    buff = LITContent.Buffs.bdDiceMove;
                    break;
                case 5:
                case 10:
                    buff = LITContent.Buffs.bdDiceArmor;
                    break;
                case 6:
                case 11:
                    buff = LITContent.Buffs.bdDiceRegen;
                    break;
            }

            float duration = baseBuffDuration + (buffStackDuration * itemCount);
            body.AddTimedBuff(buff, duration);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "BlessedDice" - Items
             * BuffDef - "bdDiceArmor" - Items
             * BuffDef - "bdDiceAtk" - Items
             * BuffDef - "bdDiceMove" - Items
             * BuffDef - "bdDiceLuck" - Items
             * BuffDef - "bdDiceCrit" - Items
             */
            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acBlessedDice", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("BlessedDice");
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }
    }
#endif
}
