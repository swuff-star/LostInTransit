using BepInEx;
using BepInEx.Configuration;
using HG.Reflection;
using LostInTransit.Modules;
using Moonstorm;
using R2API;
using R2API.Utils;
using System.Linq;
using System.Security;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]
[assembly: SearchableAttribute.OptIn]

namespace LostInTransit
{
    #region R2API Dependencies
    [BepInDependency("com.bepis.r2api.dot")]
    [BepInDependency("com.bepis.r2api.networking")]
    [BepInDependency("com.bepis.r2api.prefab")]
    [BepInDependency("com.bepis.r2api.sound")]
    #endregion

    [BepInDependency("com.TeamMoonstorm.MoonstormSharedUtils", BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("com.TheMysticSword.AspectAbilities", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class LITMain : BaseUnityPlugin
    {
        internal const string GUID = "com.ContactLight.LostInTransit";
        internal const string MODNAME = "Lost in Transit";
        internal const string VERSION = "0.4.0";

        public static LITMain instance;

        public static PluginInfo pluginInfo;

        public static ConfigFile config;

        public static bool DEBUG = false;

        public void Awake()
        {
            instance = this;
            pluginInfo = Info;
            config = Config;
            LITLogger.logger = Logger;

            new LITConfig().Init();
            new LITAssets().Init();
            new LITContent().Init();
            new LITLanguage().Init();

            ConfigurableFieldManager.AddMod(this);
        }
    }
}