namespace CloseTabsToRight
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidCloseTabsToRightCommandPackageString = "7696d777-6fe2-44fa-96fc-ac6a1072f7a1";
        public const string guidCloseTabsToRightCommandPackageCmdSetString = "d240bb4e-16ec-4c43-bdad-3847641f8e30";
        public const string guidImagesString = "61ed8618-158e-4904-910d-dc5d80a539fd";
        public static Guid guidCloseTabsToRightCommandPackage = new Guid(guidCloseTabsToRightCommandPackageString);
        public static Guid guidCloseTabsToRightCommandPackageCmdSet = new Guid(guidCloseTabsToRightCommandPackageCmdSetString);
        public static Guid guidImages = new Guid(guidImagesString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int MyMenuGroup = 0x1020;
        public const int CloseTabsToRightCommandId = 0x0100;
        public const int bmpPic1 = 0x0001;
        public const int bmpPic2 = 0x0002;
        public const int bmpPicSearch = 0x0003;
        public const int bmpPicX = 0x0004;
        public const int bmpPicArrows = 0x0005;
        public const int bmpPicStrikethrough = 0x0006;
    }
}
