﻿using System;
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
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = DataContext as SettingsViewModel;
            SftpPassword.Password = vm?.SftpPassword;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = DataContext as SettingsViewModel;
            if (vm == null)
            {
                return;
            }

            vm.SftpPassword = SftpPassword.Password;

            vm.OkCommand.Execute(null);

            if (vm.OkCommand.Result)
            {
                Close();
            }
        }
    }
}
