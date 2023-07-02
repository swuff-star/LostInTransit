using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Integrations.Thunderstore;

namespace LostInTransit.Editor.ImportExtensions
{
    public class InstallRiskyMod : ThunderstorePackageInstaller
    {
        public override string DependencyId => "Risky_Lives-RiskyMod";

        public override string ThunderstoreAddress => "https://thunderstore.io";

        public override string Description => "Installs RiskyMod, which LIT has cross compatibility with.";

        public override int Priority => Moonstorm.EditorUtils.Importers.Constants.Priority.InstallRiskOfOptions - 1;
    }
}
