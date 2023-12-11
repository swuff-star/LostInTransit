using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using R2API;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class ArmsRaceDroneMods : ItemBase
    {
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("ArmsRaceDroneModifiers", LITBundle.Items);

        public class ArmsRaceDroneModsBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.ArmsRaceDroneMods;

            public void Awake()
            {
                base.Awake();
                body.RecalculateStats();
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (body.healthComponent.shield >= 1f && damageInfo.damage >= body.healthComponent.shield + body.healthComponent.barrier)
                {
                    if (ArmsRace.shieldGating == true && !(damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection))
                    {
                        damageInfo.damage = body.healthComponent.shield + body.healthComponent.barrier;
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseShieldAdd += (body.maxHealth * (0.01f * ArmsRace.extraShield * stack));
            }
        }
    }
}
