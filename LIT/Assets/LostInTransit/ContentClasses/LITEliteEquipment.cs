using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInTransit
{
    public abstract class LITEliteEquipment : IEliteContentPiece
    {
        public abstract List<EliteDef> eliteDefs { get; }
        public abstract NullableRef<List<GameObject>> itemDisplayPrefabs { get; }
        EquipmentDef IContentPiece<EquipmentDef>.asset => equipmentDef;
        public abstract EquipmentDef equipmentDef { get; }

        public abstract bool Execute(EquipmentSlot slot);
        public abstract void OnEquipmentObtained(CharacterBody body);
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract IEnumerator LoadContentAsync();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract void Initialize();
    }
}
