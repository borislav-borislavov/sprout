using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class SqlServerDataAdapterVM : ObservableObject
    {
        [ObservableProperty]
        private SqlServerDataAdapterConfig _dataAdapter;

        public SqlServerDataAdapterVM(SqlServerDataAdapterConfig dataAdapter)
        {
            DataAdapter = dataAdapter;
        }
    }
}
