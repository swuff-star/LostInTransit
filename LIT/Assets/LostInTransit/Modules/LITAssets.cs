using LostInTransit.Utils;
using Moonstorm.Loaders;
using R2API;
using Moonstorm;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Path = System.IO.Path;

namespace LostInTransit
{
    public enum LITBundle
    {
        Invalid,
        All,
        Main,
        Base,
        Artifacts,
        Equips,
        Items,
        Dev,
        Shared
    }
    public class LITAssets : AssetsLoader<LITAssets>
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";
        private const string MAIN = "litmain";
        private const string BASE = "litbase";
        private const string ARTIFACTS = "litartifacts";
        private const string EQUIPS = "litequips";
        private const string ITEMS = "lititems";
        private const string DEV = "litdev";
        private const string SHARED = "litshared";

        private static Dictionary<LITBundle, AssetBundle> assetbundles = new Dictionary<LITBundle, AssetBundle>();
        private static AssetBundle[] streamedSceneBundles = Array.Empty<AssetBundle>();

        [Obsolete("LoadAsset should not be used without specifying the LITBundle", true)]
        public new static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
#if DEBUG
            LITLog.Warning($"Method {GetCallingMethod()} is trying to load an asset of name {name} and type {typeof(TAsset).Name} without specifying what bundle to use for loading. This causes large performance loss as LITAssets has to search thru the entire bundle collection. Avoid calling LoadAsset without specifying the AssetBundle.");
#endif
            return LoadAsset<TAsset>(name, LITBundle.All);
        }

        [Obsolete("LoadAllAssetsOfType should not be used without specifying the LITBundle", true)]
        public new static TAsset[] LoadAllAssetsOfType<TAsset>() where TAsset : UnityEngine.Object
        {
#if DEBUG
            LITLog.Warning($"Method {GetCallingMethod()} is trying to load all assets of type {typeof(TAsset).Name} without specifying what bundle to use for loading. This causes large performance loss as LITAssets has to search thru the entire bundle collection. Avoid calling LoadAsset without specifying the AssetBundle.");
#endif
            return LoadAllAssetsOfType<TAsset>(LITBundle.All);
        }

        public static TAsset LoadAsset<TAsset>(string name, LITBundle bundle) where TAsset : UnityEngine.Object
        {
            if (Instance == null)
            {
                LITLog.Error("Cannot load asset when there's no instance of LITAssets!");
                return null;
            }
            return Instance.LoadAssetInternal<TAsset>(name, bundle);
        }
        public static TAsset[] LoadAllAssetsOfType<TAsset>(LITBundle bundle) where TAsset : UnityEngine.Object
        {
            if (Instance == null)
            {
                LITLog.Error("Cannot load asset when there's no instance of LITAssets!");
                return null;
            }
            return Instance.LoadAllAssetsOfTypeInternal<TAsset>(bundle);
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for (int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();

                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType == typeof(LITAssets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName: {fileName}, Location: L{fileLineNumber} C{fileColumnNumber})";
            }

            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif

        public override AssetBundle MainAssetBundle => GetAssetBundle(LITBundle.Main);
        public string AssemblyDir => Path.GetDirectoryName(LITMain.pluginInfo.Location);
        private string SoundBankPath { get => Path.Combine(AssemblyDir, "soundbanks", "LostInTransitSoundbank.bnk"); }

        public AssetBundle GetAssetBundle(LITBundle bundle)
        {
            if (bundle == LITBundle.All || bundle == LITBundle.Invalid)
            {
                LITLog.Warning("Invalid bundle enum specified, bundle enum cannot be \"All\" or \"Invalid\"");
                return null;
            }
            return assetbundles[bundle];
        }

        internal void Init()
        {
            var bundlePaths = GetAssetBundlePaths();
            foreach (string path in bundlePaths)
            {
                var fileName = Path.GetFileName(path);
                switch (fileName)
                {
                    case MAIN: LoadAndAssign(path, LITBundle.Main); break;
                    case BASE: LoadAndAssign(path, LITBundle.Base); break;
                    case ARTIFACTS: LoadAndAssign(path, LITBundle.Artifacts); break;
                    case EQUIPS: LoadAndAssign(path, LITBundle.Equips); break;
                    case ITEMS: LoadAndAssign(path, LITBundle.Items); break;
                    case DEV: LoadAndAssign(path, LITBundle.Dev); break;
                    case SHARED: LoadAndAssign(path, LITBundle.Shared); break;
                    default:
                        {
                            try
                            {
                                var ab = AssetBundle.LoadFromFile(path);
                                if (!ab)
                                {
                                    throw new FileLoadException($"AssetBundle.LoadFromFile did not return an asset bundle. (Path:{path} FileName:{fileName})");
                                }
                                if (!ab.isStreamedSceneAssetBundle)
                                {
                                    throw new Exception($"AssetBundle is not a streamed scene bundle, but it's file name was not found on the Switch statement. (Path:{path} FileName:{fileName})");
                                }
                                else
                                {
                                    HG.ArrayUtils.ArrayAppend(ref streamedSceneBundles, ab);
                                }
                                LITLog.Warning($"Invalid or Unexpected file in the AssetBundles folder (File name: {fileName}, Path: {path})");
                            }
                            catch (Exception e)
                            {
                                LITLog.Error($"Default statement on bundle loading method hit, Exception thrown.\n{e}");
                            }
                            break;
                        }
                }
            }

            void LoadAndAssign(string path, LITBundle bundleEnum)
            {
                try
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(path);
                    if (!bundle)
                    {
                        throw new FileLoadException("AssetBundle.LoadFromFile did not return an asset bundle");
                    }
                    if (assetbundles.ContainsKey(bundleEnum))
                    {
                        throw new InvalidOperationException($"AssetBundle in path loaded succesfully, but the assetBundles dictionary already contains an entry for {bundleEnum}.");
                    }

                    assetbundles[bundleEnum] = bundle;
                }
                catch (Exception e)
                {
                    LITLog.Error($"Could not load assetbundle at path {path} and assign to enum {bundleEnum}. {e}");
                }
            }
        }

        private TAsset LoadAssetInternal<TAsset>(string name, LITBundle bundle) where TAsset : UnityEngine.Object
        {
            TAsset asset = null;
            if (bundle == LITBundle.All)
            {
                asset = FindAsset<TAsset>(name, out LITBundle foundInBundle);
#if DEBUG
                if (!asset)
                {
                    LITLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
                }
                else
                {
                    LITLog.Info($"Asset of type {typeof(TAsset).Name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly");
                }
#endif
                return asset;
            }

            asset = assetbundles[bundle].LoadAsset<TAsset>(name);
#if DEBUG
            if (!asset)
            {
                LITLog.Warning($"The  method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, SS2Bundle)\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");
                return LoadAssetInternal<TAsset>(name, LITBundle.All);
            }
#endif
            return asset;

            TAsset FindAsset<TAsset>(string assetName, out LITBundle foundInBundle) where TAsset : UnityEngine.Object
            {
                foreach ((var enumVal, var assetBundle) in assetbundles)
                {
                    var loadedAsset = assetBundle.LoadAsset<TAsset>(assetName);
                    if (loadedAsset)
                    {
                        foundInBundle = enumVal;
                        return loadedAsset;
                    }
                }
                foundInBundle = LITBundle.Invalid;
                return null;
            }
        }

        private TAsset[] LoadAllAssetsOfTypeInternal<TAsset>(LITBundle bundle) where TAsset : UnityEngine.Object
        {
            List<TAsset> loadedAssets = new List<TAsset>();
            if (bundle == LITBundle.All)
            {
                FindAssets<TAsset>(loadedAssets);
#if DEBUG
                if (loadedAssets.Count == 0)
                {
                    LITLog.Warning($"Could not find any asset of type {typeof(TAsset)} inside any of the bundles");
                }
#endif
                return loadedAssets.ToArray();
            }

            loadedAssets = assetbundles[bundle].LoadAllAssets<TAsset>().ToList();
#if DEBUG
            if (loadedAssets.Count == 0)
            {
                LITLog.Warning($"Could not find any asset of type {typeof(TAsset)} inside the bundle {bundle}");
            }
#endif
            return loadedAssets.ToArray();

            void FindAssets<TAsset>(List<TAsset> output) where TAsset : UnityEngine.Object
            {
                foreach ((var _, var bndl) in assetbundles)
                {
                    output.AddRange(bndl.LoadAllAssets<TAsset>());
                }
                return;
            }
        }

        private string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(Path.Combine(AssemblyDir, ASSET_BUNDLE_FOLDER_NAME))
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .ToArray();
        }

        internal void LoadSoundbank()
        {
            byte[] array = File.ReadAllBytes(SoundBankPath);
            SoundAPI.SoundBanks.Add(array);
        }

        internal void SwapMaterialShaders()
        {
            SwapShadersFromMaterials(LoadAllAssetsOfType<Material>(LITBundle.All).Where(mat => mat.shader.name.StartsWith("Stubbed")));
        }

        internal void FinalizeCopiedMaterials()
        {
            foreach (var (_, bundle) in assetbundles)
            {
                FinalizeMaterialsWithAddressableMaterialShader(bundle);
            }
        }
    }
}
