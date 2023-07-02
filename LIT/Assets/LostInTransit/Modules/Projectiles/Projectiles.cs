using Moonstorm;
using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LostInTransit.Modules
{
    public sealed class Projectiles : ProjectileModuleBase
    {
        public static Projectiles Instance { get; set; }
        public override R2APISerializableContentPack SerializableContentPack => LITContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLog.Info($"Initializing Projectiles...");
            GetProjectileBases();
        }

        protected override IEnumerable<ProjectileBase> GetProjectileBases()
        {
            base.GetProjectileBases()
                .ToList()
                .ForEach(proj => AddProjectile(proj));
            return null;
        }
    }
}
