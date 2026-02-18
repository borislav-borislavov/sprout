using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views
{
    /// <summary>
    /// Interaction logic for EditGridConfig.xaml
    /// </summary>
    public partial class EditGridConfig : UserControl
    {
        private ObservableCollection<StringWrapper> _rows = [];
        private ObservableCollection<StringWrapper> _columns = [];

        public EditGridConfig()
        {
            InitializeComponent();
            DataContextChanged += EditGridConfig_DataContextChanged;
        }

        private void EditGridConfig_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is GridConfig gridConfig)
            {
                _rows = new ObservableCollection<StringWrapper>(gridConfig.Rows.Select(r => new StringWrapper { Value = r }));
                _columns = new ObservableCollection<StringWrapper>(gridConfig.Columns.Select(c => new StringWrapper { Value = c }));

                _rows.CollectionChanged += (s, args) => SyncBack(gridConfig.Rows, _rows);
                _columns.CollectionChanged += (s, args) => SyncBack(gridConfig.Columns, _columns);

                RowsGrid.ItemsSource = _rows;
                ColumnsGrid.ItemsSource = _columns;
            }
        }

        private static void SyncBack(List<string> target, ObservableCollection<StringWrapper> source)
        {
            target.Clear();
            target.AddRange(source.Select(w => w.Value));
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            _rows.Add(new StringWrapper { Value = "1*" });
        }

        private void RemoveRow_Click(object sender, RoutedEventArgs e)
        {
            if (RowsGrid.SelectedItem is StringWrapper selected)
            {
                _rows.Remove(selected);
            }
        }

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            _columns.Add(new StringWrapper { Value = "1*" });
        }

        private void RemoveColumn_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnsGrid.SelectedItem is StringWrapper selected)
            {
                _columns.Remove(selected);
            }
        }

        public class StringWrapper
        {
            public string Value { get; set; } = string.Empty;
        }
    }
}
