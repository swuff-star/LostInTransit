using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using System.Linq;
using R2API;
using MSU.Config;
using System.Collections;
using RiskOfOptions;
using MSU;

namespace LostInTransit
{
    public class LITConfig
    {
        public const string PREFIX = "LIT.";
        public const string MAIN = PREFIX + "Main";
        public const string ITEMS = PREFIX + "Items";
        public const string EQUIPS = PREFIX + "Equips";

        internal static ConfigFactory ConfigFactory { get; private set; }

        public static ConfigFile ConfigMain { get; private set; }
        public static ConfigFile ConfigItem { get; private set; }
        public static ConfigFile ConfigEquip { get; private set; }

        internal static IEnumerator RegisterToModSettingsManager()
        {
            var request = LITAssets.LoadAssetAsync<Sprite>("Icon", LITBundle.Main);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            ModSettingsManager.SetModIcon(request.Asset, LITMain.GUID, LITMain.MODNAME);
            ModSettingsManager.SetModDescription("Lost in Transit is a mod focused on restoring features lost from Risk of Rain 1, currently including items.", LITMain.GUID, LITMain.MODNAME);
        }

        internal LITConfig(BaseUnityPlugin bup)
        {
            ConfigFactory = new ConfigFactory(bup, true);
            ConfigMain = ConfigFactory.CreateConfigFile(MAIN, true);
            ConfigItem = ConfigFactory.CreateConfigFile(ITEMS, true);
            ConfigEquip = ConfigFactory.CreateConfigFile(EQUIPS, true);
        }

        /*public class LITConfig : ConfigLoader<LITConfig>
        {
            public const string items = "LIT.Items";
            public const string equips = "LIT.Equips";
            public const string blight = "LIT.BlightCosts";

            public override BaseUnityPlugin MainClass { get; } = LITMain.instance;

            public override bool CreateSubFolder => true;

            public static ConfigFile itemConfig;
            public static ConfigFile equipsConfig;
            public static ConfigFile blightCost;

            internal static ConfigEntry<bool> enableItems;
            internal static ConfigEntry<bool> enableEquipments;
            internal static ConfigEntry<KeyCode> frenziedBlink;
            internal static ConfigEntry<int> tpBlightCost;

            public void Init()
            {
                itemConfig = CreateConfigFile(items, true);
                equipsConfig = CreateConfigFile(equips, true);
                blightCost = CreateConfigFile(blight, true);

                SetConfigs();
            }

            internal static void SetConfigs()
            {
                frenziedBlink = LITMain.config.Bind<KeyCode>("Lost in Transit :: Keybinds", "AffixFrenzied Blink Key", KeyCode.F, "The key a player must press to use the blinking passive of the AffixFrenzied buff.");
                tpBlightCost = blightCost.Bind<int>($"Teleboss Cost Multiplier", "Teleboss Cost Multiplier", 2, "Cost multiplied appled to the final body cost if the body is part of the teleporter boss, set this to a negative value to disable teleport bosses altogether.");
            }

            internal static int BindBlightCost(GameObject bodyPrefab, SpawnCard card)
            {
                return blightCost.Bind<int>("Blight Costs",
                                                     $"{bodyPrefab.name} Blight Cost",
                                                     Mathf.RoundToInt(card.directorCreditCost / 2),
                                                     $"The cost of turning {bodyPrefab.name} into a blighted elite.\nSet this value to -1 to blacklist the body.").Value;
            }
        }*/
    }
}
