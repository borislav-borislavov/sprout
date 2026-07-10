using Sprout.Core.Models.DataAdapters.Filters;
using System.Data;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
    public interface IDataProvider : IDependent 
	{
		IDataAdapter Parent { get; }

		DataTable Data { get; set; }

        string Text { get; set; }

		Dictionary<string, IFilter> Filters { get; set; }

		bool DeferInitialLoad { get; set; }
	}
}
