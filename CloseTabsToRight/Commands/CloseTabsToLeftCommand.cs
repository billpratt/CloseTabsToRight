using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static CloseTabsToRight.Helpers.WindowFrameHelpers;
using static CloseTabsToRight.Helpers.DocumentHelpers;

namespace CloseTabsToRight.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CloseTabsToLeftCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private readonly DTE2 _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseTabsToLeftCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CloseTabsToLeftCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;
            _dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(PackageGuids.GuidCommandPackageCmdSet, PackageIds.CloseTabsToLeftCommandId);
                var menuItem = new MenuCommand(this.CommandCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CloseTabsToLeftCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new CloseTabsToLeftCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void CommandCallback(object sender, EventArgs e)
        {
            CloseTabsToLeft();
        }

        private void CloseTabsToLeft()
        {
            var vsWindowFrames = GetVsWindowFrames(_package).ToList();
            var windowFrames = vsWindowFrames.Select(vsWindowFrame => vsWindowFrame as WindowFrame);
            var activeFrame = GetActiveWindowFrame(vsWindowFrames, _dte);

            var windowFrame = activeFrame;
            if (windowFrame == null)
                return;

            var windowFramesDict = windowFrames.ToDictionary(frame => frame.FrameMoniker.ViewMoniker);
            var docGroup = GetDocumentGroup(windowFrame);
            var viewMoniker = windowFrame.FrameMoniker.ViewMoniker;
            var documentViews = docGroup.Children.Where(c => c != null && c.GetType() == typeof(DocumentView)).Select(c => c as DocumentView);

            var framesToClose = new List<WindowFrame>();
            foreach (var name in documentViews.Select(documentView => CleanDocumentViewName(documentView.Name)))
            {
                if (name == viewMoniker)
                {
                    // We found the active tab. No need to continue
                    break;
                }

                var frame = windowFramesDict[name];
                if (frame != null)
                    framesToClose.Add(frame);
            }

            foreach (var frame in framesToClose)
            {
                frame.CloseFrame(__FRAMECLOSE.FRAMECLOSE_PromptSave);
            }
        }
    }
}
