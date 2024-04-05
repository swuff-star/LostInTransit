using LostInTransit.Buffs;
using MSU;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class BlessedDice : LITItem
    {
        private const string token = "LIT_ITEM_BLESSEDDICE_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("BlessedDice", LITBundle.Items);

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Base duration of buff from Blessed Dice", ConfigDescOverride = "Base duration of buff after using a shrine.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float newBaseTimer = 10f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Duration of buff from Blessed Dice", ConfigDescOverride = "Added duration of buff per stack of Dice.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float newStackTimer = 5f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Barrier from buff", ConfigDescOverride = "Barrier/Temp HP gained while you have the shield buff, as a percentage of max health")]
        public static float barrierAmount = 50f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Barrier decay rate during buff", ConfigDescOverride = "Rate at which barrier decays while you have the Shield buff, as a percentage of normal decay rate.")]
        public static float decayMult = 0f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Armor from buff", ConfigDescOverride = "Armor added while you have the armor buff.")]
        public static float armorAmount = 50f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Move speed from buff", ConfigDescOverride = "Move speed added while you have the move speed buff, in percent.")]
        public static float moveAmount = 50f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Attack speed from buff", ConfigDescOverride = "Attack speed added while you have the attack speed buff, in percent.")]
        public static float atkAmount = 50f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Crit chance from buff", ConfigDescOverride = "Critical strike chance added while you have the critical strike buff, in percent.")]
        public static float critAmount = 20f;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Luck from buff", ConfigDescOverride = "Luck added while you have the luck buff. (Whole numbers only)")]
        public static int luckAmount = 1;

        [RiskOfOptionsConfigureField(LITConfig.items, ConfigNameOverride = "Weighted Rolls", ConfigDescOverride = "Make all buffs equally likely, instead of weighted for balance")]
        public static bool fairRolls = false;


        /// <summary>
        /// Todo: this needs to be probably on its own network behavior, mainly due to the usage of RNG, lol
        /// </summary>
        public class BlessedDiceBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.BlessedDice;

            private float CalcBuffTimer()
            {
                float stackTimer = newStackTimer * (stack - 1);
                return newBaseTimer + stackTimer;
            }
            
            private BuffDef ChooseRandomBuff()
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
                return buff;
            }
            
            private void AddBuffOnShrine(Interactor interactor, IInteractable interactable, GameObject interactableObject)
            {
                MonoBehaviour monoBehaviour = (MonoBehaviour)interactable;
                bool isPurchase = monoBehaviour.GetComponent<PurchaseInteraction>();
                //These have to be nested to prevent NREs when checking for isShrine on non-purchase events
                if (isPurchase)
                {
                    body.AddTimedBuff(ChooseRandomBuff(), CalcBuffTimer());
                    /*if (interactableObject.GetComponent<PurchaseInteraction>().isShrine)
                    {
                        
                    }*/
                }
            }

            

            public void Start()
            {
                GlobalEventManager.OnInteractionsGlobal += AddBuffOnShrine;
            }
            
            public void OnDestroy()
            {
                GlobalEventManager.OnInteractionsGlobal -= AddBuffOnShrine;
            }
        }
    }
}
