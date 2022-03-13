﻿using LostInTransit.Items;
using Moonstorm;
using RoR2;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    [DisabledContent]
    public class RepulsionArmorActive : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.Instance.MainAssetBundle.LoadAsset<BuffDef>("RepulsionArmorActive");

        public class RepulsionArmorActiveBehavior : BaseBuffBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation(useOnClient = false, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.RepulsionArmorActive;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                damageInfo.damage *= ((100f - RepulsionArmor.damageResist) * 0.01f);
            }
        }
    }
}