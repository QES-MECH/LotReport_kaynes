using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CognexDisplay.ViewModels;
using Framework.MVVM;
using LotReport.Models;

namespace LotReport.ViewModels
{
    public class DieViewModel : PropertyChangedBase
    {
        private bool _displayGraphic;

        private string _status;
        private Die _die;
        private BitmapImage _image;
        private BinCode _currentRejectCode;

        public DieViewModel()
        {
            WireCommands();
        }

        public IMessageBoxService MessageService { get; set; } = new MessageBoxService();

        public bool CognexDisplay { get; set; }

        public CognexDisplayViewModel CognexDisplayViewModel { get; set; }

        public LeadFrameMap LeadFrameMap { get; set; }

        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public Die Die { get => _die; set => SetProperty(ref _die, value); }

        public Point MapCoordinate => Die.Coordinate;

        // public Point MapCoordinate => GetMapCoordinate(Die.Coordinate);

        public BitmapImage Image { get => _image; set => SetProperty(ref _image, value); }

        public BinCode CurrentRejectCode { get => _currentRejectCode; set => SetProperty(ref _currentRejectCode, value); }

        public AsyncCommand<object> LoadedCommand { get; private set; }

        public RelayCommand LoadImageCommand { get; private set; }

        public RelayCommand GraphicCommand { get; private set; }

        public void Init(LeadFrameMap map, Die die)
        {
            LeadFrameMap = map;
            Die = die;

            CognexDisplay = Settings.CognexDisplay;
            if (CognexDisplay)
            {
                CognexDisplayViewModel = new CognexDisplayViewModel();
            }
        }

        private void WireCommands()
        {
            LoadedCommand = AsyncCommand.Create(
                (token, param) =>
                {
                    return Task.Run(() =>
                    {
                        CurrentRejectCode = Die.BinCode;

                        try
                        {
                            if (CognexDisplay)
                            {
                                CognexDisplayViewModel.LoadImage(Die.DiePath);
                                CognexDisplayViewModel.DisplayImage(true);

                                string graphicRelativePath = Path.Combine(
                                    Directory.GetParent(Die.DiePath).Name,
                                    Path.GetFileNameWithoutExtension(Die.DiePath) + ".vpp");
                                CognexDisplayViewModel.LoadGraphic(Path.Combine(Settings.VisionImageDirectory, graphicRelativePath));
                                CognexDisplayViewModel.DisplayGraphic(_displayGraphic = true);
                            }
                            else
                            {
                                BitmapImage image = new BitmapImage();
                                image.BeginInit();
                                image.UriSource = new Uri(Die.DiePath);
                                image.EndInit();
                                image.Freeze();

                                Application.Current.Dispatcher.Invoke(() => Image = image);
                            }
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

            GraphicCommand = new RelayCommand(
                param =>
                {
                    try
                    {
                        CognexDisplayViewModel.DisplayGraphic(_displayGraphic = !_displayGraphic);
                    }
                    catch (Exception e)
                    {
                        MessageService.Show(e.Message, "Graphic", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }, param => CognexDisplay);
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
