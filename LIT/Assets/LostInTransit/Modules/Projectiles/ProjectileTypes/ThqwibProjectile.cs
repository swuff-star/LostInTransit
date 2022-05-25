using Moonstorm;
using R2API;
using RoR2.Projectile;
using UnityEngine;
//using RoR2;

namespace LostInTransit.Projectiles
{
    //[DisabledContent]
    public class ThqwibProjectile : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/characterbodies/scavsackprojectile"), "ThqwibProjectile", false);
        //★ without doing any fucking research or testing whatsoever, i think this is fine. there are 78 more errors to fix before the mod can build.

        public static GameObject ThqwibProj;

        public override void Initialize()
        {
            if (ProjectilePrefab)
            {
                var onKillComponent = ProjectilePrefab.GetComponent<ProjectileGrantOnKillOnDestroy>();
                if (onKillComponent)
                    GameObject.Destroy(onKillComponent);
                ProjectilePrefab.AddComponent<ProjectileChanceForOnKillOnDestroy>();
                ThqwibProj = ProjectilePrefab;
            }
        }
    }
}