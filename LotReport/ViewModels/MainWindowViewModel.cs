using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotReport.Models;
using LotReport.Models.DirectoryItems;

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

        public AsyncCommand<object> LoadedCommand { get; private set; }

        private void WireCommands()
        {
        }
    }
}
