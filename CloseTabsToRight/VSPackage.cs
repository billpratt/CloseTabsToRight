using System.Runtime.InteropServices;
using CloseTabsToRight.Commands;
using Microsoft.VisualStudio.Shell;

namespace CloseTabsToRight
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidCloseTabsToRightCommandPackageString)]
    public sealed class VsPackage : Package
    {
        protected override void Initialize()
        {
            CloseTabsToRightCommand.Initialize(this);
            base.Initialize();
        }

    }
}
