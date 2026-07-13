using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
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
            ISproutTextBoxFactory sproutTextBoxFactory
            )
        {
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

        public UIElement GetControl(SproutControlConfig sControl,
            Dictionary<string, UIElement> controls)
        {
            switch (sControl)
            {
                case GridConfig gridConfig:
                {
                    var grid = RegisterControl(_gridFactory.Create(gridConfig), controls);

                    foreach (var childConfig in gridConfig.Children)
                    {
                        grid.Children.Add(GetControl(childConfig, controls));
                    }

                    return grid;
                }
                case SproutBorderConfig sproutBorderConfig:
                {
                    var sproutBorder = RegisterControl(_sproutBorderFactory.Create(sproutBorderConfig), controls);

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

                    return RegisterControl(_sproutListFactory.Create(sproutListConfig, itemTemplateRoot), controls);
                }
                case SproutButtonConfig sproutButtonConfig:
                    return RegisterControl(_sproutButtonFactory.Create(sproutButtonConfig), controls);
                case SproutDataGridConfig sproutDataGridConfig:
                    return RegisterControl(_sproutDataGridFactory.Create(sproutDataGridConfig), controls);
                case SproutComboConfig sproutComboConfig:
                    return RegisterControl(_sproutComboFactory.Create(sproutComboConfig), controls);
                case SproutTextBoxConfig sproutTextBoxConfig:
                    return RegisterControl(_sproutTextBoxFactory.Create(sproutTextBoxConfig), controls);
                case SproutCheckBoxConfig sproutCheckBoxConfig:
                    return RegisterControl(_sproutCheckBoxFactory.Create(sproutCheckBoxConfig), controls);
                case SproutDatePickerConfig sproutDatePickerConfig:
                    return RegisterControl(_sproutDatePickerFactory.Create(sproutDatePickerConfig), controls);
                case SproutLabelConfig sproutLabelConfig:
                    return RegisterControl(_sproutLabelFactory.Create(sproutLabelConfig), controls);
                default:
                    throw new NotImplementedException();
            }
        }

        private static T RegisterControl<T>(T control, Dictionary<string, UIElement> controls)
            where T : FrameworkElement
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
