using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm;

namespace LostInTransit.Bodies
{
    [DisabledContent]
    public sealed class Child : MonsterBase
    {
        public override MSMonsterDirectorCard MonsterDirectorCard => throw new System.NotImplementedException();

        public override GameObject BodyPrefab => LITAssets.LoadAsset<GameObject>("ChildBody");

        public override GameObject MasterPrefab => LITAssets.LoadAsset<GameObject>("ChildMaster");
    }
}
