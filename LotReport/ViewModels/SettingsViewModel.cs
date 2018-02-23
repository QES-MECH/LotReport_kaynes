using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Framework.MVVM;
using LotReport.Models;
using Microsoft.Win32;

namespace LotReport.ViewModels
{
    public class SettingsViewModel : PropertyChangedBase
    {
        private string _databaseDirectory;
        private string _binCodeDirectory;

        public SettingsViewModel()
        {
            this.LoadData();
            this.WireCommands();
        }

        public string DatabaseDirectory { get => _databaseDirectory; set => SetProperty(ref _databaseDirectory, value); }

        public string BinCodeDirectory { get => _binCodeDirectory; set => SetProperty(ref _binCodeDirectory, value); }

        public RelayCommand<bool> OkCommand { get; private set; }

        public RelayCommand DatabaseFolderCommand { get; private set; }

        public RelayCommand BinCodeFileCommand { get; private set; }

        private void WireCommands()
        {
            OkCommand = new RelayCommand<bool>(
                param => SaveData());

            DatabaseFolderCommand = new RelayCommand(
                param =>
                {
                    using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        folderDialog.Description = "Browse for Database Folder";
                        folderDialog.RootFolder = Environment.SpecialFolder.Desktop;
                        folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var result = folderDialog.ShowDialog();

                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            DatabaseDirectory = folderDialog.SelectedPath;
                        }
                    }
                });

            BinCodeFileCommand = new RelayCommand(
                param =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Title = "Open BinCode File";
                    bool? result = fileDialog.ShowDialog();

                    if (result == true)
                    {
                        BinCodeDirectory = fileDialog.FileName;
                    }
                });
        }

        private void LoadData()
        {
            DatabaseDirectory = Settings.DatabaseDirectory;
            BinCodeDirectory = Settings.BinCodeDirectory;
        }

        private bool SaveData()
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to save?",
                "Save Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return false;
            }

            Settings.DatabaseDirectory = DatabaseDirectory;
            Settings.BinCodeDirectory = BinCodeDirectory;

            try
            {
                Settings.SaveToFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Failed to save Settings. Error: {0}", ex.Message),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }
    }
}
