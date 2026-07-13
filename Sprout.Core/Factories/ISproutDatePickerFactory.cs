using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutDatePickerFactory
    {
        SproutDatePicker Create(SproutDatePickerConfig config);
    }
}
