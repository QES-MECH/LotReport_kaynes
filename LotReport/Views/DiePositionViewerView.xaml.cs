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
using LotReport.ViewModels;

namespace LotReport.Views
{
    /// <summary>
    /// Interaction logic for DiePositionViewerView.xaml
    /// </summary>
    public partial class DiePositionViewerView : Window
    {
        public DiePositionViewerView()
        {
            InitializeComponent();
        }

        private void Window_loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DiePositionViewerViewModel vm)
            {
                vm.SelectedImagePath = vm.CombinedImagePaths[0];
                vm.LoadSelectedDieInspect.Execute(DataContext);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is DiePositionViewerViewModel vm)
            {
                vm.CognexDisplayViewModel?.Dispose();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
