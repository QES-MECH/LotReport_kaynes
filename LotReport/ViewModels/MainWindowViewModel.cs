using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LotReport.Models;
using LotReport.Models.DirectoryItems;
using LotReport.Views;

namespace LotReport.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private string _status;
        private LeadFrameTable _leadFrameMap;
        private List<FolderItem> _lots;
        private FolderItem _selectedLot;
        private List<Item> _directoryItems;
        private LotData _lotData;

        public MainWindowViewModel()
        {
            LeadFrameMap = LeadFrameTable.LoadTemplate(25, 5);
            WireCommands();
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public LeadFrameTable LeadFrameMap
        {
            get
            {
                return _leadFrameMap;
            }

            set
            {
                _leadFrameMap = value;
                OnPropertyChanged();
            }
        }

        public List<FolderItem> Lots
        {
            get
            {
                return _lots;
            }

            set
            {
                _lots = value;
                OnPropertyChanged();
            }
        }

        public FolderItem SelectedLot
        {
            get
            {
                return _selectedLot;
            }

            set
            {
                _selectedLot = value;
                OnPropertyChanged();
                UpdateSelectedLot(value);
            }
        }

        public List<Item> DirectoryItems
        {
            get
            {
                return _directoryItems;
            }

            set
            {
                _directoryItems = value;
                OnPropertyChanged();
            }
        }

        public LotData LotData
        {
            get
            {
                return _lotData;
            }

            set
            {
                _lotData = value;
                OnPropertyChanged();
            }
        }

        public WindowService SettingsWindow { get; private set; } = new WindowService();

        public WindowService DieWindow { get; private set; } = new WindowService();

        public AsyncCommand<object> LoadedCommand { get; private set; }

        public RelayCommand RefreshLotsCommand { get; private set; }

        public RelayCommand SettingsCommand { get; private set; }

        public RelayCommand GenerateMapCommand { get; private set; }

        public RelayCommand DieCommand { get; private set; }

        private void WireCommands()
        {
            LoadedCommand = AsyncCommand.Create(
                (token, param) =>
                {
                    return Task.Run(() =>
                    {
                        try
                        {
                            Settings.LoadFromFile();
                        }
                        catch (Exception ex)
                        {
                            Status = string.Format("Failed to load Settings. Error: {0}", ex.Message);
                        }

                        Application.Current.Dispatcher.Invoke(() => RefreshLotsCommand.Execute(null));
                    });
                });

            RefreshLotsCommand = new RelayCommand(
                param =>
                {
                    try
                    {
                        using (WaitCursor waitCursor = new WaitCursor())
                        {
                            Lots = DirectoryProvider.GetFolderItems(Settings.DatabaseDirectory);
                        }
                    }
                    catch (Exception ex)
                    {
                        Status = string.Format("Failed to load Lots. Error: {0}", ex.Message);
                    }
                });

            SettingsCommand = new RelayCommand(
                param => SettingsWindow.ShowDialog<SettingsView>(new SettingsViewModel()));

            GenerateMapCommand = new RelayCommand(
                param =>
                {
                    FileItem file = param as FileItem;
                    if (file == null)
                    {
                        return;
                    }

                    try
                    {
                        LeadFrameMap = LeadFrameTable.Load(file.Path);
                    }
                    catch (Exception ex)
                    {
                        Status = string.Format("Failed to load Lead Frame Map. Error: {0}", ex.Message);
                    }
                });

            DieCommand = new RelayCommand(
                param =>
                {
                    Die selectedDie = param as Die;

                    if (selectedDie == null)
                    {
                        return;
                    }

                    DieViewModel vm = new DieViewModel();
                    vm.Die = selectedDie;

                    this.DieWindow.ShowDialog<DieView>(vm);
                });
        }

        private void UpdateSelectedLot(FolderItem lot)
        {
            try
            {
                DirectoryItems = DirectoryProvider.GetItems(lot.Path);

                string lotDataFile = lot.Name + ".xml";
                Item lotFile = DirectoryItems.FirstOrDefault(item => item.Name == lotDataFile);

                LotData currentLotData = new LotData();
                currentLotData.LoadFromFile(lotFile.Path);

                LotData = currentLotData;
            }
            catch (Exception ex)
            {
                Status = string.Format("Failed to load Lot ID: {0}. Error: {1}", lot.Name, ex.Message);
            }
        }

        private class WaitCursor : IDisposable
        {
            private Cursor _defaultCursor;

            public WaitCursor()
            {
                _defaultCursor = Mouse.OverrideCursor;

                Mouse.OverrideCursor = Cursors.Wait;
            }

            public void Dispose()
            {
                Mouse.OverrideCursor = _defaultCursor;
            }
        }
    }
}
