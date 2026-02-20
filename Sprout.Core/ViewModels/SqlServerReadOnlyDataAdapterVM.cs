using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Sprout.Core.Models.Configurations;
using System;
using System.Xml;

namespace Sprout.Core.ViewModels
{
    /// <summary>
    /// A read-only variant of the SQL Server data adapter view model.
    /// Only exposes connection string and query (no insert/update/delete commands).
    /// Used for ComboBox column data sources in the data grid.
    /// </summary>
    public partial class SqlServerReadOnlyDataAdapterVM : ObservableObject
    {
        [ObservableProperty]
        private SqlServerDataAdapterConfig _dataAdapter;

        public TextDocument DataProviderDocument { get; } = new TextDocument();

        public IHighlightingDefinition Highlighting { get; }

        public SqlServerReadOnlyDataAdapterVM(SqlServerDataAdapterConfig dataAdapter)
        {
            using var stream = GetType().Assembly
                        .GetManifestResourceStream("Sprout.Core.TSQL.xshd");

            using var reader = XmlReader.Create(stream!);
            Highlighting = HighlightingLoader.Load(
                reader,
                HighlightingManager.Instance);

            DataAdapter = dataAdapter;

            DataProviderDocument.Text = (DataAdapter.DataProvider as SqlServerDataProviderConfig)?.Text ?? string.Empty;

            DataProviderDocument.TextChanged += (_, __) =>
            {
                if ((DataAdapter.DataProvider as SqlServerDataProviderConfig)?.Text != DataProviderDocument.Text)
                    (DataAdapter.DataProvider as SqlServerDataProviderConfig).Text = DataProviderDocument.Text;
            };
        }
    }
}
