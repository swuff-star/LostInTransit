using R2API;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm;
using RoR2;
using LostInTransit.Components;
using RoR2.Items;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class MeatNugget : ItemBase
    {
        private const string token = "LIT_ITEM_MEATNUGGET_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("MeatNugget", LITBundle.Items);

        public static GameObject MeatNuggetPickup = LITAssets.LoadAsset<GameObject>("MeatNuggetPickup", LITBundle.Items);

        [ConfigurableField(ConfigName = "Proc Chance", ConfigDesc = "Proc chance for Meat Nugget.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float procChance = 8f;

        [ConfigurableField(ConfigName = "Regen Additive", ConfigDesc = "Amount added to regen by nugget pickup.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float regenAdded = 1.6f;

        [ConfigurableField(ConfigName = "Does Regen Stack", ConfigDesc = "If true, the regen buff duration can stack up to the number of Meat Nuggets you have.")]
        public static bool doesStack = true;

        [ConfigurableField(ConfigName = "Duration", ConfigDesc = "Base duration of the regen buff granted by dropped nuggets.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float newBaseDuration = 2;

        [ConfigurableField(ConfigName = "Stacking Duration", ConfigDesc = "Extra duration of the regen buff per stack of Meat Nugget.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float newStackDuration = 1;

        public class MeatNuggetBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.MeatNugget;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                GameObject victim = damageReport.victim.gameObject;
                if (Util.CheckRoll(procChance * damageReport.damageInfo.procCoefficient, damageReport.attackerMaster))
                {
                    GameObject nugget = UnityEngine.Object.Instantiate<GameObject>(MeatNuggetPickup, victim.transform.position, UnityEngine.Random.rotation);
                    nugget.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
                    NuggetPickup nugbuff = nugget.GetComponentInChildren<NuggetPickup>();
                    nugbuff.BuffTimer = CalcDuration();
                    nugbuff.RegenMult = regenAdded;
                    if (doesStack)
                    {
                        nugbuff.RegenStacks = stack;
                    }

                    NetworkServer.Spawn(nugget);
                    //meat hit vfx here (bleed?)
                    //meat drop sound here (???)
                }
            }
            private float CalcDuration()
            {
                float stackDuration = newStackDuration * (stack - 1);
                return newBaseDuration + stackDuration;
            }
        }
    }
}
