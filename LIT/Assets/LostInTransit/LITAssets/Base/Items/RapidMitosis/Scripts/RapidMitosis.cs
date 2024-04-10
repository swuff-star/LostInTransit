using MSU;
using R2API;
using RoR2.Items;
using RoR2;
using System;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;

namespace LostInTransit.Items
{
    public sealed class RapidMitosis : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_RAPIDMITOSIS_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Equipment CDR Amount", ConfigDescOverride = "Equipment Cooldown Reduction per Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisEquipCD = 0.30f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Skill CDR Amount", ConfigDescOverride = "Skill Cooldown Reduction granted via Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisSkillCD = 0.4f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Skill CDR Length", ConfigDescOverride = "Duration of the buff granted via Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisDur = 6f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private AssetCollection _assetCollection;

        public override void Initialize()
        {
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += Inventory_CalculateEquipmentCooldownScale;
            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += ProcMitosis;
        }

        private float Inventory_CalculateEquipmentCooldownScale(On.RoR2.Inventory.orig_CalculateEquipmentCooldownScale orig, Inventory self)
        {
            float num = orig(self);
            num *= (1 - MSUtil.InverseHyperbolicScaling(mitosisEquipCD, mitosisEquipCD, 0.7f, self.GetItemCount(LITContent.Items.RapidMitosis)));
            return num;
        }

        private static void ProcMitosis(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);

            if (self.hasAuthority && self.inventory)
            {
                int mitosisCount = self.inventory.GetItemCount(LITContent.Items.RapidMitosis);
                if (mitosisCount > 0)
                {
                    self.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMitosisBuff.buffIndex, Items.RapidMitosis.mitosisDur);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acRapidMitosis", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("RapidMitosis");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        //I'm pretty sure that recalculatestatsAPI can now handle skill cooldown scales? might be a good idea to switch to that ASAP
        public class MitosisBuffBehavior : BuffBehaviour, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => LITContent.Buffs.bdMitosisBuff;
            private GameObject effectInstance;
            private List<GameObject> effectInstances;

            public void RecalculateStatsEnd()
            {
                if (!enabled)
                    return;
                if (CharacterBody.HasBuff(LITContent.Buffs.bdMitosisBuff))
                {
                    if (CharacterBody.skillLocator)
                    {
                        if (CharacterBody.skillLocator.primary)
                            CharacterBody.skillLocator.primary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (CharacterBody.skillLocator.secondary)
                            CharacterBody.skillLocator.secondary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (CharacterBody.skillLocator.utility)
                            CharacterBody.skillLocator.utility.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (CharacterBody.skillLocator.special)
                            CharacterBody.skillLocator.special.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                    }
                }
            }

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                GameObject charModel = CharacterBody.modelLocator.modelTransform.gameObject;
                if (charModel != null)
                {
                    CharacterModel cm = charModel.GetComponent<CharacterModel>();
                    if (cm != null)
                    {
                        CharacterModel.RendererInfo[] rendererInfos = cm.baseRendererInfos;
                        if (rendererInfos != null)
                        {
                            for (int i = 0; i < rendererInfos.Length; i++)
                            {
                                //pls work
                                if (rendererInfos[i].renderer && !rendererInfos[i].ignoreOverlays)
                                {
                                    GameObject effect = AddParticles(rendererInfos[i].renderer, CharacterBody.coreTransform);
                                    if (effect != null)
                                    {
                                        //effectInstances.Add(effect);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private GameObject AddParticles(Renderer modelRenderer, Transform targetParentTransform)
            {
                if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
                {
                    GameObject effectPrefab = Instantiate(LITAssets.LoadAsset<GameObject>("MitosisEffect", LITBundle.Items), targetParentTransform);
                    ParticleSystem ps = effectPrefab.GetComponent<ParticleSystem>();
                    ParticleSystem.ShapeModule shape = ps.shape;
                    if (modelRenderer != null)
                    {
                        if (modelRenderer is MeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                            shape.meshRenderer = (MeshRenderer)modelRenderer;
                        }
                        else if (modelRenderer is SkinnedMeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                            shape.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                        }
                    }
                    ParticleSystem.MainModule main = ps.main;
                    ps.gameObject.SetActive(true);
                    BoneParticleController bpc = effectPrefab.GetComponent<BoneParticleController>();
                    if (bpc != null && modelRenderer is SkinnedMeshRenderer)
                    {
                        bpc.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                    }
                    return effectPrefab;
                }
                return null;
            }

            public void RecalculateStatsStart()
            {

            }
        }
    }
}
