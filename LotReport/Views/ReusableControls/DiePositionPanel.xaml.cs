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

namespace LotReport.Views.ReusableControls
{
    /// <summary>
    /// Interaction logic for DiePositionPanel.xaml
    /// </summary>
    public partial class DiePositionPanel : UserControl
    {
        public DiePositionPanel()
        {
            InitializeComponent();
        }

        // DependencyProperty for the Command
        public static readonly DependencyProperty DiePositionSelectedCommandProperty =
            DependencyProperty.Register(
                nameof(DiePositionSelectedCommand),
                typeof(ICommand),
                typeof(DiePositionPanel),
                new PropertyMetadata(null));

        public ICommand DiePositionSelectedCommand
        {
            get => (ICommand)GetValue(DiePositionSelectedCommandProperty);
            set => SetValue(DiePositionSelectedCommandProperty, value);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DiePositionSelectedCommand != null && DiePositionSelectedCommand.CanExecute(DataContext))
            {
                DiePositionSelectedCommand.Execute(DataContext);
            }
            else
            {
                var parentWindow = Window.GetWindow(this);

                if (parentWindow?.DataContext != null)
                {
                    var vm = parentWindow.DataContext;
                    var commandProperty = vm.GetType().GetProperty("LoadSelectedDieInspect");
                    var command = commandProperty?.GetValue(vm) as System.Windows.Input.ICommand;

                    if (command != null && command.CanExecute(DataContext))
                    {
                        command.Execute(DataContext);
                    }
                }
            }
        }
    }
}
