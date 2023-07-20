using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class RustyJetpack : ItemBase
    {
        private const string token = "LIT_ITEM_RUSTYJETPACK_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("RustyJetpack", LITBundle.Items);

        [ConfigurableField(ConfigName = "Jump Power", ConfigDesc = "Added jump power per Jetpack, as a percentage of normal jump power. Halved after the first stack.")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float addedJumpPower = 2f;

        [ConfigurableField(ConfigName = "Fall Speed Reduction", ConfigDesc = "Amount of gravity removed, as a pecent")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float reducedGravity = 0.35f;

        [ConfigurableField(ConfigName = "Fall Speed Limit", ConfigDesc = "Maximum amount fall speed can be reduced by, in percent")]
        public static float minGrav = 90f;

        public class RustyJetpackBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => LITContent.Items.RustyJetpack;
            public void RecalculateStatsEnd()
            {
                //body.jumpPower += addedJumpPower + ((addedJumpPower / 2) * (stack - 1));
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.jumpPowerMultAdd += stack * addedJumpPower;
                args.baseJumpPowerAdd += stack * addedJumpPower;
            }
            private void FixedUpdate()
            {
                if (!body.characterMotor || !body)
                    return;

                if (body.characterMotor.isGrounded)
                {
                    return;
                }

                if (body.inputBank.jump.down)
                {
                    body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity;
                }    
            }
        }
    }
}
