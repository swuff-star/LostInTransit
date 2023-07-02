using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm;

namespace LostInTransit.Bodies
{
    [DisabledContent]
    public sealed class MechanicalSpider : MonsterBase
    {
        public override MSMonsterDirectorCard MonsterDirectorCard => throw new System.NotImplementedException();

        public override GameObject BodyPrefab => LITAssets.LoadAsset<GameObject>("MechanicalSpiderBody", LITBundle.Dev);

        public override GameObject MasterPrefab => LITAssets.LoadAsset<GameObject>("MechanicalSpiderMaster", LITBundle.Dev);
    }
}
