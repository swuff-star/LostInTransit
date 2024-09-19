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
    public class Prescriptions : LITEquipment, IContentPackModifier
    {
        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;

        public override EquipmentDef equipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        private AssetCollection _assetCollection;

        public override bool Execute(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMeds.buffIndex, 12 * timeMult);
            return true;
        }

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += MedsGain;
        }

        private void MedsGain(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(LITContent.Buffs.bdMeds))
            {
                args.attackSpeedMultAdd += 0.7f;
                args.damageMultAdd += 0.2f;
                args.armorAdd += 20f;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acPrescriptions", LITBundle.Equips);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;
            _equipmentDef = _assetCollection.FindAsset<EquipmentDef>("Prescriptions");
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_assetCollection.FindAsset<BuffDef>("bdMeds"));
        }
    }
}                                                                  
