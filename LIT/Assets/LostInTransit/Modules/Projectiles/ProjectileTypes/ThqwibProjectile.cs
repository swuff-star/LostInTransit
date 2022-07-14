using Moonstorm;
using R2API;
using RoR2.Projectile;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
//using RoR2;

namespace LostInTransit.Projectiles
{
    //[DisabledContent]
    public class ThqwibProjectile : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get => prefab; }
        private GameObject prefab;
        //★ without doing any fucking research or testing whatsoever, i think this is fine. there are 78 more errors to fix before the mod can build.

        public static GameObject ThqwibProj;

        public override async void Initialize()
        {
            prefab = await ClonePrefab();
            if (prefab)
            {
                var onKillComponent = prefab.GetComponent<ProjectileGrantOnKillOnDestroy>();
                if (onKillComponent)
                    GameObject.Destroy(onKillComponent);
                prefab.AddComponent<ProjectileChanceForOnKillOnDestroy>();
                ThqwibProj = prefab;
            }
        }

        private async Task<GameObject> ClonePrefab()
        {
            var fab = await Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavSackProjectile.prefab").Task;
            prefab = PrefabAPI.InstantiateClone(fab, "LIT_Thqwib", true);
            return prefab;
        }
    }
}