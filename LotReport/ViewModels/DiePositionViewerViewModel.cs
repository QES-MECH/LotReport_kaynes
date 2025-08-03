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
    public class DiePositionViewerViewModel : PropertyChangedBase
    {
        private bool _displayGraphic;

        private string _status;
        private Die _die;
        private BitmapImage _image;
        private string _positionHeader;
        private BinCode _currentRejectCode;
        private string _selectedImagePath;

        public DiePositionViewerViewModel()
        {
            WireCommands();
        }

        public IMessageBoxService MessageService { get; set; } = new MessageBoxService();

        public bool CognexDisplay { get; set; }

        public CognexDisplayViewModel CognexDisplayViewModel { get; set; }

        public LeadFrameMap LeadFrameMap { get; set; }

        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public Die Die { get => _die; set => SetProperty(ref _die, value); }

        public Point MapCoordinate => GetMapCoordinate(Die.Coordinate);

        public string PositionHeader { get => _positionHeader; set => SetProperty(ref _positionHeader, value); }

        public BitmapImage Image { get => _image; set => SetProperty(ref _image, value); }

        public BinCode CurrentRejectCode { get => _currentRejectCode; set => SetProperty(ref _currentRejectCode, value); }

        public List<string> CombinedImagePaths => (Die.DiePath ?? new List<string>())
            .Concat(Die.DiePath3D?.SelectMany(p => p.Value) ?? Enumerable.Empty<string>())
            .ToList();

        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                if (_selectedImagePath != value)
                {
                    _displayGraphic = false;
                }

                CurrentIndex = CombinedImagePaths.IndexOf(value);
                SetProperty(ref _selectedImagePath, value);
            }
        }

        public int CurrentIndex { get; set; }

        public AsyncCommand<object> LoadSelectedDieInspect { get; private set; }

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
                try
                {
                    CognexDisplayViewModel = new CognexDisplayViewModel();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Failed to Load Cognex Display Library");
                }
            }
        }

        private void WireCommands()
        {
            LoadSelectedDieInspect = AsyncCommand.Create(
                (token, param) =>
                {
                    return Task.Run(() =>
                    {
                        try
                        {
                            CurrentRejectCode = Die.BinCode;
                            GC.Collect();

                            string imagePath = param as string;
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                SelectedImagePath = imagePath;
                                if (CognexDisplay)
                                {
                                    CognexDisplayViewModel.DisplayGraphic(false);
                                    CognexDisplayViewModel.DisplayImage(false);

                                    CognexDisplayViewModel.LoadImage(SelectedImagePath);
                                    CognexDisplayViewModel.DisplayImage(true);
                                }
                                else
                                {
                                    BitmapImage image = new BitmapImage();
                                    using (FileStream stream = File.OpenRead(SelectedImagePath))
                                    {
                                        image.BeginInit();
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.StreamSource = stream;
                                        image.EndInit();
                                        image.Freeze();
                                    }

                                    Application.Current.Dispatcher.Invoke(() => Image = image);
                                }
                                UpdatePositionHeaderDisplay();
                                Status = SelectedImagePath;
                            }
                        }
                        catch (Exception ex)
                        {
                            Status = string.Format("Failed to load Die Image. Error: {0}", ex.Message);
                        }
                    });
            });

            GraphicCommand = new RelayCommand(
                param =>
                {
                    try
                    {
                        string graphicRelativePath = Path.ChangeExtension(SelectedImagePath, ".vpp");
                        if (!_displayGraphic)
                        {
                            CognexDisplayViewModel.LoadGraphic(graphicRelativePath);
                            CognexDisplayViewModel.DisplayGraphic(_displayGraphic = true);
                        }
                        else
                        {
                            CognexDisplayViewModel.DisplayGraphic(_displayGraphic = false);
                        }
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

        private void UpdatePositionHeaderDisplay()
        {
            PositionHeader = $"Coordinate[{MapCoordinate}] - Position: {CurrentIndex + 1}";
        }
    }
}
