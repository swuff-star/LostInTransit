using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class RustyJetpack : ItemBase
    {
        private const string token = "LIT_ITEM_RUSTYJETPACK_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("RustyJetpack");

        [ConfigurableField(ConfigName = "Jump Power", ConfigDesc = "Added jump power per Jetpack, as a percentage of normal jump power. Halved after the first stack.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier(token, StatTypes.DivideBy2, 1)]
        public static float addedJumpPower = 50f;

        [ConfigurableField(ConfigName = "Fall Speed Reduction", ConfigDesc = "Reduced fall speed per Jetpack, in pecent")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float reducedGravity = 15f;

        [ConfigurableField(ConfigName = "Fall Speed Limit", ConfigDesc = "Maximum amount fall speed can be reduced by, in percent")]
        public static float minGrav = 90f;

        public class RustyJetpackBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => LITContent.Items.RustyJetpack;
            public void RecalculateStatsEnd()
            {
                body.jumpPower += addedJumpPower + ((addedJumpPower / 2) * (stack - 1));
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.jumpPowerMultAdd += (addedJumpPower /  100) * (1 + ((stack - 1) / 2));
            }
            private void FixedUpdate()
            {
                if (body.characterMotor.isGrounded)
                {
                    return;
                }
                float gravReduction = (float)Math.Pow(reducedGravity / 100, 6 / stack + 5);
                body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * Mathf.Min(1 - gravReduction, 1 - (minGrav / 100));
            }
        }
    }
}
