using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;

namespace LostInTransit.Characters
{
    [DisabledContent]
    public sealed class Robomando : SurvivorBase
    {
        public override SurvivorDef SurvivorDef => LITAssets.LoadAsset<SurvivorDef>("SurvivorRobomando", LITBundle.Characters);

        public override GameObject BodyPrefab => LITAssets.LoadAsset<GameObject>("RobomandoBody", LITBundle.Characters);

        public override GameObject MasterPrefab => null;
    }
}
