using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
	public interface IDataProvider
	{
		IDataAdapter Parent { get; }

		DataTable Data { get; set; }

		IEnumerable<DataProviderDependency> Dependencies { get; /*set;*/ }

		Dictionary<string, IFilter> Filters { get; set; }
    }
}
