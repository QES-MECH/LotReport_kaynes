using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotReport.Models;
using LotReport.Models.DirectoryItems;
using LotReport.Views;

namespace LotReport.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private string _status;
        private LeadFrameTable _leadFrameMap;
        private List<Item> _directoryItems;

        public MainWindowViewModel()
        {
            this.WireCommands();
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

        public WindowService SettingsWindow { get; private set; } = new WindowService();

        public AsyncCommand<object> LoadedCommand { get; private set; }

        public RelayCommand SettingsCommand { get; private set; }

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

                        try
                        {
                            DirectoryItems = DirectoryProvider.GetItems(Settings.DatabaseDirectory);
                        }
                        catch (Exception ex)
                        {
                            Status = string.Format("Failed to load Settings. Error: {0}", ex.Message);
                        }
                    });
                });

            SettingsCommand = new RelayCommand(
                param => SettingsWindow.ShowDialog<SettingsView>(new SettingsViewModel()));
        }
    }
}
