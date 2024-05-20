using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using ThunderKit.Integrations.Thunderstore;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace LostInTransit.Editor.ImportExtensions
{
    public class InstallLITDependencies : OptionalExecutor
    {
        public override int Priority => -166_000;

        public override string Description => "Installs both RiskyMod and ProperSave to the project.";

        private const string TRANSIENT_STORE_NAME = "transient-store";
        private const string RISKY_MOD = "Risky_Lives-RiskyMod";
        private const string PROPER_SAVE = "KingEnderBrine-ProperSave";

        private ThunderstoreSource _transientStore;

        public override bool Execute()
        {
            try
            {
                EditorApplication.UnlockReloadAssemblies();
                var pkgSource = PackageSourceSettings.PackageSources.OfType<ThunderstoreSource>().FirstOrDefault(source => source.Url == "https://thunderstore.io");

                if (!pkgSource)
                {
                    if (_transientStore)
                    {
                        pkgSource = _transientStore;
                    }
                    else
                    {
                        pkgSource = CreateInstance<ThunderstoreSource>();
                        pkgSource.Url = "https://thunderstore.io";
                        pkgSource.name = TRANSIENT_STORE_NAME;
                        pkgSource.ReloadPages(true);
                        _transientStore = pkgSource;
                    }
                }
                else if (pkgSource.Packages == null || pkgSource.Packages.Count == 0)
                {
                    pkgSource.ReloadPages(true);
                }
                else
                {
                    _transientStore = pkgSource;
                }

                if(pkgSource.Packages == null || pkgSource.Packages.Count == 0)
                {
                    Debug.LogWarning($"PackageSource at \"{pkgSource.Url}\" has no packages.");
                    return false;
                }

                List<(PackageGroup, string)> pkgs = new List<(PackageGroup, string)>();
                foreach(string dependency in GetDependencyIDs())
                {
                    var pkg = pkgSource.Packages.FirstOrDefault(p => p.DependencyId == dependency);

                    if(pkg != null && !pkg.Installed)
                    {
                        pkgs.Add((pkg, "latest"));
                    }
                }

                if (pkgs.Count == 0)
                    return true;

                var task = pkgSource.InstallPackages(pkgs);
                while (!task.IsCompleted)
                {
                    Debug.Log("Waiting for Completion...");
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
            PackageHelper.ResolvePackages();
            return true;
        }

        private List<string> GetDependencyIDs()
        {
            return new List<string>
            {
                RISKY_MOD,
                PROPER_SAVE
            };
        }

        public override void Cleanup()
        {
            if (_transientStore)
                DestroyImmediate(_transientStore);
        }
    }
}
