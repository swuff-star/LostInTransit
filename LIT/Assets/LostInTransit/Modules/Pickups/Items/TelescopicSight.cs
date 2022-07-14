using Moonstorm;
using RoR2;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class TelescopicSight : ItemBase
    {
        private const string token = "LIT_ITEM_TELESCOPICSIGHT_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("TelescopicSight");

        public static string section;

        [ConfigurableField(ConfigDesc = "Base proc chance for Telescopic Sight.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseProcChance = 1f;

        [ConfigurableField(ConfigName = "Proc Chance per Stack", ConfigDesc = "Extra proc chance per stack of sights.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float procChancePerStack = 0.5f;

        [ConfigurableField(ConfigDesc = "Whether Telescopic Sight's instant kill should have a cooldown.")]
        public static bool enableCooldown = true;

        [ConfigurableField(ConfigDesc = "Cooldown between Telescopic Sight activations.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float cooldownDuration = 20f;

        [ConfigurableField(ConfigDesc = "Seconds removed from cooldown per stack.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float cooldownReductio = 2f;

        [ConfigurableField(ConfigDesc = "Percentage of max health that's dealt to set exceptions when activated on them.")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float exceptionHealthPercentage = 0.2f;

        [ConfigurableField( ConfigDesc = "Whether Telescopic Sight should instakill elites.")]
        public static bool instakillElites = true;

        [ConfigurableField( ConfigDesc = "Whether Telescopic Sight should instakill boss monsters.")]
        public static bool instakillBosses = false;


        public class TelescopicSightBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Thallium;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (damageInfo.dotIndex == DotController.DotIndex.None)
                {
                    if (Util.CheckRoll(CalcChance() * damageInfo.procCoefficient) && !body.HasBuff(LITContent.Buffs.TeleSightCD))
                    {
                        if (enableCooldown) 
                            body.AddCooldownBuff(LITContent.Buffs.TeleSightCD, CalcCooldown());

                        var flag = ChooseWetherToInstakill(victimHealthComponent.body);
                        if (flag)
                        {
                            damageInfo.damage = victimHealthComponent.body.maxHealth * 4;
                        }
                        else
                        {
                            damageInfo.damage = victimHealthComponent.body.maxHealth * exceptionHealthPercentage;
                        }
                        Util.PlaySound("TeleSightProc", body.gameObject);
                    }
                }
            }
            private float CalcChance()
            {
                float stackChance = procChancePerStack * (stack - 1);
                return baseProcChance + stackChance;
            }
            private float CalcCooldown()
            {
                //Yknow, we should NEVER reach a cooldown of 0, so this caps the cooldown at around 10 seconds.
                return cooldownDuration - ((1 - 1 / (1 + 0.25f * (stack - 1))) * 10);
                //Agreeable.
            }
            /*
             * This code dictates wether the body can be instakilled or not, based off the config and the CharacterBody.
             * By default, only normal enemies & elites should be instakillable, Bosses are treated as exceptions.
             */
            //Hey, English lesson: "whether". There's no need for the 'h', but it's there for... some reason.
            //I fucking hate english.
            private bool ChooseWetherToInstakill(CharacterBody body)
            {
                if (body.isChampion)
                {
                    return instakillBosses;
                }
                if (body.isElite)
                {
                    return instakillElites;
                }
                return true;
            }
        }
    }
}
