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
    public abstract class LITVoidItem : IVoidItemContentPiece
    {
        public abstract NullableRef<List<GameObject>> itemDisplayPrefabs { get; }
        ItemDef IContentPiece<ItemDef>.asset => itemDef;
        public abstract ItemDef itemDef { get; }

        public abstract List<ItemDef> GetInfectableItems();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}
