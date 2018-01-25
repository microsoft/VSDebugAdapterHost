// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EngineLauncher.Model;
using EngineLauncher.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace EngineLauncher.Dialog
{
    internal class EngineLauncherViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private EngineLauncherModel model;

        #endregion

        #region Constructor

        internal EngineLauncherViewModel(EngineLauncherModel model)
        {
            this.model = model;

            this.Engines = this.model.EngineRegistrations
                .Select(r => new EngineRegistrationViewModel(r))
                .OrderBy(vm => vm.Name)
                .ToList();

            this.LaunchConfigs = new ObservableCollection<LaunchConfigurationViewModel>(
                this.model.LaunchConfigurations
                .Select(c => new LaunchConfigurationViewModel(this, c))
                .OrderBy(vm => vm.IsUserPath)
                .ThenBy(vm => vm.FileName));

            this.OkCommand = new UiCommand<DialogWindow>(
                this.OnCommit,
                w => this.Validate());

            this.CancelCommand = new UiCommand<DialogWindow>(this.OnCancel);

            this.BrowseCommand = new UiCommand(this.OnBrowse);
        }

        #endregion

        #region Event Handlers

        private void OnBrowse()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Launch Configurations (*.json)|*.json|All Files (*.*)|(*.*)"
            };

            if (ofd.ShowDialog() == true)
            {
                LaunchConfiguration config = this.model.CreateUserLaunchConfiguration(ofd.FileName);
                LaunchConfigurationViewModel configVM = new LaunchConfigurationViewModel(this, config);

                this.LaunchConfigs.Add(configVM);

                this.SelectedConfig = configVM;
            }
        }

        private void OnCancel(DialogWindow w)
        {
            if (w != null)
            {
                w.DialogResult = false;
                w.Close();
            }
        }

        private bool Validate()
        {
            return this.SelectedEngine != null && this.SelectedConfig != null;
        }

        private void OnCommit(DialogWindow w)
        {
            if (w != null)
            {
                w.DialogResult = true;
                w.Close();
            }
        }

        internal void DeleteConfig(LaunchConfigurationViewModel config)
        {
            this.LaunchConfigs.Remove(config);

            this.model.RemoveUserLaunchConfiguration(config.FullPath);
        }

        #endregion

        #region Public Properties for Binding

        public UiCommand<DialogWindow> OkCommand { get; }
        public UiCommand<DialogWindow> CancelCommand { get; }
        public UiCommand BrowseCommand { get; }

        public IEnumerable<EngineRegistrationViewModel> Engines { get; }
        public ObservableCollection<LaunchConfigurationViewModel> LaunchConfigs { get; }

        private EngineRegistrationViewModel _selectedEngine;
        public EngineRegistrationViewModel SelectedEngine
        {
            get { return this._selectedEngine; }
            set
            {
                if (value != this._selectedEngine)
                {
                    this._selectedEngine = value;
                    this.NotifyPropertyChanged(nameof(this.SelectedEngine));
                    this.OkCommand.Refresh();
                }
            }
        }

        private LaunchConfigurationViewModel _selectedConfig;
        public LaunchConfigurationViewModel SelectedConfig
        {
            get { return this._selectedConfig; }
            set
            {
                if (value != this._selectedConfig)
                {
                    this._selectedConfig = value;
                    this.NotifyPropertyChanged(nameof(this.SelectedConfig));
                    this.OkCommand.Refresh();
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class EngineRegistrationViewModel
    {
        #region Private Fields

        private EngineRegistration registration;

        #endregion

        #region Constructor

        internal EngineRegistrationViewModel(EngineRegistration registration)
        {
            this.registration = registration;
        }

        #endregion

        #region Public Properties for Binding

        public string Name => this.registration.Name;
        public Guid Id => this.registration.Id;

        #endregion
    }

    internal class LaunchConfigurationViewModel
    {
        #region Private Fields

        private LaunchConfiguration config;

        #endregion

        #region Constructor

        internal LaunchConfigurationViewModel(EngineLauncherViewModel launcher, LaunchConfiguration config)
        {
            this.config = config;

            this.DeleteCommand = new UiCommand(
                () => launcher.DeleteConfig(this),
                () => this.IsUserPath);
        }

        #endregion

        #region Public Properties for Binding

        public string FullPath => this.config.Path;
        public string FileName => Path.GetFileName(this.FullPath);
        public bool IsUserPath => this.config.IsUserPath;

        public UiCommand DeleteCommand { get; }

        #endregion
    }
}
