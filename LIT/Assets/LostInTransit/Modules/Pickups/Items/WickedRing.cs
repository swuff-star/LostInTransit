using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using System;
using UnityEngine;

namespace LostInTransit.Items
{
    public class WickedRing : ItemBase
    {
        private const string token = "LIT_ITEM_WICKEDRING_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("WickedRingNew", LITBundle.Items);

        [ConfigurableField(ConfigDesc = "Seconds removed from skill cooldowns on kill.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float secondsRemovedPerKill = 1f;


        public class WickedRingBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.WickedRingNew;

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if (body.skillLocator) 
                    body.skillLocator.DeductCooldownFromAllSkillsServer(secondsRemovedPerKill * stack);
            }
        }
    }
}
