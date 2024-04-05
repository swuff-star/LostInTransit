using LostInTransit.Buffs;
using MSU;
using RoR2;
using UnityEngine;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class Thallium : LITItem
    {
        public const string token = "LIT_ITEM_THALLIUM_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("Thallium", LITBundle.Items);

        [RiskOfOptionsConfigureField(ConfigDescOverride = "Chance to afflict Thallium Poisoning.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float procChance = 10f;

        [RiskOfOptionsConfigureField(ConfigDescOverride = "Total damage of Thallium, as a percentage of the victim's damage. Halved after the first stack")]
        [TokenModifier(token, StatTypes.Default, 1)]
        [TokenModifier(token, StatTypes.DivideByN, 2, 2)]
        public static float totalDamage = 500f;

        [RiskOfOptionsConfigureField(ConfigDescOverride = "How much the victim is slowed by.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float slowMultiplier = 75f;

        [RiskOfOptionsConfigureField(ConfigDescOverride = "Amount of time needed to deal the full damage. By default, increases with stacks. Minimum 1.")]
        public static int poisonDuration = 4;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Poison is Fixed Duration", ConfigDescOverride = "If enabled, stacks increase the damage per tick instead of the total duration")]
        public static bool noTimeToDie = false;

        public class ThalliumBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = false)]
            public static ItemDef GetItemDef() => LITContent.Items.Thallium;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                var attacker = damageReport.attacker;
                var victim = damageReport.victim;
                var dotController = DotController.FindDotController(victim.gameObject);
                bool flag = false;
                if (dotController)
                    flag = dotController.HasDotActive(ThalliumPoison.index);

                if (Util.CheckRoll(procChance * damageReport.damageInfo.procCoefficient) && !flag)
                {
                    float newDuration = Mathf.Max(poisonDuration, 1f);
                    float newDamage = (totalDamage / 100) * (1 + ((stack - 1) / 2));
                    if (!noTimeToDie)
                        newDuration += (stack - 1) * 2;
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker.gameObject,
                        victimObject = victim.gameObject,
                        dotIndex = ThalliumPoison.index,
                        duration = newDuration,
                        //G - dividing by attacker damage = 1, then multiply by victim damage for corrected damage
                        damageMultiplier = (damageReport.victimBody.damage / damageReport.attackerBody.damage) * (newDamage / newDuration)
                    };
                    DotController.InflictDot(ref dotInfo);
                    Util.PlaySound("ThalliumProc", body.gameObject);
                }
            }
        }
    }
}
