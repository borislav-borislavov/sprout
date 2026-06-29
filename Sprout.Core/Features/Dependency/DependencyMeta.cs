using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Models.Queries
{
    public class DependencyMeta
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
        public bool IsMandatory { get; set; }
        public string RawPatameter { get; internal set; }
        public bool IsFromUIState { get; internal set; }
    }
}
