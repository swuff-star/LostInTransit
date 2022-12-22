using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class BlessedDice : ItemBase
    {
        private const string token = "LIT_ITEM_BLESSEDDICE_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("BlessedDice");

        [ConfigurableField(LITConfig.items, ConfigName = "Base duration of buff from Blessed Dice", ConfigDesc = "Base duration of buff after using a shrine.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float newBaseTimer = 10f;

        [ConfigurableField(LITConfig.items, ConfigName = "Duration of buff from Blessed Dice", ConfigDesc = "Added duration of buff per stack of Dice.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float newStackTimer = 5f;

        [ConfigurableField(LITConfig.items, ConfigName = "Barrier from buff", ConfigDesc = "Barrier/Temp HP gained while you have the shield buff, as a percentage of max health")]
        public static float barrierAmount = 50f;

        [ConfigurableField(LITConfig.items, ConfigName = "Barrier decay rate during buff", ConfigDesc = "Rate at which barrier decays while you have the Shield buff, as a percentage of normal decay rate.")]
        public static float decayMult = 0f;

        [ConfigurableField(LITConfig.items, ConfigName = "Armor from buff", ConfigDesc = "Armor added while you have the armor buff.")]
        public static float armorAmount = 50f;

        [ConfigurableField(LITConfig.items, ConfigName = "Move speed from buff", ConfigDesc = "Move speed added while you have the move speed buff, in percent.")]
        public static float moveAmount = 50f;

        [ConfigurableField(LITConfig.items, ConfigName = "Attack speed from buff", ConfigDesc = "Attack speed added while you have the attack speed buff, in percent.")]
        public static float atkAmount = 50f;

        [ConfigurableField(LITConfig.items, ConfigName = "Crit chance from buff", ConfigDesc = "Critical strike chance added while you have the critical strike buff, in percent.")]
        public static float critAmount = 20f;

        [ConfigurableField(LITConfig.items, ConfigName = "Luck from buff", ConfigDesc = "Luck added while you have the luck buff. (Whole numbers only)")]
        public static int luckAmount = 1;

        [ConfigurableField(LITConfig.items, ConfigName = "Weighted Rolls", ConfigDesc = "Make all buffs equally likely, instead of weighted for balance")]
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
                        buff = LITContent.Buffs.DiceLuck;
                        break;
                    case 2:
                    case 7:
                        buff = LITContent.Buffs.DiceCrit;
                        break;
                    case 3:
                    case 8:
                        buff = LITContent.Buffs.DiceAtk;
                        break;
                    case 4:
                    case 9:
                        buff = LITContent.Buffs.DiceMove;
                        break;
                    case 5:
                    case 10:
                        buff = LITContent.Buffs.DiceArmor;
                        break;
                    case 6:
                    case 11:
                        buff = LITContent.Buffs.DiceRegen;
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
