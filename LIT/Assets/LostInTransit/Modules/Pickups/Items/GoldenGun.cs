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
                UpdateStacks();
            }

            private void UpdateStacks()
            {
                gunCap = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap));
                Debug.Log(goldCap + " = gold cap");
                goldForBuff = Run.instance.GetDifficultyScaledCost((int)GetCap(goldCap)) / GetCap(goldNeeded); 
                Debug.Log(goldForBuff + " = gold per buff");
            }


            private float GetCap(uint value)
            {
                return value + ((value / 2) * (stack - 1));
            }

            public void OnDestroy()
            {
                body.SetBuffCount(LITContent.Buffs.GoldenGun.buffIndex, 0);
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += 0.01f * body.GetBuffCount(LITContent.Buffs.GoldenGun.buffIndex);
            }

            private void FixedUpdate()
            {
                if(NetworkServer.active)
                {
                    if (body.master.money > 0)
                    {
                        buffsToGive = (int)(Mathf.Min(body.master.money, gunCap) / goldForBuff);
                        if (buffsToGive != body.GetBuffCount(LITContent.Buffs.GoldenGun))
                        {
                            body.SetBuffCount(LITContent.Buffs.GoldenGun.buffIndex, buffsToGive);
                        }
                    }
                }
            }
        }
    }
}
