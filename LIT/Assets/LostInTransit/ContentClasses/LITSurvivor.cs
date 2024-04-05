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
        public abstract SurvivorDef SurvivorDef { get; }
        public abstract NullableRef<GameObject> MasterPrefab { get; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.Component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.Asset => CharacterPrefab;
        public abstract GameObject CharacterPrefab { get; }

        public abstract IEnumerator LoadContentAsync();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract void Initialize();
    }
}