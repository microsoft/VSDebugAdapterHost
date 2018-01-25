// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EngineLauncher.Model;
using EngineLauncher.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace EngineLauncher
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(EngineLauncherPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class EngineLauncherPackage : Package, IVsSolutionEvents
    {
        #region Constants

        public const string PackageGuidString = "ff5205bf-e1fd-4fb4-8105-91825e6834f0";

        private const string SelectedEngineKey = "SelectedEngine";
        private const string SelectedConfigurationKey = "SelectedConfiguration";
        private const string AdditionalLaunchConfigurationsKey = "AdditionalLaunchConfigurations";

        #endregion

        #region Constructor

        public EngineLauncherPackage()
        {
            this.Model = new EngineLauncherModel(this);

            this.AddOptionKey(SelectedEngineKey);
            this.AddOptionKey(SelectedConfigurationKey);
            this.AddOptionKey(AdditionalLaunchConfigurationsKey);

            this.ResetSettings();
        }

        #endregion

        #region Package Members

        internal Guid? SelectedEngine { get; set; }
        internal string SelectedConfiguration { get; set; }

        internal EngineLauncherModel Model { get; }

        private void ResetSettings()
        {
            this.SelectedEngine = null;
            this.SelectedConfiguration = null;

            this.Model.Reset();
        }

        protected override void Initialize()
        {
            base.Initialize();
            LaunchEngineCommand.Initialize(this);

            // Register for solution events so we can receive notification when a solution is closed.
            IVsSolution sln = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            uint cookie;
            sln.AdviseSolutionEvents(this, out cookie);
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string rawValue = reader.ReadToEnd();

                switch (key)
                {
                    case SelectedEngineKey:
                        this.SelectedEngine = GuidHelper.TryParseGuid(rawValue);
                        break;

                    case SelectedConfigurationKey:
                        this.SelectedConfiguration = rawValue;
                        break;

                    case AdditionalLaunchConfigurationsKey:
                        foreach (string configPath in rawValue.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            this.Model.CreateUserLaunchConfiguration(configPath);
                        }
                        break;

                    default:
                        Debug.Fail(Invariant($"Unknown storage key '{key}'!"));
                        break;
                }
            }

            base.OnLoadOptions(key, stream);
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string rawValue = null;

                switch (key)
                {
                    case SelectedEngineKey:
                        rawValue = this.SelectedEngine?.ToString("B");
                        break;

                    case SelectedConfigurationKey:
                        rawValue = this.SelectedConfiguration;
                        break;

                    case AdditionalLaunchConfigurationsKey:
                        rawValue = String.Join("|", this.Model.LaunchConfigurations.Where(c => c.IsUserPath).Select(c => c.Path));
                        break;

                    default:
                        Debug.Fail(Invariant($"Unknown storage key '{key}'!"));
                        break;
                }

                writer.Write(rawValue);
            }

            base.OnSaveOptions(key, stream);
        }

        #endregion

        #region IVsSolutionEvents Implementation

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            // Reset settings when the solution is closed.  This ensures that configuration options will not be reused if a solution
            //  is closed, and then another solution that does not already have our setting keys present in its SUO is opened.
            this.ResetSettings();
            return VSConstants.S_OK;
        }

        #endregion
    }
}
