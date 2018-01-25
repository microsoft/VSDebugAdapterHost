// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EngineLauncher.Utilities;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace EngineLauncher.Model
{
    internal class EngineLauncherModel
    {
        #region Private Fields

        private EngineLauncherPackage package;
        private Lazy<IEnumerable<EngineRegistration>> enginesLazy;
        private List<LaunchConfiguration> additionalLaunchConfigurations;

        private static readonly Guid AdapterHostClsid = new Guid("DAB324E9-7B35-454C-ACA8-F6BB0D5C8673");

        #endregion

        #region Constructor

        internal EngineLauncherModel(EngineLauncherPackage package)
        {
            this.package = package;

            this.enginesLazy = new Lazy<IEnumerable<EngineRegistration>>(() => this.GetEngineRegistrations());
            this.additionalLaunchConfigurations = new List<LaunchConfiguration>();
        }

        #endregion

        #region Internal API

        internal IEnumerable<EngineRegistration> EngineRegistrations => this.enginesLazy.Value;
        internal IEnumerable<LaunchConfiguration> LaunchConfigurations => this.GetLaunchConfigurations();

        internal LaunchConfiguration CreateUserLaunchConfiguration(string path)
        {
            LaunchConfiguration configuration = new LaunchConfiguration(path, true);

            this.additionalLaunchConfigurations.Add(configuration);

            return configuration;
        }

        internal void RemoveUserLaunchConfiguration(string path)
        {
            this.additionalLaunchConfigurations.Remove(this.additionalLaunchConfigurations
                .FirstOrDefault(c => String.Equals(c.Path, path, StringComparison.OrdinalIgnoreCase)));
        }

        internal void Reset()
        {
            this.additionalLaunchConfigurations.Clear();
        }

        #endregion

        #region Engine Registrations

        private IEnumerable<EngineRegistration> GetEngineRegistrations()
        {
            List<EngineRegistration> registrations = new List<EngineRegistration>();

            // Check all engine registrations to find ones that use the debug adapter host
            using (RegistryKey enginesKey = this.package.ApplicationRegistryRoot.OpenSubKey("AD7Metrics\\Engine"))
            {
                if (enginesKey != null)
                {
                    foreach (string engineId in enginesKey.GetSubKeyNames())
                    {
                        using (RegistryKey engineKey = enginesKey.OpenSubKey(engineId))
                        {
                            Guid? clsid = engineKey.GetGuidValue("CLSID");
                            if (clsid == null || clsid.Value != AdapterHostClsid)
                            {
                                // CLSID wasn't specified, or doesn't point to the debug adapter host, so skip it
                                continue;
                            }

                            Guid? id = GuidHelper.TryParseGuid(engineId);
                            if (id == null)
                            {
                                continue;
                            }

                            // It's common to have multiple registrations for the same engine, where one is for launch
                            //  and the other is for attach - try to detect this so we can tell the values apart.
                            string engineName = engineKey.GetValue("Name") as string;
                            if (engineKey.GetBoolValue("Attach") != false && engineKey.GetGuidValue("PortSupplier") != null)
                            {
                                // If the registration specifies true for "Attach" and includes a port supplier guid, consider
                                //  it an "attach" registration
                                engineName = engineName + " [Attach]";
                            }

                            registrations.Add(new EngineRegistration(engineName, id.Value));
                        }
                    }
                }
            }

            return registrations;
        }

        #endregion

        #region Launch Configurations

        private IEnumerable<LaunchConfiguration> GetLaunchConfigurations()
        {
            List<LaunchConfiguration> configs = new List<LaunchConfiguration>();

            // First, look for files named "launch*.json" in all projects in the current solution
            DTE2 dte = (DTE2)((IServiceProvider)this.package).GetService(typeof(SDTE));
            foreach (Project project in dte.Solution.Projects)
            {
                this.GetProjectLaunchConfigurations(project.ProjectItems, configs);
            }

            // Then include any manually-added files loaded from the settings
            configs.AddRange(this.additionalLaunchConfigurations);

            return configs;
        }

        private void GetProjectLaunchConfigurations(ProjectItems items, List<LaunchConfiguration> configs)
        {
            short fileNameIndex = 0;

            foreach (ProjectItem projectItem in items)
            {
                if (projectItem.ProjectItems != null && projectItem.ProjectItems.Count != 0)
                {
                    this.GetProjectLaunchConfigurations(projectItem.ProjectItems, configs);
                }

                if (projectItem.SubProject != null && projectItem.SubProject.ProjectItems != null)
                {
                    this.GetProjectLaunchConfigurations(projectItem.SubProject.ProjectItems, configs);
                }

                string fileName = null;
                try
                {
                    fileName = projectItem.FileNames[fileNameIndex];
                }
                catch
                {
                    // Some project systems use 0-based indexes, others are 1-based - try the other option
                    fileNameIndex = (short)((fileNameIndex == 0) ? 1 : 0);

                    try
                    {
                        fileName = projectItem.FileNames[fileNameIndex];
                    }
                    catch
                    {
                    }
                }

                if (!String.IsNullOrEmpty(fileName) &&
                    Path.GetFileName(fileName).StartsWith("launch", StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(Path.GetExtension(fileName), ".json", StringComparison.OrdinalIgnoreCase))
                {
                    configs.Add(new LaunchConfiguration(fileName));
                }
            }
        }

        #endregion
    }

    internal class EngineRegistration
    {
        #region Constructor

        internal EngineRegistration(string name, Guid id)
        {
            this.Name = name;
            this.Id = id;
        }

        #endregion

        #region Properties

        internal string Name { get; }
        internal Guid Id { get; }

        #endregion
    }

    internal class LaunchConfiguration
    {
        #region Constructor

        internal LaunchConfiguration(string path, bool isUserPath = false)
        {
            this.Path = path;
            this.IsUserPath = isUserPath;
        }

        #endregion

        #region Properties

        internal string Path { get; }
        internal bool IsUserPath { get; }

        #endregion
    }
}
