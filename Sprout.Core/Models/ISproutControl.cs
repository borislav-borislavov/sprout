using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Models
{
    internal interface ISproutControl
    {
        SproutControlType ControlType { get; }

        BaseSproutControlVM VM { get; }

        SproutControlConfig Config { get; }
    }

    internal interface ISproutControl<C, V> : ISproutControl where C : SproutControlConfig where V : BaseSproutControlVM
    {
        new C Config { get; set; }

        /// <summary>
        /// The new keyword explicitly shadows (hides) the VM property from the parent interface. Instead of returning the generic BaseSproutControlVM, it returns the specific generic type V (which is constrained to where V : BaseSproutControlVM).
        /// Why it's there: When you are working with a specific control implementation (e.g., ISproutControl<TextBoxConfig, TextBoxVM>), you don't want to constantly cast VM from the base class to TextBoxVM just to access TextBox-specific properties. This ensures compile-time type safety.
        /// </summary>
        new V VM { get; }

        /// <summary>
        /// Because the generic interface shadows the base property (new V VM),
        /// C# needs to know what to do if someone casts your specific class back down to the non-generic ISproutControl.
        /// If you access the property via ISproutControl<C, V>, you get the specific, strongly-typed V.
        /// If you access the property via the base ISproutControl, it calls this explicit implementation under the hood and safely returns the same object cast as BaseSproutControlVM
        /// </summary>
        BaseSproutControlVM ISproutControl.VM => VM;

        SproutControlConfig ISproutControl.Config => Config;
    }
}
