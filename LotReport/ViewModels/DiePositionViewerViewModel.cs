using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
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

        // Categorize 3D images
        private bool _is3DMode;

        private BitmapImage _imageLeft;
        private BitmapImage _imageRight;
        private BitmapImage _imageFront;
        private BitmapImage _imageBack;

        private CognexDisplayViewModel _cognexVmLeft;
        private CognexDisplayViewModel _cognexVmRight;
        private CognexDisplayViewModel _cognexVmFront;
        private CognexDisplayViewModel _cognexVmBack;

        private string _pathLeft;
        private string _pathRight;
        private string _pathFront;
        private string _pathBack;

        private int _selectedLighting = 1;

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

        public bool Is3DMode { get => _is3DMode; set => SetProperty(ref _is3DMode, value); }

        public BitmapImage ImageLeft { get => _imageLeft; set => SetProperty(ref _imageLeft, value); }

        public BitmapImage ImageRight { get => _imageRight; set => SetProperty(ref _imageRight, value); }

        public BitmapImage ImageFront { get => _imageFront; set => SetProperty(ref _imageFront, value); }

        public BitmapImage ImageBack { get => _imageBack; set => SetProperty(ref _imageBack, value); }

        public CognexDisplayViewModel CognexVmLeft { get => _cognexVmLeft; set => SetProperty(ref _cognexVmLeft, value); }

        public CognexDisplayViewModel CognexVmRight { get => _cognexVmRight; set => SetProperty(ref _cognexVmRight, value); }

        public CognexDisplayViewModel CognexVmFront { get => _cognexVmFront; set => SetProperty(ref _cognexVmFront, value); }

        public CognexDisplayViewModel CognexVmBack { get => _cognexVmBack; set => SetProperty(ref _cognexVmBack, value); }

        public string PathLeft { get => _pathLeft; set => SetProperty(ref _pathLeft, value); }

        public string PathRight { get => _pathRight; set => SetProperty(ref _pathRight, value); }

        public string PathFront { get => _pathFront; set => SetProperty(ref _pathFront, value); }

        public string PathBack { get => _pathBack; set => SetProperty(ref _pathBack, value); }

        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                if (_selectedImagePath != value)
                {
                    _displayGraphic = false;
                }

                CurrentIndex = Current2DPaths.IndexOf(value);
                UpdateCurrentRejectCode(value);
                SetProperty(ref _selectedImagePath, value);
            }
        }

        public int CurrentIndex { get; set; }

        public List<string> Current2DPaths => SelectedLighting == 1 ? Die.DiePath : Die.DiePathLighting2;

        public int SelectedLighting
        {
            get => _selectedLighting;
            set
            {
                if (_selectedLighting != value)
                {
                    _selectedLighting = value;
                    OnPropertyChanged(nameof(SelectedLighting));
                    OnPropertyChanged(nameof(IsLighting1Enabled));
                    OnPropertyChanged(nameof(IsLighting2Enabled));
                    OnPropertyChanged(nameof(Current2DPaths));
                }
            }
        }

        public bool IsLighting1Enabled => SelectedLighting != 1;

        public bool IsLighting2Enabled => SelectedLighting != 2;

        public AsyncCommand<object> LoadSelectedDieInspect { get; private set; }

        public AsyncCommand<object> LoadedCommand { get; private set; }

        public RelayCommand LoadImageCommand { get; private set; }

        public RelayCommand GraphicCommand { get; private set; }

        public AsyncCommand<object> View3DImageCommand { get; private set; }

        public RelayCommand BackTo2DViewCommand { get; private set; }

        public RelayCommand Selected3DSideCommand { get; private set; }

        public RelayCommand Lighting1Command { get; private set; }

        public RelayCommand Lighting2Command { get; private set; }

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
                async (token, param) =>
                {
                    string imagePath = param as string;
                    if (string.IsNullOrEmpty(imagePath))
                    {
                        return;
                    }

                    imagePath = Path.Combine(Settings.VisionImageDirectory, imagePath);
                    SelectedImagePath = imagePath;

                    if (CognexDisplay)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (CognexDisplayViewModel == null)
                            {
                                CognexDisplayViewModel = new CognexDisplayViewModel();
                            }

                            CognexDisplayViewModel.DisplayGraphic(false);
                            CognexDisplayViewModel.DisplayImage(false);

                            CognexDisplayViewModel.LoadImage(SelectedImagePath);
                            CognexDisplayViewModel.DisplayImage(true);

                            string vpp = Path.ChangeExtension(SelectedImagePath, ".vpp");

                            if (File.Exists(vpp))
                            {
                                CognexDisplayViewModel.LoadGraphic(vpp);
                                CognexDisplayViewModel.DisplayGraphic(true);

                            }
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            var newImage = LoadBitmap(SelectedImagePath);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Image = newImage;
                            });
                        });
                    }

                    UpdatePositionHeaderDisplay();
                    Status = SelectedImagePath;
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

            View3DImageCommand = AsyncCommand.Create(
                async (token, param) =>
                {
                    if (Die?.DiePath3D == null)
                    {
                        return;
                    }

                    Is3DMode = true;

                    PathLeft = Die.DiePath3D.TryGetValue("Left", out var listL) && CurrentIndex < listL.Count ? listL[CurrentIndex] : null;
                    PathRight = Die.DiePath3D.TryGetValue("Right", out var listR) && CurrentIndex < listR.Count ? listR[CurrentIndex] : null;
                    PathFront = Die.DiePath3D.TryGetValue("Front", out var listF) && CurrentIndex < listF.Count ? listF[CurrentIndex] : null;
                    PathBack = Die.DiePath3D.TryGetValue("Back", out var listB) && CurrentIndex < listB.Count ? listB[CurrentIndex] : null;

                    if (CognexDisplay)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (CognexVmLeft != null)
                            {
                                CognexVmLeft.DisplayGraphic(false);
                                CognexVmLeft.DisplayImage(false);
                            }

                            if (CognexVmRight != null)
                            {
                                CognexVmRight.DisplayGraphic(false);
                                CognexVmRight.DisplayImage(false);
                            }

                            if (CognexVmFront != null)
                            {
                                CognexVmFront.DisplayGraphic(false);
                                CognexVmFront.DisplayImage(false);
                            }

                            if (CognexVmBack != null)
                            {
                                CognexVmBack.DisplayGraphic(false);
                                CognexVmBack.DisplayImage(false);
                            }

                            CognexVmLeft = LoadCognex(PathLeft);
                            CognexVmRight = LoadCognex(PathRight);
                            CognexVmFront = LoadCognex(PathFront);
                            CognexVmBack = LoadCognex(PathBack);
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            var imgL = LoadBitmap(PathLeft);
                            var imgR = LoadBitmap(PathRight);
                            var imgF = LoadBitmap(PathFront);
                            var imgB = LoadBitmap(PathBack);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ImageLeft = null;
                                ImageRight = null;
                                ImageFront = null;
                                ImageBack = null;

                                ImageLeft = imgL;
                                ImageRight = imgR;
                                ImageFront = imgF;
                                ImageBack = imgB;
                            });
                        });
                    }
                    Status = $"Displaying 3D images for Die {PositionHeader}";
                },
                param => !string.IsNullOrEmpty(SelectedImagePath));

            BackTo2DViewCommand = new RelayCommand(
                param =>
                {
                    if (!Is3DMode)
                    {
                        return;
                    }

                    Is3DMode = false;

                    ImageLeft = null;
                    ImageRight = null;
                    ImageFront = null;
                    ImageBack = null;

                    if (CognexVmLeft != null)
                    {
                        CognexVmLeft.DisplayGraphic(false);
                        CognexVmLeft.DisplayImage(false);
                        CognexVmLeft = null;
                    }

                    if (CognexVmRight != null)
                    {
                        CognexVmRight.DisplayGraphic(false);
                        CognexVmRight.DisplayImage(false);
                        CognexVmRight = null;
                    }

                    if (CognexVmFront != null)
                    {
                        CognexVmFront.DisplayGraphic(false);
                        CognexVmFront.DisplayImage(false);
                        CognexVmFront = null;
                    }

                    if (CognexVmBack != null)
                    {
                        CognexVmBack.DisplayGraphic(false);
                        CognexVmBack.DisplayImage(false);
                        CognexVmBack = null;
                    }
                });

            Selected3DSideCommand = new RelayCommand(
                param =>
                {
                    if (param is string path && !string.IsNullOrEmpty(path))
                    {
                        SelectedImagePath = path;
                    }
                });

            Lighting1Command = new RelayCommand(
                param =>
                {
                    SelectedLighting = 1;
                    var firstImage = Current2DPaths?.FirstOrDefault();

                    if (!string.IsNullOrEmpty(firstImage))
                    {
                        LoadSelectedDieInspect.Execute(firstImage);
                        UpdatePositionHeaderDisplay();
                    }
                },
                param =>
                {
                    return Die != null;
                });

            Lighting2Command = new RelayCommand(
                param =>
                {
                    SelectedLighting = 2;
                    var firstImage = Current2DPaths?.FirstOrDefault();

                    if (!string.IsNullOrEmpty(firstImage))
                    {
                        LoadSelectedDieInspect.Execute(firstImage);
                        UpdatePositionHeaderDisplay();
                    }
                },
                param =>
                {
                    return Die != null;
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

        private void UpdatePositionHeaderDisplay()
        {
            PositionHeader = $"Coordinate[{MapCoordinate}] - Position: {CurrentIndex + 1}";
        }

        private void UpdateCurrentRejectCode(string path)
        {
            if (string.IsNullOrEmpty(path) || SelectedImagePath == null)
            {
                return;
            }

            // 1. Check if the path exists in current 2D paths
            var paths = Current2DPaths;
            int index2D = paths != null ? paths.IndexOf(path) : -1;
            if (index2D != -1 && index2D < Die.BinCode2D.Count)
            {
                CurrentRejectCode = Die.BinCode2D[index2D];
                return;
            }

            // 2. Check if the path exists in 3D (Dictionary search)
            foreach (var side in Die.DiePath3D.Keys)
            {
                int index3D = Die.DiePath3D[side].IndexOf(path);
                if (index3D != -1 && index3D < Die.BinCode3D.Count)
                {
                    CurrentRejectCode = Die.BinCode3D[index3D];
                    return;
                }
            }

            // 3. Fallback: If not found in 2D/3D lists, show the Main Die bincode
            CurrentRejectCode = Die.BinCode;
        }

        private BitmapImage LoadBitmap(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = File.OpenRead(path))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.DecodePixelWidth = 1024;
                    bitmap.EndInit();
                }

                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private CognexDisplayViewModel LoadCognex(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                var vm = new CognexDisplayViewModel();
                vm.DisplayGraphic(false);
                vm.DisplayImage(false);

                vm.LoadImage(path);
                vm.DisplayImage(true);

                string vpp = Path.ChangeExtension(path, ".vpp");
                if (File.Exists(vpp))
                {
                    vm.LoadGraphic(vpp);
                    _displayGraphic = true;
                    vm.DisplayGraphic(_displayGraphic = true);
                }

                return vm;
            }
            catch
            {
                return null;
            }
        }
    }
}
