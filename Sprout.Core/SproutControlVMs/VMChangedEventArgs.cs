using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.SproutControlVMs
{
    public sealed record VMChangedEventArgs
    {
        /// <summary>
        /// The name of the ViewModel that changed.
        /// </summary>
        public string ControlName { get; set; }
        public string PropertyName { get; set; }

        public VMChangedEventArgs(object control, string propertyName)
        {
            ControlName = (control as BaseSproutControlVM).Name;
            PropertyName = propertyName;
        }
    }
}
