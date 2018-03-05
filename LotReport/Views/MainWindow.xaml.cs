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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LotReport.ViewModels;

namespace LotReport.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = DataContext as MainWindowViewModel;
            if (vm == null)
            {
                return;
            }

            await vm.LoadedCommand.ExecuteAsync(null);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (DataContext is MainWindowViewModel vm)
            {
                if (dataGrid.SelectedItem != null)
                {
                    vm.UpdateSelectedLotCommand.Execute(vm.SelectedLot);
                    e.Handled = true;
                }
            }
        }
    }
}
