//------------------------------------------------------------------------------
// <copyright file="CloseTabsToRightCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CloseTabsToRight.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CloseTabsToRightCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d240bb4e-16ec-4c43-bdad-3847641f8e30");

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

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService != null)
            {
                var id = new CommandID(CommandSet, CommandId);
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

            var vsWindowFrames = GetVsWindowFrames().ToList();
            var activeFrame = GetActiveWindowFrame(vsWindowFrames);
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
            var vsWindowFrames = GetVsWindowFrames().ToList();
            var windowFrames = vsWindowFrames.Select(vsWindowFrame => vsWindowFrame as WindowFrame);
            var activeFrame = GetActiveWindowFrame(vsWindowFrames);

            var windowFrame = activeFrame;
            if (windowFrame == null)
                return;

            var dict = windowFrames.ToDictionary(frame => frame.FrameMoniker.ViewMoniker);

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

                var frame = dict[name];
                if (frame != null)
                    framesToClose.Add(frame);
            }

            foreach (var frame in framesToClose)
            {
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

        private static string CleanDocumentViewName(string name)
        {
            return name.StartsWith("D:0:0:") ? name.Substring(6) : name;
        }

        private WindowFrame GetActiveWindowFrame(IEnumerable<IVsWindowFrame> frames)
        {
            return (from vsWindowFrame in frames
                    let window = GetWindow(vsWindowFrame)
                    where window == _dte.ActiveWindow
                    select vsWindowFrame as WindowFrame)
                .FirstOrDefault();
        }

        private static Window GetWindow(IVsWindowFrame vsWindowFrame)
        {
            object window;
            ErrorHandler.ThrowOnFailure(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ExtWindowObject,
                out window));

            return window as Window;
        }

        private IEnumerable<IVsWindowFrame> GetVsWindowFrames()
        {
            var windowFrames = new List<IVsWindowFrame>();

            var uiShell = ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

            IEnumWindowFrames windowEnumerator;
            ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out windowEnumerator));

            if (windowEnumerator.Reset() != VSConstants.S_OK)
                return Enumerable.Empty<IVsWindowFrame>();

            var frames = new IVsWindowFrame[1];
            bool hasMorewindows = true;
            do
            {
                uint fetched;
                hasMorewindows = windowEnumerator.Next(1, frames, out fetched) == VSConstants.S_OK && fetched == 1;

                if (!hasMorewindows || frames[0] == null)
                    continue;

                windowFrames.Add(frames[0]);

            } while (hasMorewindows);

            return windowFrames;
        }

        private static DocumentGroup GetDocumentGroup(WindowFrame windowFrame)
        {
            return Microsoft.VisualStudio.PlatformUI.ExtensionMethods.FindAncestor<DocumentGroup, ViewElement>(
                            windowFrame.FrameView, e => e.Parent as ViewElement);
        }
    }
}
