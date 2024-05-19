using BepInEx;
using BepInEx.Configuration;
using HG.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MSU;
using ProperSave;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LostInTransit
{
    #region R2API Dependencies
    [BepInDependency("com.bepis.r2api.dot")]
    [BepInDependency("com.bepis.r2api.networking")]
    [BepInDependency("com.bepis.r2api.prefab")]
    [BepInDependency("com.bepis.r2api.sound")]
    #endregion

    [BepInDependency(MSU.MSUMain.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.RiskyLives.RiskyMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ProperSavePlugin.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    //[BepInDependency("com.TheMysticSword.AspectAbilities", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]

    public class LITMain : BaseUnityPlugin
    {
        internal const string GUID = "com.ContactLight.LostInTransit";
        internal const string MODNAME = "Lost in Transit";
        internal const string VERSION = "0.4.0";

        internal static LITMain Instance { get; private set; }

        public static bool RiskyModInstalled { get; private set; }
        public static bool ProperSaveInstalled { get; private set; }

        private void Awake()
        {
            Instance = this;

            new LITLog(Logger);
            new LITConfig(this);
            new LITContent();

            //Disabling temp items cuz they seem to be broken atm.
            //LITAssets.AssetsAvailability.CallWhenAvailable(() => new LITTempItems().Init());

            LanguageFileLoader.AddLanguageFilesFromMod(this, "LITLang");

            RiskyModInstalled = MSUtil.IsModInstalled("com.RiskyLives.RiskyMod");
            ProperSaveInstalled = MSUtil.IsModInstalled(ProperSave.ProperSavePlugin.GUID);
        }
    }
}