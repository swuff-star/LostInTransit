using R2API;
using UnityEngine;

namespace LostInTransit.Elites
{
    public static class VolatileSpitebomb
    {
        public static GameObject VolatileSpiteBomb { get => _volatileSpiteBomb; }
        private static GameObject _volatileSpiteBomb;
        /*public static int MaxBombCount
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(MaxBombCount));
                    return BombArtifactManager.maxBombCount;
                }
                return 30 + 5 * (Run.instance.stageClearCount + 1);
            }
        }

        public static float ExtraBombPerRadius
        {
            get
            {
                if (!Run.instance)
                {
                    LogGetError(nameof(ExtraBombPerRadius));
                    return BombArtifactManager.extraBombPerRadius;
                }
                return 6 + 1 * 0.12f * (Run.instance.stageClearCount + 1);
            }
        }

        public static float BombDamageCoef
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(BombDamageCoef));
                    return BombArtifactManager.bombDamageCoefficient;
                }
                return 1 + 1 * 0.1f * (Run.instance.stageClearCount + 1);
            }
        }

        public static float BombSpawnBaseRadius
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(BombSpawnBaseRadius));
                    return BombArtifactManager.bombSpawnBaseRadius;
                }
                return 4 + 1f * 0.12f * (Run.instance.stageClearCount + 1);
            }
        }

        public static float BombSpawnRadiusCoefficient
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(BombSpawnRadiusCoefficient));
                    return BombArtifactManager.bombSpawnRadiusCoefficient;
                }
                return 5 + 1 * 0.12f * (Run.instance.stageClearCount + 1);
            }
        }

        public static float BombBlastRadius
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(BombBlastRadius));
                    return BombArtifactManager.bombBlastRadius;
                }
                return 8;
            }
        }

        public static float BombFuseTimeout
        {
            get
            {
                if(!Run.instance)
                {
                    LogGetError(nameof(BombFuseTimeout));
                    return BombArtifactManager.bombFuseTimeout;
                }
                return 4f;
            }
        }

        public static float MaxBombStepUpDistance { get => 100f; }

        public static float MaxBombFallDistance { get => 500f; }*/

        internal static void BeginSetup()
        {
            LITLog.Info($"Volatile elites are enabled, setting up Volatile Bomb...");
            _volatileSpiteBomb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/SpiteBomb"), "VolatileSpitebomb", false);
            if (_volatileSpiteBomb)
            {
                HG.ArrayUtils.ArrayAppend(ref LITContent.Instance.SerializableContentPack.networkedObjectPrefabs, _volatileSpiteBomb);
            }
        }
        private static void LogGetError(string propertyName) => LITLog.Error($"Tried to get {propertyName} when no run is active! this is not allowed.");
    }
}
