﻿using RoR2;
using UnityEngine.Networking;
using Moonstorm;
using System;
using UnityEngine;

namespace LostInTransit.Items
{
    public class LifeSavings : LITItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.LITAssets.LoadAsset<ItemDef>("LifeSavings");
        public static ItemDef itemDef;
        public static float newMoneyKeptBase;
        public static float newMoneyKeptStack;

        //★ (godzilla 1998 main character voice)
        //★ that's a lotta Debug.WriteLine()
        //Neb - Why dont use Debug.Log(), lol | Doesnt matter, i killed this code and rewrote it as my child.
        public override void Initialize()
        {
            Config();
            DescriptionToken();
            itemDef = ItemDef;
        }

        public override void Config()
        {
            var section = $"Item: {ItemDef.name}";
            newMoneyKeptBase = LITMain.config.Bind<float>(section, "Money Kept", 5f, "Percentage of money kept between stages.").Value;
            newMoneyKeptStack = LITMain.config.Bind<float>(section, "Money Kept per Stack", 2.5f, "Amount of kept money added for each stack of Life Savings").Value;
        }

        public override void DescriptionToken()
        {
            LITUtil.AddTokenToLanguage(ItemDef.descriptionToken,
                $"Keep <style=cIsUtility>{newMoneyKeptBase}%</style> <style=cStack>(+{newMoneyKeptStack}% per stack)</style> of <style=cIsUtility>earned gold</style> between stages. Gold is not kept when travelling between <style=cWorldEvent>Hidden Realms</style>.",
                LangEnum.en);
        }
        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<LifeSavingsBehavior>(stack);
        }

        public class LifeSavingsBehavior : CharacterBody.ItemBehavior
        {
            public LifeSavingsMasterBehavior MasterBehavior
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
                if(body.isPlayerControlled)
                    MasterBehavior.UpdateStacks();
            }

            private void OnDestroy()
            {
                if(body.isPlayerControlled)
                    MasterBehavior.CheckIfShouldDestroy();
            }
        }

        //This is one of the rare few cases where an item behavior is not enough.
        public class LifeSavingsMasterBehavior : MonoBehaviour
        {
            public CharacterMaster CharMaster { get => gameObject.GetComponent<CharacterMaster>(); }
            public int stack;
            public bool moneyPending;
            public uint storedGold;

            public void Start()
            {
                Debug.LogError("Start");
                CharMaster.inventory.onInventoryChanged += UpdateStacks;
                SceneExitController.onBeginExit += ExtractMoney;
                Stage.onStageStartGlobal += GiveMoney;
                UpdateStacks();
            }

            internal void UpdateStacks()
            {
                Debug.LogError("Updating Stacks");
                stack = (int)CharMaster?.inventory.GetItemCount(itemDef);
            }

            private void ExtractMoney(SceneExitController obj)
            {
                Debug.LogError("Attempting to extract money");
                if((bool)!Run.instance?.isRunStopwatchPaused)
                {
                    Debug.LogError("Not hidden realm, extracting money...");
                    moneyPending = true;
                    storedGold = CalculatePercentage();
                }
            }

            private uint CalculatePercentage()
            {
                Debug.LogError("Calculating Percentage");
                var percentage = newMoneyKeptBase + (newMoneyKeptStack * (stack - 1));

                Debug.Log($"Percentage to Extract: {percentage}");
                Debug.Log($"Money that master has: {CharMaster.money}");
                uint toReturn;
                toReturn = (uint)(CharMaster.money / 100 * Mathf.Min(percentage, 100));
                Debug.Log($"Amount of money to extract: {toReturn}");
                CharMaster.money -= toReturn;
                return toReturn;
            }
            private void GiveMoney(Stage obj)
            {
                Debug.LogError("Giving Money");
                CharMaster.GiveMoney(storedGold);
                storedGold = 0;
                moneyPending = false;
            }

            public void CheckIfShouldDestroy()
            {
                Debug.LogError("Check if should destroy");
                if (!moneyPending)
                    Destroy(this);
            }

            public void OnDestroy()
            {
                Debug.LogError("Destroyed");
                SceneExitController.onBeginExit -= ExtractMoney;
                Stage.onStageStartGlobal -= GiveMoney;
                CharMaster.inventory.onInventoryChanged -= UpdateStacks;
            }
        }
    }
}
