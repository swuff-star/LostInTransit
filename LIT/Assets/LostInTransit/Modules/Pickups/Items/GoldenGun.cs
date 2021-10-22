﻿using RoR2;
using System;
using LostInTransit.Buffs;
using UnityEngine.Networking;
using Moonstorm;
using UnityEngine;

namespace LostInTransit.Items
{
    public class GoldenGun : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.LITAssets.LoadAsset<ItemDef>("GoldenGun");

        [ConfigurableField(ConfigName = "Maximum Gold Threshold", ConfigDesc = "The maximum amount of gold that Golden Gun will account for.")]
        public static uint goldCap = 600;

        [ConfigurableField(ConfigName = "Maximum Damage Bonus", ConfigDesc = "The maximum amount of bonus damage Golden Gun grants.")]
        public static uint goldNeeded = 40;

        /*
        public override void Config()
        {
            var section = $"Item: {ItemDef.name}";
            goldCap = LITMain.config.Bind<uint>(section, "Maximum Gold Threshold", 600, "The maximum amount of gold that Golden Gun will account for.").Value;
            goldNeeded = LITMain.config.Bind<uint>(section, "Maximum Damage Bonus", 40, "The maximum amount of added damage Golden Gun will give.").Value;
            //★ 'goldNeeded' is a really bad variable name for this. More accurate would be 'maxDamage' or something - since you SHOULD need 15 gold per buff, not 40.
            //★ Not changing it, but am bitching about it.
        }*/

        /*
        public override void DescriptionToken()
        {
            LITUtil.AddTokenToLanguage(ItemDef.descriptionToken,
                $"Deal <style=cIsDamage>extra damage</style> based on held <style=cIsUtility>gold</style>, up to an extra <style=cIsDamage>+{goldNeeded}% damage</style> <style=cStack>(+{goldNeeded/2}% per stack)</style> at <style=cIsUtility>{goldCap} gold</style> <style=cStack>(+{goldCap/2} per stack, scaling with time)</style>.",
                LangEnum.en);
        }*/
        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<GoldenGunBehavior>(stack);
        }

        public class GoldenGunBehavior : CharacterBody.ItemBehavior
        {
            public float gunCap = 0;
            private float goldForBuff = 0;
            private int buffsToGive = 0;

            private void Start()
            {
                body.onInventoryChanged += UpdateStacks;
                UpdateStacks();
            }

            private void UpdateStacks()
            {
                gunCap = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap));
                goldForBuff = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap)) / GetCap(goldNeeded); //★ This is the same stacking behavior so I will simply reuse it.
            }

            private float GetCap(uint value)
            {
                return value + (value / 2) * (stack - 1);
            }

            private void FixedUpdate()
            {
                if (body.master.money > 0)
                {
                    buffsToGive = (int)(Mathf.Min(body.master.money, gunCap) / goldForBuff);
                    /*if (buffsToGive > goldNeeded + (goldNeeded / 2) * (stack - 1))
                    {
                        buffsToGive = (int)(goldNeeded + (goldNeeded / 2) * (stack - 1));
                    }*/ //★ I'm very tired and struggling to read through how this works. I'm just fucking hardcoding a second cap check.
                    if (buffsToGive != body.GetBuffCount(GoldenGunBuff.buff))
                    {
                        body.SetBuffCount(GoldenGunBuff.buff.buffIndex, buffsToGive);
                    }
                }
                    


                /*//This all works, but the math is the tiniest bit inconsistent. This is due to the fact that 40 does not divide evenly into 700. Fuck Hopoo.
                //"Fixed" the above by buffing it to a 600 gold cap instead of 700.
                //Debug.Write("Current cap for Golden Gun: " + gunCap);
                if (body.master.money < 1) { return; }
                float goldPerBuff = (gunCap / 40f); //To-do: Change the 40f out for a configurable value. This is hard to explain in words how the math works in a short desc.
                //Debug.Write("Amount of gold needed for one Golden Gun buff: " + goldPerBuff);
                float buffToGive = (body.master.money / (int)goldPerBuff);
                Math.Floor(buffToGive);
                //Debug.WriteLine("Amount of earned Golden Gun buffs: " + buffToGive);
                float currentBuffs = body.GetBuffCount(GoldenGunBuff.buff);
                if (currentBuffs < (40f + ((40f * 0.5f) * (stack - 1))) && buffToGive > currentBuffs)
                    { body.AddBuff(GoldenGunBuff.buff); }
                if (buffToGive < currentBuffs)
                { body.RemoveBuff(GoldenGunBuff.buff); } 
                //body.SetBuffCount(GoldenGunBuff.buffGGIndex, (int)buffToGive); This seemed like a much less hacky implementation, but for some reason gave the Leeching buff rather than Golden Gun buff.
                //To-do: Find cleaner implementation?*/
            }
        }
    }
}
