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
        private bool _markGraphics;
        private string _visionImageDirectory;
        private string _sftpHost;
        private ushort _sftpPort;
        private string _sftpUsername;
        private string _sftpPassword;
        private string _sftpPrivateKeyFileName;
        private string _sftpDirectory;

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

        public bool MarkGraphics { get => _markGraphics; set => SetProperty(ref _markGraphics, value); }

        public string VisionImageDirectory { get => _visionImageDirectory; set => SetProperty(ref _visionImageDirectory, value); }

        public string SftpHost { get => _sftpHost; set => SetProperty(ref _sftpHost, value); }

        public ushort SftpPort { get => _sftpPort; set => SetProperty(ref _sftpPort, value); }

        public string SftpUsername { get => _sftpUsername; set => SetProperty(ref _sftpUsername, value); }

        public string SftpPassword { get => _sftpPassword; set => SetProperty(ref _sftpPassword, value); }

        public string SftpPrivateKeyFileName { get => _sftpPrivateKeyFileName; set => SetProperty(ref _sftpPrivateKeyFileName, value); }

        public string SftpDirectory { get => _sftpDirectory; set => SetProperty(ref _sftpDirectory, value); }

        public RelayCommand<bool> OkCommand { get; private set; }

        public RelayCommand DatabaseFolderCommand { get; private set; }

        public RelayCommand BinCodeFileCommand { get; private set; }

        public RelayCommand VisionImageFolderCommand { get; private set; }

        public RelayCommand PrivateKeyFileCommand { get; private set; }

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

            PrivateKeyFileCommand = new RelayCommand(
                param =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Title = "Open Private Key File";
                    bool? result = fileDialog.ShowDialog();

                    if (result == true)
                    {
                        SftpPrivateKeyFileName = fileDialog.FileName;
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
            MarkGraphics = Settings.MarkGraphics;
            VisionImageDirectory = Settings.VisionImageDirectory;
            SftpHost = Settings.SftpHost;
            SftpPort = Settings.SftpPort;
            SftpUsername = Settings.SftpUsername;
            SftpPassword = Settings.SftpPassword;
            SftpPrivateKeyFileName = Settings.SftpPrivateKeyFileName;
            SftpDirectory = Settings.SftpDirectory;
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
            Settings.MarkGraphics = MarkGraphics;
            Settings.VisionImageDirectory = VisionImageDirectory;
            Settings.SftpHost = SftpHost;
            Settings.SftpPort = SftpPort;
            Settings.SftpUsername = SftpUsername;
            Settings.SftpPassword = SftpPassword;
            Settings.SftpPrivateKeyFileName = SftpPrivateKeyFileName;
            Settings.SftpDirectory = SftpDirectory;

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
