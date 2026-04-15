using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.DataProviders
{
    /// <summary>
    /// A contract for a data service that manages data operations and UI state integration.
    /// </summary>
    /// <remarks>Implementations of this interface provide methods for inserting, updating, and deleting data
    /// rows. The interface also supports binding dependencies between
    /// data providers and UI state, facilitating synchronization between data and user interface components. Consumers
    /// should ensure proper disposal of resources by calling Dispose when the service is no longer needed.</remarks>
    public interface IDataService : IDisposable
    {
        UiStateRegistry UiStateRegistry { get; }

        Task ProvideData();

        Task<IEnumerable<ActionMessage>> Insert(DataRow dataRow);
        Task<IEnumerable<ActionMessage>> Update(DataRow dataRow);
        Task<IEnumerable<ActionMessage>> Delete(DataRow dataRow);
    }
}
