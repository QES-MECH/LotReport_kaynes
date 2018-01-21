using System.Windows;
using System.Windows.Media;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private RejectCode _rejectCode;
        private Mark _markStatus;
        private string _diePath;
        private string _markPath;
        private Brush _color;
        private Point _coordinate;

        public Die()
        {
            RejectCode = new RejectCode();
        }

        public enum Mark
        {
            NA,
            Pass,
            Fail
        }

        public RejectCode RejectCode
        {
            get => this._rejectCode;
            set => SetProperty(ref _rejectCode, value);
        }

        public Mark MarkStatus
        {
            get => _markStatus;
            set => SetProperty(ref _markStatus, value);
        }

        public string DiePath
        {
            get => this._diePath;
            set => SetProperty(ref _diePath, value);
        }

        public string MarkPath
        {
            get => _markPath;
            set => SetProperty(ref _markPath, value);
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
