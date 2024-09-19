using MSU;
using R2API;
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
    public abstract class LITMonster : IMonsterContentPiece
    {
        public abstract NullableRef<MonsterCardProvider> cardProvider { get; }
        public abstract NullableRef<DirectorCardHolderExtended> dissonanceCard { get; }
        public abstract NullableRef<GameObject> masterPrefab { get; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => characterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.asset => characterPrefab;
        public abstract GameObject characterPrefab { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}
