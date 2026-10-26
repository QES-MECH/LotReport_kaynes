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
                var initialPath = vm.Current2DPaths?.FirstOrDefault();
                if (!string.IsNullOrEmpty(initialPath))
                {
                    vm.SelectedImagePath = initialPath;
                    vm.LoadSelectedDieInspect.Execute(initialPath);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is DiePositionViewerViewModel vm)
            {
                vm.CognexDisplayViewModel?.Dispose();
                vm.CognexVmLeft?.Dispose();
                vm.CognexVmRight?.Dispose();
                vm.CognexVmFront?.Dispose();
                vm.CognexVmBack?.Dispose();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void On3DSideClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string path && DataContext is DiePositionViewerViewModel vm)
            {
                vm.Selected3DSideCommand.Execute(path);
            }
        }
    }
}
