using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.UIStates
{
    public class SproutPageUIState : BaseUIState
    {
        //private Dictionary<string, object> _data = [];

        //public object this[string key]
        //{
        //    get { return _data[key]; }
        //    set { _data[key] = value; }
        //}.

        public object Data { get; set; }
    }
}
