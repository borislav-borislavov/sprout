using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for ManageDataAdapter.xaml
    /// </summary>
    public partial class ManageDataAdapterWindow : Window
    {
        public ManageDataAdapterWindow(ManageDataAdapterVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
