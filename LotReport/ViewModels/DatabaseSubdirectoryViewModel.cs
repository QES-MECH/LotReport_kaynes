using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.MVVM;

namespace LotReport.ViewModels
{
    public class DatabaseSubdirectoryViewModel : PropertyChangedBase
    {
        public DatabaseSubdirectoryViewModel()
        {
            WireCommands();
        }

        public Action Close { get; set; }

        public IMessageBoxService MessageService { get; set; } = new MessageBoxService();

        public List<DirectoryInfo> Subdirectories { get; set; }

        public DirectoryInfo SelectedDirectory { get; set; }

        public RelayCommand OkCommand { get; private set; }

        public void Init(string databaseDirectory)
        {
            DirectoryInfo databaseFolder = new DirectoryInfo(databaseDirectory);
            Subdirectories = databaseFolder.GetDirectories().ToList();
            SelectedDirectory = Subdirectories.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        }

        private void WireCommands()
        {
            OkCommand = new RelayCommand(
                param =>
                {
                    Close();
                },
                param => SelectedDirectory != null);
        }
    }
}
