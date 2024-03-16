using System.Text.RegularExpressions;
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

            //Name begins with "D:{number}:{number}:" where {number} can vary 
            //depending on the number of tabs open for the same file
            return Regex.IsMatch(name, @"^(D:\d+:\d+:)") ? name.Substring(6) : name;
        }

        public static DocumentGroup GetDocumentGroup(WindowFrame windowFrame)
        {
            return Microsoft.VisualStudio.PlatformUI.ExtensionMethods.FindAncestor<DocumentGroup, ViewElement>(
                windowFrame.FrameView, e => e.Parent as ViewElement);
        }
    }
}
