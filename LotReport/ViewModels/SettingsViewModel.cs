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
        private bool _shiftFilter;
        private DateTime _dayShift;
        private DateTime _nightShift;
        private bool _cognexDisplay;
        private string _visionImageDirectory;

        public SettingsViewModel()
        {
            this.LoadData();
            this.WireCommands();
        }

        public string DatabaseDirectory { get => _databaseDirectory; set => SetProperty(ref _databaseDirectory, value); }

        public string BinCodeDirectory { get => _binCodeDirectory; set => SetProperty(ref _binCodeDirectory, value); }

        public bool ShiftFilter { get => _shiftFilter; set => SetProperty(ref _shiftFilter, value); }

        public DateTime DayShift { get => _dayShift; set => SetProperty(ref _dayShift, value); }

        public DateTime NightShift { get => _nightShift; set => SetProperty(ref _nightShift, value); }

        public bool CognexDisplay { get => _cognexDisplay; set => SetProperty(ref _cognexDisplay, value); }

        public string VisionImageDirectory { get => _visionImageDirectory; set => SetProperty(ref _visionImageDirectory, value); }

        public RelayCommand<bool> OkCommand { get; private set; }

        public RelayCommand DatabaseFolderCommand { get; private set; }

        public RelayCommand BinCodeFileCommand { get; private set; }

        public RelayCommand VisionImageFolderCommand { get; private set; }

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

            VisionImageFolderCommand = new RelayCommand(
                param =>
                {
                    using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        folderDialog.Description = "Browse for Vision Image Folder";
                        folderDialog.RootFolder = Environment.SpecialFolder.Desktop;
                        folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var result = folderDialog.ShowDialog();

                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            VisionImageDirectory = folderDialog.SelectedPath;
                        }
                    }
                });
        }

        private void LoadData()
        {
            DatabaseDirectory = Settings.DatabaseDirectory;
            BinCodeDirectory = Settings.BinCodeDirectory;
            ShiftFilter = Settings.ShiftFilter;
            DayShift = Settings.DayShift;
            NightShift = Settings.NightShift;
            CognexDisplay = Settings.CognexDisplay;
            VisionImageDirectory = Settings.VisionImageDirectory;
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
            Settings.ShiftFilter = ShiftFilter;
            Settings.DayShift = DayShift;
            Settings.NightShift = NightShift;
            Settings.CognexDisplay = CognexDisplay;
            Settings.VisionImageDirectory = VisionImageDirectory;

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
