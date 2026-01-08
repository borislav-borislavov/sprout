using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Messages;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    internal partial class AddControlVM : ObservableObject
    {
        public SproutControlConfig NewControl { get; set; }

        public string[] AvailableControls { get; set; }

        [NotifyCanExecuteChangedFor(nameof(AddControlCommand))]
        [ObservableProperty]
        private string _selectedControlType;

        [NotifyCanExecuteChangedFor(nameof(AddControlCommand))]
        [ObservableProperty]
        private string _controlName;

        public AddControlVM()
        {
            AvailableControls = Enum.GetNames(typeof(SproutControlType));
        }

        [RelayCommand(CanExecute = nameof(CanAddControl))]
        private void AddControl()
        {
            var result = Enum.TryParse<SproutControlType>(SelectedControlType, out var controlType);

            if (result == false)
            {
                throw new InvalidOperationException(
                    "Selected control type is not handled yet.");
            }

            switch (controlType)
            {
                case SproutControlType.Grid:
                    NewControl = new GridConfig()
                    {
                        Name = ControlName
                    };
                    break;
                case SproutControlType.DataGrid:
                    NewControl = new SproutDataGridConfig()
                    {
                        Name = ControlName
                    };
                    break;
                case SproutControlType.ComboBox:
                    NewControl = new SproutComboConfig()
                    {
                        Name = ControlName
                    };
                    break;
                case SproutControlType.Button:
                    NewControl = new ButtonConfig()
                    {
                        Name = ControlName
                    };
                    break;
                default:
                    throw new NotImplementedException(
                        $"Control type '{controlType}' is not implemented yet.");
            }

            WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
        }

        private bool CanAddControl()
        {
            return !string.IsNullOrWhiteSpace(ControlName) && !string.IsNullOrWhiteSpace(SelectedControlType);
        }
    }
}
