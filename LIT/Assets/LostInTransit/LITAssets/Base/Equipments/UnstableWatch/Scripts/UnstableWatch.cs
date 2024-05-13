using LostInTransit.Items;
using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class UnstableWatch : LITEquipment, IContentPackModifier
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private static GameObject _buffWard;

        private AssetCollection _assetCollection;

        public override bool Execute(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdTimeStop.buffIndex, 8 * timeMult);
            return true;
        }

        public override void Initialize()
        {
            On.RoR2.CharacterBody.RecalculateStats += DoSlow;
        }

        private void DoSlow(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(LITContent.Buffs.bdTimeStopDebuff))
            {
                self.moveSpeed *= 0f;
                self.attackSpeed *= 0f;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "UnstableWatch" - Equips
             * BuffDef - "bdTimeStop" - Equips
             * GameObject - "TimeStopSphere" - Equips
             * BuffDef - "bdTimeStopDebuff" - Equips
             */
            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acUnstableWatch", LITBundle.Equips);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _equipmentDef = _assetCollection.FindAsset<EquipmentDef>("UnstableWatch");
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class TimeStopBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation()]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdTimeStop;
            private GameObject wardInstance;

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                wardInstance = Instantiate(_buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = CharacterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();
                if (wardInstance != null)
                    Destroy(wardInstance);
            }
        }
    }
#endif
}                                                                  
