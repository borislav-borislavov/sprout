using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace Sprout.Core.ViewModels
{
    public partial class SqlServerDataAdapterVM : ObservableObject
    {
        [ObservableProperty]
        private SqlServerDataAdapterConfig _dataAdapter;

        public TextDocument DataProviderDocument { get; } = new TextDocument();
        public TextDocument InsertDocument { get; } = new TextDocument();
        public TextDocument UpdateDocument { get; } = new TextDocument();
        public TextDocument DeleteDocument { get; } = new TextDocument();

        [ObservableProperty]
        private string _dataProviderText;

        public IHighlightingDefinition Highlighting { get; } = HighlightingManager.Instance.GetDefinition("SQL");

        public SqlServerDataAdapterVM(SqlServerDataAdapterConfig dataAdapter)
        {
            using var stream = GetType().Assembly
                        .GetManifestResourceStream("Sprout.Core.TSQL.xshd");

            using var reader = XmlReader.Create(stream!);
            Highlighting = HighlightingLoader.Load(
                reader,
                HighlightingManager.Instance);

            //Highlighting = HighlightingManager.Instance.GetDefinition("Sprout.Core.TSQL.xshd");

            DataAdapter = dataAdapter;

            DataProviderDocument.Text = (DataAdapter.DataProvider as SqlServerDataProviderConfig).Text;

            DataProviderDocument.TextChanged += (_, __) =>
            {
                if ((DataAdapter.DataProvider as SqlServerDataProviderConfig).Text != DataProviderDocument.Text)
                    (DataAdapter.DataProvider as SqlServerDataProviderConfig).Text = DataProviderDocument.Text;
            };

            InsertDocument.Text = DataAdapter.Insert?.Text ?? string.Empty;
            InsertDocument.TextChanged += (_, __) =>
            {
                if (DataAdapter.Insert.Text != InsertDocument.Text)
                    DataAdapter.Insert.Text = InsertDocument.Text;
            };

            UpdateDocument.Text = DataAdapter.Update?.Text ?? string.Empty;
            UpdateDocument.TextChanged += (_, __) =>
            {
                if (DataAdapter.Update.Text != UpdateDocument.Text)
                    DataAdapter.Update.Text = UpdateDocument.Text;
            };

            DeleteDocument.Text = DataAdapter.Delete?.Text ?? string.Empty;
            DeleteDocument.TextChanged += (_, __) =>
            {
                if (DataAdapter.Delete.Text != DeleteDocument.Text)
                    DataAdapter.Delete.Text = DeleteDocument.Text;
            };
        }
    }
}
