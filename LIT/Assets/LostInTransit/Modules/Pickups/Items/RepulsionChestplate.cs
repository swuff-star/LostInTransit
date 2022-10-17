using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class RepulsionArmor : ItemBase
    {
        private const string token = "LIT_ITEM_REPULCHEST_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("Chestplate");

        [ConfigurableField(ConfigName = "Hits Needed to Activate", ConfigDesc = "Amount of times required to take damage before activating Repulsion Armor.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float hitsNeededConfig = 6f;

        [ConfigurableField(ConfigName = "Hits Needed per Stack", ConfigDesc = "Amount of extra hits needed per stack to activate Repulsion Armor.")] //This kinda sucks but is easy to include if anyone wanted it for some god-forsaken reason.
        public static float hitsNeededConfigStack = 0f;

        [ConfigurableField(ConfigName = "Base Duration of Buff", ConfigDesc = "Amount of time the Repulsion Armor buff lasts.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buffBaseLength = 3f;

        [ConfigurableField(ConfigName = "Stacking Duration of Buff", ConfigDesc = "Extra aount of time added to the Repulsion Armor buff per stack.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float buffStackLength = 1.5f;

        [ConfigurableField(ConfigName = "Maximum Duration", ConfigDesc = "Maximum length of the Repulsion Armor buff. Set to 0 to disable.")]
        public static float durCap = 0f;

        [ConfigurableField(ConfigName = "Damage Reduction", ConfigDesc = "Amount of armor added while the Repulsion Armor buff is active.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float damageResist = 500f;

        public class RepulsionArmorBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Chestplate;

            /*public float hitsNeededToActivate; //it's a mouthful but I am very high and I will easily be able to remember what it is for like this.
            private float stopwatch;
            private static float checkTimer = 0.25f;*/

            public void Start()
            {
                //body.SetBuffCount(LITContent.Buffs.RepulsionArmorCD.buffIndex, (int)hitsNeededConfig);
            }
            public void LateStart()
            {
                if ((body.GetBuffCount(LITContent.Buffs.RepulsionArmorActive) == 0) && (body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD) == 0))
                {
                    //Debug.Log("YOU HAVE NOTHING");
                    body.AddTimedBuffAuthority(LITContent.Buffs.RepulsionArmorActive.buffIndex, (buffBaseLength + buffStackLength * (stack - 1)) / 2f);
                    //body.SetBuffCount(LITContent.Buffs.RepulsionArmorActive.buffIndex, (int)hitsNeededConfig);
                }
                /*stopwatch = 0f;
                hitsNeededToActivate = hitsNeededConfig + (hitsNeededConfigStack * (stack - 1));
                if (hitsNeededToActivate < 1)
                { hitsNeededToActivate = 1; } //Failsafe for if someone tries to set this shit to 0.
                if(NetworkServer.active)
                    body.AddBuff(LITContent.Buffs.RepulsionArmorActive);*/
                //if (body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD) == 0 && body.GetBuffCount(LITContent.Buffs.RepulsionArmorActive) == 0) body.SetBuffCount(LITContent.Buffs.RepulsionArmorCD.buffIndex, (int)(hitsNeededConfig + hitsNeededConfigStack));
            }
            /*private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > checkTimer)
                    {
                        //n- I hate this syntax
                        stopwatch -= checkTimer;
                        //System.Diagnostics.Debug.WriteLine("It's been " + checkTimer + "!");
                        float currentCDBuffs = body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD);
                        float repulCount = (buffBaseLength + (buffStackLength * (stack - 1)));
                        if (repulCount > durCap && durCap != 0f)
                        { repulCount = durCap; }
                        if (currentCDBuffs < hitsNeededToActivate)
                        { body.AddBuff(LITContent.Buffs.RepulsionArmorCD); } //Maybe use a bool alongside this to interact proper w/ Blast Shower? Might not be needed.
                        if (currentCDBuffs > hitsNeededToActivate && currentCDBuffs > 0) //There simply MUST be a better way to do this.
                        { body.RemoveBuff(LITContent.Buffs.RepulsionArmorCD); }
                        if (currentCDBuffs == 0)
                        {
                            body.AddTimedBuff(LITContent.Buffs.RepulsionArmorActive, repulCount);
                            //hitsNeededToActivate = hitsNeededConfig; //I'm honestly not the happiest about this, but it gets the job done.
                            stopwatch -= repulCount + 0.05f;
                            //This SHOULD be a sneaky way to add a buffer to the cooldown. Could do this better by actually checking # of buffs. To-do: That, later.
                        }
                        if (body.HasBuff(LITContent.Buffs.RepulsionArmorActive))
                        { hitsNeededToActivate = hitsNeededConfig; } //...This isn't exactly the result I'm looking for with the above, but who am I to complain about working code? Should probably make a new timer for it.
                    }
                } //That's a lotta if statements. Cleaner implementation probably possible but not worth pursuing at this time.
            }*/

            /*private void FixedUpdate()
            {
                
            }*/

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker == body) return;
                if ((body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD) <= 1) && (!body.HasBuff(LITContent.Buffs.RepulsionArmorActive)))
                {
                    if (body.HasBuff(LITContent.Buffs.RepulsionArmorCD)) body.RemoveBuff(LITContent.Buffs.RepulsionArmorCD);

                    body.AddTimedBuffAuthority(LITContent.Buffs.RepulsionArmorActive.buffIndex, (buffBaseLength + buffStackLength * (stack - 1)));
                }
                if (body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD) > 1) body.RemoveBuff(LITContent.Buffs.RepulsionArmorCD);
            }
        }
    }
}
