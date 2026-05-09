using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Sprout.Core.Models.Configurations.Duck;
using System;
using System.Xml;

namespace Sprout.Core.ViewModels
{
    public partial class DuckDataAdapterVM : ObservableObject
    {
        [ObservableProperty]
        private DuckDataAdapterConfig _dataAdapter;

        public TextDocument DataProviderDocument { get; } = new TextDocument();
        public TextDocument InsertDocument { get; } = new TextDocument();
        public TextDocument UpdateDocument { get; } = new TextDocument();
        public TextDocument DeleteDocument { get; } = new TextDocument();

        [ObservableProperty]
        private string _dataProviderText;

        public IHighlightingDefinition Highlighting { get; } = HighlightingManager.Instance.GetDefinition("SQL");

        public DuckDataAdapterVM(DuckDataAdapterConfig dataAdapter)
        {
            using var stream = GetType().Assembly
                        .GetManifestResourceStream("Sprout.Core.DuckDb.xshd");
                        //.GetManifestResourceStream("Sprout.Core.TSQL.xshd");

            using var reader = XmlReader.Create(stream!);
            Highlighting = HighlightingLoader.Load(
                reader,
                HighlightingManager.Instance);

            DataAdapter = dataAdapter;

            DataProviderDocument.Text = (DataAdapter.DataProvider as DuckDataProviderConfig).Text;

            DataProviderDocument.TextChanged += (_, __) =>
            {
                if ((DataAdapter.DataProvider as DuckDataProviderConfig).Text != DataProviderDocument.Text)
                    (DataAdapter.DataProvider as DuckDataProviderConfig).Text = DataProviderDocument.Text;
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