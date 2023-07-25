using Moonstorm;
using R2API;
using RoR2.Projectile;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
//using RoR2;

namespace LostInTransit.Projectiles
{
    public class ScrapProjectile : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = LITAssets.LoadAsset<GameObject>("DrifterScrapProjectile", LITBundle.Characters);
        public static GameObject scrapProj;

        public override void Initialize()
        {
            base.Initialize();
            scrapProj = ProjectilePrefab;
        }
    }
}