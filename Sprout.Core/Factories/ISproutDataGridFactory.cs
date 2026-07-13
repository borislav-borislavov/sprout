using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutDataGridFactory
    {
        SproutDataGrid Create(SproutDataGridConfig config);
    }
}
