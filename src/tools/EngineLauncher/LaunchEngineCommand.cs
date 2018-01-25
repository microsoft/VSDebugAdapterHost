// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using EngineLauncher.Dialog;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using static System.FormattableString;

namespace EngineLauncher
{
    internal sealed class LaunchEngineCommand
    {
        #region Private Fields

        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("67f86efb-e995-4cfe-9463-ee564a43d07a");

        const string DebugAdapterHostPackageCmdSet = "0ddba113-7ac1-4c6e-a2ef-dcac3f9e731e";
        const int LaunchCommandId = 0x0101;

        private readonly EngineLauncherPackage package;

        #endregion

        #region Static Instance

        private LaunchEngineCommand(EngineLauncherPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                CommandID menuCommandID = new CommandID(CommandSet, CommandId);
                MenuCommand menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static LaunchEngineCommand Instance { get; private set; }

        public static void Initialize(EngineLauncherPackage package)
        {
            Instance = new LaunchEngineCommand(package);
        }

        #endregion

        private IServiceProvider ServiceProvider => this.package;

        private void MenuItemCallback(object sender, EventArgs e)
        {
            EngineLauncherViewModel launcherVM = new EngineLauncherViewModel(this.package.Model);

            if (this.package.SelectedEngine != null)
            {
                EngineRegistrationViewModel engineVM = launcherVM.Engines.FirstOrDefault(evm => evm.Id == this.package.SelectedEngine);
                launcherVM.SelectedEngine = engineVM;
            }

            if (!String.IsNullOrEmpty(this.package.SelectedConfiguration))
            {
                LaunchConfigurationViewModel configVM = launcherVM.LaunchConfigs.FirstOrDefault(lvm => String.Equals(lvm.FullPath, this.package.SelectedConfiguration, StringComparison.OrdinalIgnoreCase));
                launcherVM.SelectedConfig = configVM;
            }

            EngineLauncherDialog dlg = new EngineLauncherDialog()
            {
                DataContext = launcherVM
            };

            if (dlg.ShowModal() == true)
            {
                try
                {
                    DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(SDTE));

                    string parameters = Invariant($@"/LaunchJson:""{launcherVM.SelectedConfig.FullPath}"" /EngineGuid:""{launcherVM.SelectedEngine.Id}""");
                    dte.Commands.Raise(DebugAdapterHostPackageCmdSet, LaunchCommandId, parameters, IntPtr.Zero);

                    // Successfully issued the command - save selected options
                    this.package.SelectedEngine = launcherVM.SelectedEngine.Id;
                    this.package.SelectedConfiguration = launcherVM.SelectedConfig.FullPath;
                }
                catch (Exception ex)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        String.Format(CultureInfo.CurrentCulture, "Launch failed.  Error: {0}", ex.Message),
                        null,
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }
    }
}
