using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
	public interface IDataAdapterConfigHost
	{
		IDataAdapterConfig DataAdapter { get; set; }
	}
}
