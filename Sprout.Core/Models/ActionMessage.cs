using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models
{
    public record struct ActionMessage(string Type, string Message);
}
