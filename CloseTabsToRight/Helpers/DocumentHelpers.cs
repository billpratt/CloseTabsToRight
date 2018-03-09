using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;

namespace CloseTabsToRight.Helpers
{
    public static class DocumentHelpers
    {
        public static string CleanDocumentViewName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            return name.StartsWith("D:0:0:") ? name.Substring(6) : name;
        }

        public static DocumentGroup GetDocumentGroup(WindowFrame windowFrame)
        {
            return Microsoft.VisualStudio.PlatformUI.ExtensionMethods.FindAncestor<DocumentGroup, ViewElement>(
                windowFrame.FrameView, e => e.Parent as ViewElement);
        }
    }
}
