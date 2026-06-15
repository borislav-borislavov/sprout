using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Models
{
    internal interface ISproutControl<T> where T : SproutControlConfig
    {
        T Config { get; set; }

        SproutControlType ControlType { get; }
    }
}
