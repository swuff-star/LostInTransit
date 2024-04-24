using LostInTransit.Items;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public sealed class GiganticAmethyst : LITEquipment
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            var sloc = slot.characterBody?.skillLocator;
            if ((bool)!sloc)
            {
                return false;
            }
            if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
            {
                sloc.ApplyAmmoPack();
            }
            sloc.ApplyAmmoPack();
            //Nebby from the future here, i really love this banter so i'm keeping it.

            //N: Why the fuck is this called "AmethystProc", theres nothing to proc, wtf swuff
            //S: you're proccing the amethyst what would you have called it?
            //S: "amethystfire"? you aren't FIRING anything.
            //S: "amethystuse" is dull.
            //N: "AmethystActivation", duh.
            //S: long
            Util.PlaySound("AmethystProc", slot.characterBody.gameObject);
            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var assetRequest = LITAssets.LoadAssetAsync<EquipmentDef>("GiganticAmethyst", LITBundle.Equips);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _equipmentDef = assetRequest.Asset;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}