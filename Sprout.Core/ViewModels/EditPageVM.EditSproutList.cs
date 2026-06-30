using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using System;
#nullable disable

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        [ObservableProperty]
        private SproutListConfig _selectedList;

        [ObservableProperty]
        private SproutListPageLink _selectedListPage;

        [ObservableProperty]
        private SproutPageConfiguration _pageToAdd;

        [RelayCommand]
        private void AddListPage()
        {
            if (SelectedList == null || PageToAdd == null || PageToAdd.ID == Guid.Empty)
            {
                return;
            }

            SelectedList.Pages.Add(new SproutListPageLink
            {
                PageId = PageToAdd.ID,
                Title = PageToAdd.Title
            });
        }

        [RelayCommand]
        private void RemoveListPage()
        {
            if (SelectedList == null || SelectedListPage == null)
            {
                return;
            }

            SelectedList.Pages.Remove(SelectedListPage);
        }

        [RelayCommand]
        private void MoveListPageUp()
        {
            if (SelectedList == null || SelectedListPage == null)
            {
                return;
            }

            var index = SelectedList.Pages.IndexOf(SelectedListPage);
            if (index <= 0)
            {
                return;
            }

            SelectedList.Pages.Move(index, index - 1);
        }

        [RelayCommand]
        private void MoveListPageDown()
        {
            if (SelectedList == null || SelectedListPage == null)
            {
                return;
            }

            var index = SelectedList.Pages.IndexOf(SelectedListPage);
            if (index < 0 || index >= SelectedList.Pages.Count - 1)
            {
                return;
            }

            SelectedList.Pages.Move(index, index + 1);
        }
    }
}
