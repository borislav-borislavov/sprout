using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Queries;
using Sprout.Core.UIStates;

namespace Sprout.Core.ViewModels
{
    public partial class SproutPageVM : ObservableObject
    {
        public SproutPageConfiguration PageConfig { get; private set; }

        public Dictionary<string, Dictionary<string, GridAction>> GridActions { get; set; } = [];

        public Dictionary<string, Query> Queries { get; set; } = [];

        public UiStateRegistry UiStateRegistry { get; } = new();

        public SproutPageVM(SproutPageConfiguration pageConfig)
        {
            PageConfig = pageConfig;

            CreateQueries();

            UiStateRegistry.UiStateChanged += (_, change) =>
            {
                foreach (var query in Queries.Values)
                {
                    var dependencyHasChanged = false;

                    foreach (var dependency in query.Dependencies)
                    {
                        if (dependency.ControlName == change.ControlName)
                        {
                            dependencyHasChanged = true;

                            var debug = dependency.Value;
                        }
                    }

                    if (dependencyHasChanged)
                    {
                        QueryService.ExecuteQuery(query);
                    }
                }
            };
        }

        public void CreateQueries()
        {
            foreach (var queryConfig in PageConfig.Queries)
            {
                Queries[queryConfig.Name] = QueryService.CreateQuery(queryConfig);
            }
        }

        public void OnLoaded()
        {
#warning prevent execution of defined queries that are never used?
            foreach (var kvp in Queries)
            {
                //query dependencies have to be done after all controls are created and before all queries are executed
                QueryService.BindDependencies(kvp.Value, UiStateRegistry);
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
