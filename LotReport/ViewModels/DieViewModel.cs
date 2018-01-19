using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using LotReport.Models;

namespace LotReport.ViewModels
{
    public class DieViewModel : PropertyChangedBase
    {
        private string _status;
        private Die _die;
        private BitmapImage _image;
        private RejectCode _currentRejectCode;

        public DieViewModel()
        {
            WireCommands();
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public Die Die
        {
            get => _die;
            set => SetProperty(ref _die, value);
        }

        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public RejectCode CurrentRejectCode
        {
            get => _currentRejectCode;
            set => SetProperty(ref _currentRejectCode, value);
        }

        public AsyncCommand<object> LoadedCommand { get; private set; }

        private void WireCommands()
        {
            LoadedCommand = AsyncCommand.Create(
                (token, param) =>
                {
                    return Task.Run(() =>
                    {
                        CurrentRejectCode = Die.RejectCode;

                        BitmapImage image = new BitmapImage();

                        try
                        {
                            image.BeginInit();
                            image.UriSource = new Uri(Die.ImagePath);
                            image.EndInit();
                            image.Freeze();

                            Application.Current.Dispatcher.Invoke(() => Image = image);
                        }
                        catch (Exception ex)
                        {
                            Status = string.Format("Failed to load Die Image. Error: {0}", ex.Message);
                        }
                    });
                });
        }
    }
}
