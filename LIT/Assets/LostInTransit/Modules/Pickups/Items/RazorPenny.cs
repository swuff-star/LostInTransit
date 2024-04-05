using MSU;
using RoR2;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class RazorPenny : LITItem
    {
        private const string token = "LIT_ITEM_RAZORPENNY_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("RazorPenny", LITBundle.Items);

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Crit per Razor Penny", ConfigDescOverride = "Extra Crit added per penny.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float pennyCrit = 4f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Gold per Crit", ConfigDescOverride = "Gold gained on crit.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float critGold = 1f;


        public class RazorPennyBehavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.RazorPenny;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += pennyCrit * stack;
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.damageInfo.crit == true)
                {
                    body.master.GiveMoney((uint)(stack * (Run.instance.stageClearCount + (critGold * 1f))));
                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, damageReport.victimBody.transform.position, UnityEngine.Vector3.up, true);
                }
            }
        }
    }
}
