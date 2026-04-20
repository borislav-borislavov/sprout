using Sprout.Core.Models.DataAdapters;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.Login
{
    public class LoginResult
    {
        public string? ErrorMessage { get; set; }
        public bool Result { get; set; }
        public IDataAdapter? LoginDataAdapter { get; set; }
    }
}
