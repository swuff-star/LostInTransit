using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUpdater
{
    [MenuItem("Tools/MSEU/Upgrade Scriptable Objects")]
    private static void UpdateMSUScriptableObjects()
    {
        if (!EditorUtility.DisplayDialog("Upgrade Scriptable Objects", "By accepting this menu, Unity will attempt to upgrade your Legacy MoonstormSharedUtils ScriptableObjects into their new, MSU.Runtime Scriptable Object version (An example is MSEliteDef -> ExtendedEliteDef). This utility does not upgrade Event related scriptable objects.\n\n" +
            "It is Heavily recommended to make a backup of your project before continuing!", "I've made a backup, go ahead!", "Cancel"))
        {
            return;
        }
        MSULog.Info("Upgrading Scriptable Objects...");

        Action[] upgrades = new Action[]
        {
            UpgradeMSInteractableDirectorCard,
            UpgradeMSMonsterDirectorCard,
            UpgradeMSEliteDef,
            UpgradeSerializableEliteTierDef,
            UpgradeItemDisplayDictionary,
            UpgradeNamedIDRS,
            UpgradeMSUnlockableDef,
            UpgradeVanillaSkinDefinition
        };

        using (var progressBar = new ThunderKit.Common.Logging.ProgressBar("Upgrading Scriptable Objects"))
        {
            for(int i = 0; i < upgrades.Length; i++)
            {
                var action = upgrades[i];
                var actionName = action.Method.Name;
                progressBar.Update($"Running Method: {actionName}", "Upgrading Scriptable Objects", RoR2.Util.Remap(i, 0, upgrades.Length, 0, 1));
                try
                {
                    Thread.Sleep(1000);
                    action();
                }
                catch(Exception e)
                {
                    MSULog.Error($"Could not finish upgrade method \"{actionName}\". {e}");
                }
            }
        }
    }

    private static void UpgradeMSInteractableDirectorCard()
    {
    }

    private static void UpgradeMSMonsterDirectorCard()
    {

    }

    private static void UpgradeMSEliteDef()
    {

    }

    private static void UpgradeSerializableEliteTierDef()
    {

    }

    private static void UpgradeItemDisplayDictionary()
    {

    }

    private static void UpgradeNamedIDRS()
    {

    }

    private static void UpgradeMSUnlockableDef()
    {

    }

    private static void UpgradeVanillaSkinDefinition()
    {

    }
}
