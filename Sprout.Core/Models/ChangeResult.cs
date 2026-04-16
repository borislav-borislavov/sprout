using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models
{
    public class ChangeResult
    {
        public List<ActionMessage> Messages { get; internal set; } = [];
        public bool? Result { get; internal set; }
        public DataTable? ExtraData { get; internal set; }
    }
}
