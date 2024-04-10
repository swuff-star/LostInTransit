
using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using R2API;
using RoR2.Projectile;

namespace LostInTransit.Items
{
    public sealed class RepulsionArmor : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_REPULCHEST_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of times required to take damage before activating Repulsion Armor.")]
        [FormatToken(TOKEN, 0)]
        public static int hitsNeeded = 6;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of extra hits needed per stack to activate Repulsion Armor.")] //This kinda sucks but is easy to include if anyone wanted it for some god-forsaken reason.
        public static float hitsNeededPerStack = 0f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of armor added while the Repulsion Armor buff is active.")]
        [FormatToken(TOKEN, 1)]
        public static float armorBonus = 500f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of time the Repulsion Armor buff lasts.")]
        [FormatToken(TOKEN, 2)]
        public static float buffBaseDuration = 3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra aount of time added to the Repulsion Armor buff per stack.")]
        [FormatToken(TOKEN, 3)]
        public static float buffStackDuration = 1.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Maximum length of the Repulsion Armor buff. Set to 0 to disable.")]
        public static float maximumBuffDuration = 0f;

        public static bool badFix = false;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private AssetCollection _assetCollection;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "Chestplate" - Items
             * BuffDef - "bdRepulsionArmorActive" - Items
             * BuffDef - "bdRepulsionArmorCD" - Items
             */
            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acChestplate", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("Chestplate");
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class RepulsionArmorBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Chestplate;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.bdRepulsionArmorActive) && !body.HasBuff(LITContent.Buffs.bdRepulsionArmorCD))
                {
                    body.SetBuffCount(LITContent.Buffs.bdRepulsionArmorCD.buffIndex, hitsNeeded);
                }
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker == body) return;

                int currentCDCount = (body.GetBuffCount(LITContent.Buffs.bdRepulsionArmorCD));
                if (currentCDCount > 0)
                {
                    body.RemoveBuff(LITContent.Buffs.bdRepulsionArmorCD);
                    currentCDCount--;

                    if (currentCDCount <= 0)
                    {
                        body.AddTimedBuff(LITContent.Buffs.bdRepulsionArmorActive.buffIndex, (buffBaseDuration + buffStackDuration * (stack - 1)));
                    }
                }
            }
        }

        public class RepulsionArmorActiveBehavior : BuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdRepulsionArmorActive;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += RepulsionArmor.armorBonus;
            }

            public void FixedUpdate()       //★ i think this works because of a bug; working is working!
            {
                Collider[] array = Physics.OverlapSphere(CharacterBody.corePosition, 2f, LayerIndex.projectile.mask);

                for (int i = 0; i < array.Length; i++)
                {
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (pc)
                    {
                        if (pc.owner != gameObject)
                        {
                            pc.owner = gameObject;

                            FireProjectileInfo info = new FireProjectileInfo()
                            {
                                projectilePrefab = pc.gameObject,
                                position = pc.gameObject.transform.position,
                                rotation = Quaternion.Inverse(pc.gameObject.transform.rotation),
                                owner = CharacterBody.gameObject,
                                damage = CharacterBody.damage * 5f,
                                force = 200f,
                                crit = true,
                                damageColorIndex = DamageColorIndex.Default,
                                target = null,
                                speedOverride = 120f,
                                fuseOverride = -1
                            };
                            ProjectileManager.instance.FireProjectile(info);

                            Destroy(pc.gameObject);
                        }
                    }
                }
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();
                CharacterBody.SetBuffCount(LITContent.Buffs.bdRepulsionArmorCD.buffIndex, (int)RepulsionArmor.hitsNeeded);
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                damageInfo.damage *= ((100f - RepulsionArmor.armorBonus) * 0.01f);
            }
        }
    }
}
