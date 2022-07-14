using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using R2API;

namespace LostInTransit.Items
{
    public class GoldenGun : ItemBase
    {
        private const string token = "LIT_ITEM_GOLDENGUN_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("GoldenGun");

        [ConfigurableField(ConfigName = "Maximum Gold Threshold", ConfigDesc = "The maximum amount of gold that Golden Gun will account for.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        [TokenModifier(token, StatTypes.DivideBy2, 3)]
        public static uint goldCap = 300;

        [ConfigurableField(ConfigName = "Maximum Damage Bonus", ConfigDesc = "The maximum amount of bonus damage Golden Gun grants.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier(token, StatTypes.DivideBy2, 1)]
        public static uint goldNeeded = 40;


        public class GoldenGunBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.GoldenGun;

            public float gunCap = 0;
            private float goldForBuff = 0;
            private int buffsToGive = 0;

            private void Start()
            {
                Stage.onStageStartGlobal += UpdateStacksOnStageStart;
                UpdateStacks();
            }

            private void UpdateStacks()
            {
                gunCap = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap));
                goldForBuff = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap)) / GetCap(goldNeeded); //★ This is the same stacking behavior so I will simply reuse it.
            }

            private void UpdateStacksOnStageStart(Stage obj)
            {
                UpdateStacks();
            }

            private float GetCap(uint value)
            {
                return value + (value / 2) * (stack - 1);
            }

            public void OnDestroy()
            {
                Stage.onStageStartGlobal -= UpdateStacksOnStageStart;
                body.SetBuffCount(LITContent.Buffs.GoldenGun.buffIndex, 0);
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += 0.01f * buffsToGive;
            }

            private void FixedUpdate()
            {
                if(NetworkServer.active)
                {
                    if (body.master.money > 0)
                    {
                        buffsToGive = (int)(Mathf.Min(body.master.money, gunCap) / goldForBuff);
                        /*if (buffsToGive > goldNeeded + (goldNeeded / 2) * (stack - 1))
                        {
                            buffsToGive = (int)(goldNeeded + (goldNeeded / 2) * (stack - 1));
                        }*/ //★ I'm very tired and struggling to read through how this works. I'm just fucking hardcoding a second cap check.
                        if (buffsToGive != body.GetBuffCount(LITContent.Buffs.GoldenGun))
                        {
                            body.SetBuffCount(LITContent.Buffs.GoldenGun.buffIndex, buffsToGive);
                        }
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
