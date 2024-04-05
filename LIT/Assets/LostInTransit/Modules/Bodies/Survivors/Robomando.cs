using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSU;
using RoR2;
using RoR2.ContentManagement;

namespace LostInTransit.Characters
{
#if DEBUG
    public sealed class Robomando : LITSurvivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;

        public override NullableRef<GameObject> MasterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * SurvivorDef - "SurvivorRobomando" - Characters
             * GameObject - "RobomandoBody" - Characters
             */
            yield return null;
        }
    }
#endif
}
