using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Queries;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.ViewModels
{
    public partial class SproutPageVM : ObservableObject
    {
        private SproutPageConfiguration _pageConfig;

        public Dictionary<string, Dictionary<string, GridAction>> GridActions { get; set; } = [];

        public Dictionary<string, Query> Queries { get; set; } = [];

        public UiStateRegistry UiStateRegistry { get; } = new();

        public SproutPageVM(SproutPageConfiguration pageConfig)
        {
            _pageConfig = pageConfig;

            CreateQueries();

            UiStateRegistry.UiStateChanged += (_, change) =>
            {
                //var affectedQueries = _dependencyGraph.Resolve(change);
                //foreach (var query in affectedQueries)
                //    ExecuteQuery(query);

                foreach (var query in Queries.Values)
                {
                    var dependencyHasChanged = false;

                    foreach (var dependency in query.Dependencies)
                    {
                        if (dependency.ControlName == change.ControlName
                            /*&& dependency.PropertyName == change.PropertyName*/)
                        {

                            var x = UiStateRegistry.Get<BaseUIState>(dependency.ControlName);
                            var propInfo = x.GetType().GetProperty(change.PropertyName);
                            //propInfo.SetValue(x, );

                            if (dependency.PropertyName == nameof(SproutGridUIState.Selected))
                            {
                                var dataRowView = propInfo.GetValue(x) as DataRowView;
                                dependency.Value = dataRowView[dependency.Extra[0]];
                            }
                            else
                            {
                                dependency.Value = propInfo.GetValue(x);
                            }

                            

                            dependencyHasChanged = true;
                        }
                    }

                    if (dependencyHasChanged)
                    {
                        QueryService.ExecuteQuery(query);
                    }
                }
            };
        }

        private void CreateQueries()
        {
            foreach (var queryConfig in _pageConfig.Queries)
            {
                var query = QueryService.CreateQuery(queryConfig);
                Queries[queryConfig.Name] = query;
            }
        }

        private void BindQueryDependencies()
        {
            foreach (var query in Queries.Values)
            {
                foreach (var dependency in query.Dependencies)
                {
                    var uiState = UiStateRegistry[dependency.ControlName];

#warning the big question here is should i bind to SelectedRow.UserID or interpret every time the UiStateChanged event is fired



                    if (uiState != null)
                    {
                        //uiState.PropertyChanged += (_, e) =>
                        //{
                        //    if (e.PropertyName == dependency.PropertyName)
                        //    {
                        //        ExecuteQuery(query);
                        //    }
                        //};
                    }
                }
            }
        }

        public void OnLoaded()
        {
            foreach (var kvp in Queries)
            {
                QueryService.ExecuteQuery(kvp.Value);
            }
        }

        [RelayCommand]
        private void PerformAction(object parameter)
        {
            if (parameter is GridAction gridAction)
            {
                QueryService.ExecuteQueryAction(gridAction, Queries);
            }
        }
    }
}
