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

        public Die Die
        {
            get
            {
                return _die;
            }

            set
            {
                _die = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage Image
        {
            get
            {
                return _image;
            }

            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public RejectCode CurrentRejectCode
        {
            get
            {
                return _currentRejectCode;
            }

            set
            {
                _currentRejectCode = value;
                OnPropertyChanged();
            }
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
