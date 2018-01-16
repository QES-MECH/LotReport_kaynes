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

namespace LotReport.Views.MainWindowControls
{
    /// <summary>
    /// Interaction logic for LotListControl.xaml
    /// </summary>
    public partial class LotListControl : UserControl
    {
        public LotListControl()
        {
            InitializeComponent();
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            if (vm == null)
            {
                return;
            }

            TreeView treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }

            vm.GenerateMapCommand.Execute(treeView.SelectedItem);
        }
    }
}
