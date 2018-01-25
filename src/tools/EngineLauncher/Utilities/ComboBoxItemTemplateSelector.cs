// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System.Windows;
using System.Windows.Controls;

namespace EngineLauncher.Utilities
{
    public class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = container as ContentPresenter;
            if (presenter != null)
            {
                return presenter.TemplatedParent is ComboBox ? this.SelectedTemplate : this.ListTemplate;
            }

            return base.SelectTemplate(item, container);
        }

        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate ListTemplate { get; set; }
    }
}
