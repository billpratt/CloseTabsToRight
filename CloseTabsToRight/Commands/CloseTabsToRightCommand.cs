using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static CloseTabsToRight.Helpers.WindowFrameHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using static CloseTabsToRight.Helpers.DocumentHelpers;

namespace CloseTabsToRight.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CloseTabsToRightCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private readonly DTE2 _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseTabsToRightCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CloseTabsToRightCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;
            _dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var id = new CommandID(PackageGuids.GuidCommandPackageCmdSet, PackageIds.CloseTabsToRightCommandId);
                var command = new OleMenuCommand(CommandCallback, id);
                //command.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(command);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CloseTabsToRightCommand Instance { get; private set; }

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
            Instance = new CloseTabsToRightCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;

            var vsWindowFrames = GetVsWindowFrames(ServiceProvider).ToList();
            var activeFrame = GetActiveWindowFrame(vsWindowFrames, _dte);
            var docGroup = GetDocumentGroup(activeFrame);

            var docViewsToRight = GetDocumentViewsToRight(activeFrame, docGroup);

            button.Enabled = docViewsToRight.Any();
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
            CloseTabsToRight();
        }

        private void CloseTabsToRight()
        {
            var vsWindowFrames = GetVsWindowFrames(ServiceProvider).ToList();
            var windowFrames = vsWindowFrames.Select(vsWindowFrame => vsWindowFrame as WindowFrame);
            var activeFrame = GetActiveWindowFrame(vsWindowFrames, _dte);

            var windowFrame = activeFrame;
            if (windowFrame == null)
                return;

            var windowFramesDict = windowFrames.GroupBy(x => x.FrameMoniker.ViewMoniker).ToDictionary(frame => frame.First().FrameMoniker.ViewMoniker, frame => frame.First());
            var docGroup = GetDocumentGroup(windowFrame);
            var viewMoniker = windowFrame.FrameMoniker.ViewMoniker;
            var documentViews = docGroup.Children.Where(c => c != null && c.GetType() == typeof(DocumentView)).Select(c => c as DocumentView);

            var framesToClose = new List<WindowFrame>();
            var foundActive = false;
            foreach (var name in documentViews.Select(documentView => CleanDocumentViewName(documentView.Name)))
            {
                if (!foundActive)
                {
                    if (name == viewMoniker)
                    {
                        foundActive = true;

                    }

                    // Skip over documents until we have found the first one after the active
                    continue;
                }

                var frame = windowFramesDict[name];
                if (frame != null && !framesToClose.Contains(frame))
                    framesToClose.Add(frame);
            }

            foreach (var frame in framesToClose)
            {
                if (frame.Clones != null && frame.Clones.Any())
                {
                    var clones = frame.Clones.ToList();
                    foreach (var clone in clones)
                    {
                        clone.CloseFrame(__FRAMECLOSE.FRAMECLOSE_PromptSave);
                    }
                }
                frame.CloseFrame(__FRAMECLOSE.FRAMECLOSE_PromptSave);
            }
        }

        private IEnumerable<DocumentView> GetDocumentViewsToRight(WindowFrame activeWindowFrame, DocumentGroup docGroup)
        {
            var docViewsToRight = new List<DocumentView>();
            var viewMoniker = activeWindowFrame.FrameMoniker.ViewMoniker;
            var documentViews = docGroup.Children.Where(c => c != null && c.GetType() == typeof(DocumentView)).Select(c => c as DocumentView);
            var foundActive = false;

            foreach (var documentView in documentViews)
            {
                var name = CleanDocumentViewName(documentView.Name);
                if (!foundActive)
                {
                    if (name == viewMoniker)
                    {
                        foundActive = true;

                    }

                    // Skip over documents until we have found the first one after the active
                    continue;
                }

                docViewsToRight.Add(documentView);
            }

            return docViewsToRight;
        }
    }
}
