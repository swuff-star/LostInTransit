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
    public abstract class LITSurvivor : ISurvivorContentPiece
    {
        public abstract SurvivorDef survivorDef { get; }
        public abstract NullableRef<GameObject> masterPrefab { get; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => characterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.asset => characterPrefab;
        public abstract GameObject characterPrefab { get; }

        public abstract IEnumerator LoadContentAsync();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract void Initialize();
    }
}