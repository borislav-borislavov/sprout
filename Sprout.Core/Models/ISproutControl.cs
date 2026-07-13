using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Models
{
    internal interface ISproutControl<C, V> where C : SproutControlConfig where V : BaseSproutControlVM
    {
        C Config { get; set; }

        SproutControlType ControlType { get; }

        V VM { get; }
    }
}
