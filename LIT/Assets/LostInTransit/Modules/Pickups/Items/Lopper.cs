using Moonstorm;
using RoR2;
using RoR2.Items;
using System;

namespace LostInTransit.Items
{
    public class Lopper : ItemBase
    {
        private const string token = "LIT_ITEM_LOPPER_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("Lopper", LITBundle.Items);

        [ConfigurableField(ConfigName = "Maximum Extra Damage per Lopper", ConfigDesc = "Maximum extra damage dealt by Ol' Lopper.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float lopperMaxBonus = 0.6f;


        public class LopperBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Lopper;

            public void OnIncomingDamageOther(HealthComponent healthComponent, DamageInfo damageInfo)
            {
                //Debug.Log("Damage before Lopper: " + damageInfo.damage);
                damageInfo.damage += stack * (damageInfo.damage * Math.Min(((1f - healthComponent.combinedHealthFraction) * 2f), lopperMaxBonus));
                //Debug.Log("Damage after Lopper: " + damageInfo.damage);
            }
        }
    }
}
