using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
	/// <summary>
	/// Controls which inherit this mean they can perform CRUD operations.
	/// Some of them will have only read access, some will have full CRUD access.
	/// </summary>
	public interface IDataAdapterConfig
	{
		IDataProviderConfig DataProvider { get; set; }

		IEditCommandConfig InsertCommand { get; set; }
		IEditCommandConfig UpdateCommand { get; set; }
		IEditCommandConfig DeleteCommand { get; set; }
	}

	public interface IEditCommandConfig { }
}
