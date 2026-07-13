using Sprout.Core.Models.Configurations;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public interface IGridFactory
    {
        Grid Create(GridConfig gridConfig);
    }
}