using LostInTransit.Utils;
using R2API;
using MSU;
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
using UObject = UnityEngine.Object;
using System.Collections;

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
        Characters,
        Shared
    }

    public static class LITAssets
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";
        private const string MAIN = "litmain";
        private const string BASE = "litbase";
        private const string ARTIFACTS = "litartifacts";
        private const string EQUIPS = "litequips";
        private const string ITEMS = "lititems";
        private const string CHARACTERS = "litcharacters";
        private const string SHARED = "litshared";

        private static string SoundBankPath { get => Path.Combine(Path.GetDirectoryName(LITMain.instance.Info.Location), "soundbanks", "LostInTransitSoundbank.bnk"); }
        private static string AssetBundleFolderPath => Path.Combine(Path.GetDirectoryName(LITMain.instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        private static Dictionary<LITBundle, AssetBundle> _assetBundles = new Dictionary<LITBundle, AssetBundle>();
        private static AssetBundle[] _streamedSceneBundles = Array.Empty<AssetBundle>();

        public static ResourceAvailability AssetsAvailability = new ResourceAvailability();

        public static AssetBundle GetAssetBundle(LITBundle bundle)
        {
            return _assetBundles[bundle];
        }

        public static TAsset LoadAsset<TAsset>(string name, LITBundle bundle) where TAsset : UObject
        {
            TAsset asset = null;
            if (bundle == LITBundle.All)
            {
                return FindAsset<TAsset>(name);
            }

            asset = _assetBundles[bundle].LoadAsset<TAsset>(name);

#if DEBUG
            if (!asset)
            {
                LITLog.Warning($"The method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, LITBundle)\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

                return LoadAsset<TAsset>(name, LITBundle.All);
            }
#endif
            return asset;
        }

        public static LITAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name, LITBundle bundle) where TAsset : UObject
        {
            return new LITAssetRequest<TAsset>(name, bundle);
        }

        public static TAsset[] LoadAllAssets<TAsset>(LITBundle bundle) where TAsset : UObject
        {
            TAsset[] loadedAssets = null;
            if (bundle == LITBundle.All)
            {
                return FindAssets<TAsset>();
            }
            loadedAssets = _assetBundles[bundle].LoadAllAssets<TAsset>();

#if DEBUG
            if (loadedAssets.Length == 0)
            {
                LITLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} inside the bundle {bundle}");
            }
#endif
            return loadedAssets;
        }

        public static LITAssetRequest<TAsset> LoadAllAssetsAsync<TAsset>(LITBundle bundle) where TAsset : UObject
        {
            return new LITAssetRequest<TAsset>(bundle);
        }

        internal static IEnumerator Initialize()
        {
            LITLog.Info($"Initializing Assets");

            ParallelMultiStartCoroutine helper1 = new ParallelMultiStartCoroutine();

            helper1.Add(LoadAssetBundles);
            helper1.Add(LoadSoundbank);

            helper1.Start();
            while (!helper1.IsDone()) yield return null;

            ParallelMultiStartCoroutine helper2 = new ParallelMultiStartCoroutine();
            helper2.Add(SwapShaders);
            helper2.Add(SwapAddressableShaders);

            helper2.Start();
            while (!helper2.IsDone())
                yield return null;

            AssetsAvailability.MakeAvailable();
            yield break;
        }

        private static IEnumerator LoadAssetBundles()
        {
            ParallelMultiStartCoroutine helper = new ParallelMultiStartCoroutine();

            List<(string path, LITBundle bundleEnum, AssetBundle loadedBundle)> pathsAndBundles = new List<(string path, LITBundle bundleEnum, AssetBundle loadedBundle)>();

            string[] paths = GetAssetBundlePaths();
            for(int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                helper.Add(LoadFromPath, pathsAndBundles, path, i, paths.Length);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            foreach((string path, LITBundle bundleEnum, AssetBundle assetBundle) in pathsAndBundles)
            {
                if(bundleEnum == LITBundle.Invalid)
                {
                    HG.ArrayUtils.ArrayAppend(ref _streamedSceneBundles, assetBundle);
                }
                else
                {
                    _assetBundles[bundleEnum] = assetBundle;
                }
            }
        }

        private static IEnumerator LoadSoundbank()
        {
            byte[] soundBankBytes;

            using(FileStream stream = File.Open(SoundBankPath, FileMode.Open))
            {
                soundBankBytes = new byte[stream.Length];
                var task = stream.ReadAsync(soundBankBytes, 0, soundBankBytes.Length);

                while (!task.IsCompleted)
                    yield return null;
            }

            SoundAPI.SoundBanks.Add(soundBankBytes);
        }

        private static IEnumerator LoadFromPath(List<(string path, LITBundle bundleEnum, AssetBundle loadedBundle)> list, string path, int index, int totalPaths)
        {
            string fileName = Path.GetFileName(path);
            LITBundle? litBundleEnum = null;

            switch(fileName)
            {
                case MAIN: litBundleEnum = LITBundle.Main; break;
                case BASE: litBundleEnum = LITBundle.Base; break;
                case ARTIFACTS: litBundleEnum = LITBundle.Artifacts; break;
                case EQUIPS: litBundleEnum = LITBundle.Equips; break;
                case ITEMS: litBundleEnum = LITBundle.Items; break;
                case CHARACTERS: litBundleEnum = LITBundle.Characters; break;
                case SHARED: litBundleEnum = LITBundle.Shared; break;

                //This path does not match any of the non scene bundles, could be a scene, we will mark these on only this ocassion as "Invalid".
                default: litBundleEnum = LITBundle.Invalid; break;
            }

            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
                yield return null;

            AssetBundle bundle = request.assetBundle;
            //Throw if no bundle was loaded
            if (!bundle)
            {
                throw new FileLoadException($"AssetBundle.LoadFromFile did not return an asset bundle. (Path={path})");
            }

            //The switch statement considered this a streamed scene bundle
            if (litBundleEnum == LITBundle.Invalid)
            {
                //supposed bundle is not streamed scene? throw exception.
                if (!bundle.isStreamedSceneAssetBundle)
                {
                    throw new Exception($"AssetBundle in specified path is not a streamed scene bundle, but its file name was not found in the Switch statement. have you forgotten to setup the enum and file name in your assets class? (Path={path})");
                }
                else
                {
                    //bundle is streamed scene, add to the list and break.
                    list.Add((path, LITBundle.Invalid, bundle));
                    yield break;
                }
            }

            //The switch statement considered this to not be a streamed scene bundle, but an assets bundle.
            list.Add((path, litBundleEnum.Value, bundle));
            yield break;
        }


        private static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(AssetBundleFolderPath)
               .Where(filePath => !filePath.EndsWith(".manifest"))
               .ToArray();
        }

        private static IEnumerator SwapShaders()
        {
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundles.Values.ToArray());
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundles.Values.ToArray());
        }

        private static TAsset FindAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            TAsset loadedAsset = null;
            LITBundle foundInBundle = LITBundle.Invalid;
            foreach ((var enumVal, var assetBundle) in _assetBundles)
            {
                loadedAsset = assetBundle.LoadAsset<TAsset>(name);

                if (loadedAsset)
                {
                    foundInBundle = enumVal;
                    break;
                }
            }

