using Moonstorm;
using RoR2;
using UnityEngine;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class LifeSavings : ItemBase
    {
        private const string token = "LIT_ITEM_LIFESAVINGS_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("LifeSavings");
        public static ItemDef itemDef;

        /*[ConfigurableField(ConfigName = "Money Kept Between Stages", ConfigDesc = "Percentage of money kept between stages")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier(token, StatTypes.DivideBy2, 1)]
        public static float newMoneyKeptBase = 5f;*/

        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += GiveMoney;
        }

        private void GiveMoney(CharacterBody obj)
        {
            if (!obj.inventory)
                return;
            var inv = obj.inventory;
            int count = inv.GetItemCount(ItemDef);
            obj.master.GiveMoney((uint)(Run.instance.GetDifficultyScaledCost(25 * count)));
            Debug.Log("Giving " + Run.instance.GetDifficultyScaledCost(25 * count) + " money");
        }

        //★ sorry
        /*public class LifeSavingsBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.LifeSavings;

            public void Start()
            {
                Stage.onStageStartGlobal += GiveMoney;
            }

            private void GiveMoney(Stage obj)
            {
                body.master.GiveMoney((uint)(Run.instance.GetDifficultyScaledCost(25 * stack)));
            }

            public void OnDestroy()
            {
                Stage.onStageStartGlobal -= GiveMoney;
            }

            
            /*public LifeSavingsMasterBehavior MasterBehavior
            {
                get
                {
                    if (!_masterBehavior)
                    {
                        var component = body.master?.GetComponent<LifeSavingsMasterBehavior>();
                        if (component)
                        {
                            _masterBehavior = component;
                            return _masterBehavior;
                        }
                        else if (body.master?.playerCharacterMasterController != null)
                        {
                            _masterBehavior = body.master?.gameObject.AddComponent<LifeSavingsMasterBehavior>();
                            return _masterBehavior;
                        }
                        return _masterBehavior;
                    }
                    else
                        return _masterBehavior;
                }
            }

            private LifeSavingsMasterBehavior _masterBehavior;

            public void Start()
            {
                //Only add master behaviors to players.
                if (body.isPlayerControlled)
                    MasterBehavior.UpdateStacks();
            }

            private void OnDestroy()
            {
                if (body.isPlayerControlled)
                    MasterBehavior.CheckIfShouldDestroy();
            }*/
    }

    //This is one of the rare few cases where an item behavior is not enough.
    /*public class LifeSavingsMasterBehavior : MonoBehaviour
    {
        public CharacterMaster CharMaster { get => gameObject.GetComponent<CharacterMaster>(); }
        public int stack;
        public bool moneyPending;
        public uint storedGold;

        public void Start()
        {
            CharMaster.inventory.onInventoryChanged += UpdateStacks;
            //SceneExitController.onBeginExit += ExtractMoney;
            Stage.onStageStartGlobal += GiveMoney;
            UpdateStacks();
        }

        internal void UpdateStacks()
        {
            stack = (int)CharMaster?.inventory.GetItemCount(itemDef);
        }

        private void ExtractMoney(SceneExitController obj)
        {
            if ((bool)!Run.instance?.isRunStopwatchPaused)
            {
                moneyPending = true;
                storedGold = CalculatePercentage();
            }
        }

        private uint CalculatePercentage()
        {
            var percentage = newMoneyKeptBase + (newMoneyKeptBase/2 * (stack - 1));

            uint toReturn;
            toReturn = (uint)(CharMaster.money / 100 * Mathf.Min(percentage, 100));
            CharMaster.money -= toReturn;
            return toReturn;
        }

        //★ *takes comically large sip out of big empty soda cup creating al oud SLURRRRRRRRRRRRP sound* :grimacing: 
        private void GiveMoney(Stage obj)
        {
            CharMaster.GiveMoney(((uint)Run.instance.GetDifficultyScaledCost(25)));
            CharMaster.GiveMoney(storedGold);
            storedGold = 0;
            moneyPending = false;
        }

        public void CheckIfShouldDestroy()
        {
            if (!moneyPending)
                Destroy(this);
        }

        public void OnDestroy()
        {
            //SceneExitController.onBeginExit -= ExtractMoney;
            Stage.onStageStartGlobal -= GiveMoney;
            CharMaster.inventory.onInventoryChanged -= UpdateStacks;
        }
    }
}*/
}
