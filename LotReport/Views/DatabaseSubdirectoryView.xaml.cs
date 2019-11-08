using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace LotReport.Views
{
    /// <summary>
    /// Interaction logic for DatabaseSubdirectoryView.xaml
    /// </summary>
    public partial class DatabaseSubdirectoryView : Window
    {
        public DatabaseSubdirectoryView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.DatabaseSubdirectoryViewModel vm)
            {
                vm.Close = () => Close();
            }
        }
    }
}
