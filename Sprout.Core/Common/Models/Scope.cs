using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Common.Models
{
    public class Scope
    {
        public static implicit operator string(Scope s) => s.Content;

        private readonly string _text;

        public Scope(string text)
        {
            _text = text;
        }

        public int OpenIdx { get; internal set; }
        public int CloseIdx { get; internal set; }
        public string Content { get; internal set; }

        public override string ToString()
        {
            return Content;
        }
    }
}
