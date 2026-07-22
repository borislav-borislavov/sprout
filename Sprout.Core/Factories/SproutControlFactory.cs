using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Sprout.Core.Factories
{
    public class SproutControlFactory : ISproutControlFactory
    {
        private readonly ISproutButtonFactory _sproutButtonFactory;
        private readonly IGridFactory _gridFactory;
        private readonly ISproutBorderFactory _sproutBorderFactory;
        private readonly ISproutCheckBoxFactory _sproutCheckBoxFactory;
        private readonly ISproutComboFactory _sproutComboFactory;
        private readonly ISproutDataGridFactory _sproutDataGridFactory;
        private readonly ISproutDatePickerFactory _sproutDatePickerFactory;
        private readonly ISproutLabelFactory _sproutLabelFactory;
        private readonly ISproutListFactory _sproutListFactory;
        private readonly ISproutTextBoxFactory _sproutTextBoxFactory;
        private readonly IDataAdapterFactory _dataAdapterFactory;

        public SproutControlFactory(
            ISproutButtonFactory sproutButtonFactory,
            IGridFactory gridFactory,
            ISproutBorderFactory sproutBorderFactory,
            ISproutCheckBoxFactory sproutCheckBoxFactory,
            ISproutComboFactory sproutComboFactory,
            ISproutDataGridFactory sproutDataGridFactory,
            ISproutDatePickerFactory sproutDatePickerFactory,
            ISproutLabelFactory sproutLabelFactory,
            ISproutListFactory sproutListFactory,
            ISproutTextBoxFactory sproutTextBoxFactory,
            IDataAdapterFactory dataAdapterFactory)
        {
            _dataAdapterFactory = dataAdapterFactory;
            _sproutButtonFactory = sproutButtonFactory;
            _gridFactory = gridFactory;
            _sproutBorderFactory = sproutBorderFactory;
            _sproutCheckBoxFactory = sproutCheckBoxFactory;
            _sproutComboFactory = sproutComboFactory;
            _sproutDataGridFactory = sproutDataGridFactory;
            _sproutDatePickerFactory = sproutDatePickerFactory;
            _sproutLabelFactory = sproutLabelFactory;
            _sproutListFactory = sproutListFactory;
            _sproutTextBoxFactory = sproutTextBoxFactory;
        }

        public UIElement GetControl(SproutControlConfig sControl, Dictionary<string, UIElement> controls)
        {
            var control = GetControlInternal(sControl, controls);

            if (control is ISproutControl sproutControl)
            {
                if (sproutControl.VM is IDataAdapterHost dataAdapterHost)
                {
                    if (sproutControl.Config is IDataAdapterConfigHost adapterConfigHost)
                    {
                        if (adapterConfigHost.DataAdapter != null)
                        {
                            //TODO: remove the creation of the DataAdapter from the control factories and let this code do the work.
                            dataAdapterHost.DataAdapter = _dataAdapterFactory.Create(adapterConfigHost.DataAdapter);
                        }
                    }
                }
            }

            RegisterControl(control, controls);

            return control;
        }

        private FrameworkElement GetControlInternal(SproutControlConfig sControl, Dictionary<string, UIElement> controls)
        {
            switch (sControl)
            {
                case GridConfig gridConfig:
                    {
                        var grid = _gridFactory.Create(gridConfig);

                        foreach (var childConfig in gridConfig.Children)
                        {
                            grid.Children.Add(GetControl(childConfig, controls));
                        }

                        return grid;
                    }
                case SproutBorderConfig sproutBorderConfig:
                    {
                        var sproutBorder = _sproutBorderFactory.Create(sproutBorderConfig);

                        if (sproutBorderConfig.Child != null)
                        {
                            sproutBorder.border.Child = GetControl(sproutBorderConfig.Child, controls);
                        }

                        return sproutBorder;
                    }
                case SproutListConfig sproutListConfig:
                    {
                        UIElement? itemTemplateRoot = null;

                        if (sproutListConfig.Child != null)
                        {
                            itemTemplateRoot = GetControl(sproutListConfig.Child, new Dictionary<string, UIElement>());
                        }

                        return _sproutListFactory.Create(sproutListConfig, itemTemplateRoot);
                    }
                case SproutButtonConfig sproutButtonConfig:
                    return _sproutButtonFactory.Create(sproutButtonConfig);
                case SproutDataGridConfig sproutDataGridConfig:
                    return _sproutDataGridFactory.Create(sproutDataGridConfig);
                case SproutComboConfig sproutComboConfig:
                    return _sproutComboFactory.Create(sproutComboConfig);
                case SproutTextBoxConfig sproutTextBoxConfig:
                    return _sproutTextBoxFactory.Create(sproutTextBoxConfig);
                case SproutCheckBoxConfig sproutCheckBoxConfig:
                    return _sproutCheckBoxFactory.Create(sproutCheckBoxConfig);
                case SproutDatePickerConfig sproutDatePickerConfig:
                    return _sproutDatePickerFactory.Create(sproutDatePickerConfig);
                case SproutLabelConfig sproutLabelConfig:
                    return _sproutLabelFactory.Create(sproutLabelConfig);
                default:
                    throw new NotImplementedException();
            }
        }

        private static FrameworkElement RegisterControl(FrameworkElement control, Dictionary<string, UIElement> controls)
        {
            if (string.IsNullOrWhiteSpace(control.Name))
            {
                var index = 1;
                var nameCandidate = $"{control.GetType().Name}{index}";

                while (controls.ContainsKey(nameCandidate))
                {
                    index++;
                    nameCandidate = $"{control.GetType().Name}{index}";
                }

                control.Name = nameCandidate;
            }

            controls[control.Name] = control;

            return control;
        }
    }
}
