using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.SproutControlVMs;
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
    /// A contract for a data service that manages data operations and VM integration.
    /// </summary>
    /// <remarks>Implementations of this interface provide methods for inserting, updating, and deleting data
    /// rows. The interface also supports binding dependencies between
    /// data providers and VM, facilitating synchronization between data and user interface components. Consumers
    /// should ensure proper disposal of resources by calling Dispose when the service is no longer needed.</remarks>
    public interface IDataService : IDisposable
    {
        VMRegistry VMRegistry { get; }

        Task ProvideData();

        Task<ChangeResult> Insert(DataRow dataRow);
        Task<ChangeResult> Update(DataRow dataRow);
        Task<ChangeResult> Delete(DataRow dataRow);
        Task<ChangeResult> Change(IEditCommand editCmd, DataRow dataRow);
    }
}
