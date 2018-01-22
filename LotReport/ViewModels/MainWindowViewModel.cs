using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using LotReport.Models;
using LotReport.Models.DirectoryItems;
using LotReport.Views;

namespace LotReport.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private List<LotData> _lotDataSource = new List<LotData>();

        private string _status;
        private LeadFrameMap _leadFrameMapOperator;
        private LeadFrameMap _leadFrameMapMachine;
        private LotData _selectedLot;
        private List<Item> _selectedLotDirectory;
        private int _selectedTabIndex;

        public MainWindowViewModel()
        {
            LotDataView = CollectionViewSource.GetDefaultView(_lotDataSource);
            WireCommands();
        }

        public enum TabIndex
        {
            Lot,
            Map
        }

        public ICollectionView LotDataView { get; private set; }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public LeadFrameMap LeadFrameMapOperator
        {
            get => _leadFrameMapOperator;
            set => SetProperty(ref _leadFrameMapOperator, value);
        }

        public LeadFrameMap LeadFrameMapMachine
        {
            get => _leadFrameMapMachine;
            set => SetProperty(ref _leadFrameMapMachine, value);
        }

        public LotData SelectedLot
        {
            get => _selectedLot;
            set
            {
                SetProperty(ref _selectedLot, value);
                UpdateSelectedLot(value);
            }
        }

        public List<Item> SelectedLotDirectory
        {
            get => _selectedLotDirectory;
            set => SetProperty(ref _selectedLotDirectory, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
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
                            _lotDataSource.Clear();

                            string[] lotFiles = Directory.GetFiles(Settings.DatabaseDirectory, "*.lot", SearchOption.AllDirectories);

                            foreach (string lotFile in lotFiles)
                            {
                                LotData lotData = new LotData();
                                lotData.LoadFromFile(lotFile);
                                lotData.GenerateSummary();
                                _lotDataSource.Add(lotData);
                                LotDataView.Refresh();
                            }
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
                        LeadFrameMapOperator = LeadFrameMap.Load(file.Path, LeadFrameMap.Type.Operator);
                        var die = LeadFrameMapOperator.Dies.FirstOrDefault(d => d.Coordinate == new Point(1, 1));
                        LeadFrameMapOperator.SetDieMarkStatus(die, Die.Mark.Pass);
                        LeadFrameMapMachine = LeadFrameMap.Load(file.Path, LeadFrameMap.Type.Machine);
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

        private void UpdateSelectedLot(LotData lot)
        {
            try
            {
                List<Item> selectedLotDirectory = DirectoryProvider.GetItems(lot.LotFile.Directory.FullName);
                Item lotFile = selectedLotDirectory.FirstOrDefault(item => item.Name.Contains(".lot"));
                selectedLotDirectory.Remove(lotFile);
                SelectedLotDirectory = selectedLotDirectory;

                LeadFrameMapOperator = LeadFrameMap.LoadTemplate(lot.LeadFrameXUnits, lot.LeadFrameYUnits);
                LeadFrameMapMachine = LeadFrameMap.LoadTemplate(lot.LeadFrameXUnits, lot.LeadFrameYUnits);
            }
            catch (Exception ex)
            {
                Status = string.Format("Failed to load Lot ID: {0}. Error: {1}", lot.LotFile.Directory.FullName, ex.Message);
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
