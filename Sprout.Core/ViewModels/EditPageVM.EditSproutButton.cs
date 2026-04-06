using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        [ObservableProperty]
        private SproutButtonConfig _selectedButton;

        [ObservableProperty]
        private SproutButtonActionConfig _selectedButtonAction;

        public string[] AvailableActionTypes { get; } = [nameof(ExecuteUpdateActionConfig), nameof(RefreshDataGridActionConfig)];

        [ObservableProperty]
        private string _selectedActionType;

        [RelayCommand]
        private void AddButtonAction()
        {
            try
            {
                if (SelectedButton == null) return;

                if (string.IsNullOrWhiteSpace(SelectedActionType)) return;

                SproutButtonActionConfig newAction = SelectedActionType switch
                {
                    nameof(ExecuteUpdateActionConfig) => new ExecuteUpdateActionConfig(),
                    nameof(RefreshDataGridActionConfig) => new RefreshDataGridActionConfig(),
                    _ => throw new NotImplementedException($"Action type '{SelectedActionType}' is not implemented.")
                };

                SelectedButton.Actions.Add(newAction);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void RemoveButtonAction()
        {
            try
            {
                if (SelectedButton == null || SelectedButtonAction == null) return;

                SelectedButton.Actions.Remove(SelectedButtonAction);
                SelectedButtonAction = null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void MoveButtonActionUp()
        {
            if (SelectedButton == null || SelectedButtonAction == null) return;

            var index = SelectedButton.Actions.IndexOf(SelectedButtonAction);
            if (index <= 0) return;

            SelectedButton.Actions.Move(index, index - 1);
        }

        [RelayCommand]
        private void MoveButtonActionDown()
        {
            if (SelectedButton == null || SelectedButtonAction == null) return;

            var index = SelectedButton.Actions.IndexOf(SelectedButtonAction);
            if (index < 0 || index >= SelectedButton.Actions.Count - 1) return;

            SelectedButton.Actions.Move(index, index + 1);
        }

        [RelayCommand]
        private void InitializeButtonDataAdapter()
        {
            try
            {
                if (SelectedButton == null) return;

                if (SelectedButton.DataAdapter != null) return;

                SelectedButton.DataAdapter = new SqlServerDataAdapterConfig
                {
                    ConnectionString = string.Empty,
                    DataProvider = new SqlServerDataProviderConfig
                    {
                        Text = string.Empty
                    },
                    UpdateCommand = new SqlServerEditCommandConfig(),
                };

                OnSelectedNodeChanged(SelectedButton);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
