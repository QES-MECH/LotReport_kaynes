using System.Windows;
using System.Windows.Media;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private RejectCode _rejectCode;
        private string _imagePath;
        private Brush _color;
        private Point _coordinate;

        public Die()
        {
            RejectCode = new RejectCode();
        }

        public RejectCode RejectCode
        {
            get => this._rejectCode;
            set => SetProperty(ref _rejectCode, value);
        }

        public string ImagePath
        {
            get => this._imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public Brush Color
        {
            get => this._color;
            set => SetProperty(ref _color, value);
        }

        public Point Coordinate
        {
            get => this._coordinate;
            set => SetProperty(ref _coordinate, value);
        }
    }
}
