using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;

namespace LostInTransit.Characters
{
    public sealed class Drifter : SurvivorBase
    {
        public override SurvivorDef SurvivorDef => LITAssets.LoadAsset<SurvivorDef>("SurvivorDrifter", LITBundle.Characters);

        public override GameObject BodyPrefab => LITAssets.LoadAsset<GameObject>("DrifterBody", LITBundle.Characters);

        public override GameObject MasterPrefab => null;
    }
}
