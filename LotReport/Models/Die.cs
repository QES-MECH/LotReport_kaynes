using System.Windows;
using System.Windows.Media;
using Framework.MVVM;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private BinCode _binCode;
        private Mark _markStatus;
        private string _diePath;
        private string _markPath;
        private Brush _color;
        private Point _coordinate;

        public Die()
        {
            BinCode = new BinCode();
        }

        public enum Mark
        {
            NA,
            Pass,
            Fail
        }

        public BinCode BinCode
        {
            get => _binCode;
            set => SetProperty(ref _binCode, value);
        }

        public Mark MarkStatus
        {
            get => _markStatus;
            set => SetProperty(ref _markStatus, value);
        }

        public string DiePath
        {
            get => _diePath;
            set => SetProperty(ref _diePath, value);
        }

        public string MarkPath
        {
            get => _markPath;
            set => SetProperty(ref _markPath, value);
        }

        public Brush Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public Point Coordinate
        {
            get => _coordinate;
            set => SetProperty(ref _coordinate, value);
        }
    }
}
