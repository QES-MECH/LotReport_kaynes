using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Framework.MVVM;
using LotReport.Models;

namespace LotReport.ViewModels
{
    public class DieViewModel : PropertyChangedBase
    {
        private string _status;
        private Die _die;
        private BitmapImage _image;
        private BinCode _currentRejectCode;

        public DieViewModel()
        {
            WireCommands();
        }

        public IMessageBoxService MessageService { get; set; } = new MessageBoxService();

        public LeadFrameMap LeadFrameMap { get; set; }

        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public Die Die { get => _die; set => SetProperty(ref _die, value); }

        public Point MapCoordinate => GetMapCoordinate(Die.Coordinate);

        public BitmapImage Image { get => _image; set => SetProperty(ref _image, value); }

        public BinCode CurrentRejectCode { get => _currentRejectCode; set => SetProperty(ref _currentRejectCode, value); }

        public AsyncCommand<object> LoadedCommand { get; private set; }

        public RelayCommand LoadImageCommand { get; private set; }

        private void WireCommands()
        {
            LoadedCommand = AsyncCommand.Create(
                (token, param) =>
                {
                    return Task.Run(() =>
                    {
                        CurrentRejectCode = Die.BinCode;

                        BitmapImage image = new BitmapImage();

                        try
                        {
                            image.BeginInit();
                            image.UriSource = new Uri(Die.DiePath);
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

            LoadImageCommand = new RelayCommand(
                param =>
                {
                    try
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri((string)param);
                        image.EndInit();
                        image.Freeze();

                        Image = image;
                    }
                    catch (Exception e)
                    {
                        MessageService.Show(
                            $"Failed to Load Image. Error: {e.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                });
        }

        private Point GetMapCoordinate(Point coordinate)
        {
            switch (LeadFrameMap.MapOrigin)
            {
                case Origin.Top_Left:
                    return coordinate;
                case Origin.Top_Right:
                    return new Point(
                        LeadFrameMap.SumOfXDies - coordinate.X + 1,
                        coordinate.Y);
                case Origin.Bottom_Left:
                    return new Point(
                        coordinate.X,
                        LeadFrameMap.SumOfYDies - coordinate.Y + 1);
                case Origin.Bottom_Right:
                    return new Point(
                        LeadFrameMap.SumOfXDies - coordinate.X + 1,
                        LeadFrameMap.SumOfYDies - coordinate.Y + 1);
                default:
                    return default(Point);
            }
        }
    }
}