#if DEBUG
            if (loadedAsset)
                LITLog.Info($"Asset of type {typeof(TAsset).Name} with name {name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly.");
            else
                LITLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
#endif

            return loadedAsset;
        }

        private static TAsset[] FindAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            List<TAsset> assets = new List<TAsset>();
            foreach ((_, var bundles) in _assetBundles)
            {
                assets.AddRange(bundles.LoadAllAssets<TAsset>());
            }

#if DEBUG
            if (assets.Count == 0)
                LITLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
#endif

            return assets.ToArray();
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
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(LITAssets))
                    continue;

                if (declaringType == typeof(LITAssets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
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
    }

    public class LITAssetRequest<TAsset> where TAsset : UObject
    {
        public TAsset Asset => _asset;
        private TAsset _asset;

        public IEnumerable<TAsset> Assets => _assets;
        private List<TAsset> _assets;

        public LITBundle TargetBundle => _targetBundle;
        private LITBundle _targetBundle;

        public NullableRef<string> AssetName => _assetName;
        private NullableRef<string> _assetName;

        private bool _singleAssetLoad = true;

        public bool IsComplete => !_internalCoroutine.MoveNext();
        private IEnumerator _internalCoroutine;

        public void StartLoad()
        {
            if (_singleAssetLoad)
            {
                _internalCoroutine = LoadSingleAsset();
            }
            else
            {
                _internalCoroutine = LoadMultipleAsset();
            }
        }

        private IEnumerator LoadSingleAsset()
        {
            AssetBundleRequest request = null;
            if (_targetBundle == LITBundle.All)
            {
                foreach (LITBundle enumVal in Enum.GetValues(typeof(LITBundle)))
                {
                    if (enumVal == LITBundle.Invalid || enumVal == LITBundle.All)
                        continue;

                    var bundle = LITAssets.GetAssetBundle(enumVal);
                    request = bundle.LoadAssetAsync<TAsset>(AssetName);
                    while (!request.isDone)
                    {
                        yield return null;
                    }

                    _asset = (TAsset)request.asset;
                    if (Asset)
                    {
                        _targetBundle = enumVal;
                        yield break;
                    }
                }

#if DEBUG
                if (!Asset)
                {
                    _targetBundle = LITBundle.Invalid;
                    LITLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {AssetName} in any of the bundles.");
                }
                else
                {
                    LITLog.Info($"Asset of type {typeof(TAsset).Name} with name {AssetName} was found inside bundle {TargetBundle}, it is recommended that you load the asset directly.");
                }
#endif
                yield break;
            }

            request = LITAssets.GetAssetBundle(TargetBundle).LoadAssetAsync<TAsset>(AssetName);
            while (!request.isDone)
                yield return null;

            _asset = (TAsset)request.asset;
#if DEBUG
            if(!_asset)
            {
                LITLog.Warning($"The method \"{GetCallingMethod()}\" is calling a CommissionAssetRequest.StartLoad() while the class has the values \"{typeof(TAsset).Name}\", \"{AssetName}\" and \"{TargetBundle}\", however, the asset could not be found.\n" +
        $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

                _targetBundle = LITBundle.All;
                this._internalCoroutine = LoadSingleAsset();
                yield break;
            }
#endif
        }

        private IEnumerator LoadMultipleAsset()
        {
            _assets.Clear();

            AssetBundleRequest request = null;
            if (TargetBundle == LITBundle.All)
            {
                foreach (LITBundle enumVal in Enum.GetValues(typeof(LITBundle)))
                {
                    if (enumVal == LITBundle.All || enumVal == LITBundle.Invalid)
                        continue;

                    request = LITAssets.GetAssetBundle(enumVal).LoadAllAssetsAsync<TAsset>();
                    while (!request.isDone)
                        yield return null;

                    _assets.AddRange(request.allAssets.OfType<TAsset>());
                }

#if DEBUG
                if (_assets.Count == 0)
                {
                    LITLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
                }
#endif
                yield break;
            }

            request = LITAssets.GetAssetBundle(TargetBundle).LoadAllAssetsAsync<TAsset>();
            while (!request.isDone) yield return null;

            _assets.AddRange(request.allAssets.OfType<TAsset>());

#if DEBUG
            if (_assets.Count == 0)
            {
                LITLog.Warning($"Could not find any asset of type {typeof(TAsset)} inside the bundle {TargetBundle}");
            }
#endif

            yield break;
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
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(LITAssets))
                    continue;

                if (declaringType == typeof(LITAssets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
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

        internal LITAssetRequest(string name, LITBundle bundle)
        {
            _singleAssetLoad = true;
            _assetName = name;
            _targetBundle = bundle;
        }

        internal LITAssetRequest(LITBundle bundle)
        {
            _singleAssetLoad = false;
            _assetName = new NullableRef<string>();
            _assets = new List<TAsset>();
            _targetBundle = bundle;
        }
    }
}
