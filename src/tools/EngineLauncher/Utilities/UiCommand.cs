// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Input;

namespace EngineLauncher.Utilities
{
    internal class UiCommand : ICommand
    {
        private Func<bool> canExecuteFunc;
        private Action executeFunc;

        internal UiCommand(Action executeFunc, Func<bool> canExecuteFunc = null)
        {
            this.executeFunc = executeFunc;
            this.canExecuteFunc = canExecuteFunc;
        }

        internal void Refresh()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region ICommand Implementation

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (this.canExecuteFunc != null)
            {
                return this.canExecuteFunc();
            }

            return true;
        }

        public void Execute(object parameter)
        {
            this.executeFunc();
        }

        #endregion
    }

    internal class UiCommand<TParam> : ICommand
    {
        private Func<TParam, bool> canExecuteFunc;
        private Action<TParam> executeFunc;

        internal UiCommand(Action<TParam> executeFunc, Func<TParam, bool> canExecuteFunc = null)
        {
            this.executeFunc = executeFunc;
            this.canExecuteFunc = canExecuteFunc;
        }

        internal void Refresh()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region ICommand Implementation

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (this.canExecuteFunc != null)
            {
                return this.canExecuteFunc((TParam)parameter);
            }

            return true;
        }

        public void Execute(object parameter)
        {
            this.executeFunc((TParam)parameter);
        }

        #endregion
    }
}
