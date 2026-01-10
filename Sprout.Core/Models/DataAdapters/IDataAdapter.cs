using Sprout.Core.Models.DataAdapters.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters
{
	public interface IDataAdapter
	{
		public IDataProvider DataProvider { get; set; }

		public IEditCommand InsertCommand { get; set; }
		public IEditCommand UpdateCommand { get; set; }
		public IEditCommand DeleteCommand { get; set; }
	}
}
