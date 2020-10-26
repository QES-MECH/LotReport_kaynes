using System.Windows;
using System.Windows.Media;
using Framework.MVVM;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private Point _coordinate;
        private BinCode _binCode;
        private string _diePath;
        private string _markPath;
        private Mark _markStatus;
        private Color _color;
        private bool _modified;

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

        public Point Coordinate { get => _coordinate; set => SetProperty(ref _coordinate, value); }

        public BinCode BinCode { get => _binCode; set => SetProperty(ref _binCode, value); }

        public string DiePath { get => _diePath; set => SetProperty(ref _diePath, value); }

        public string MarkPath { get => _markPath; set => SetProperty(ref _markPath, value); }

        public Mark MarkStatus { get => _markStatus; set => SetProperty(ref _markStatus, value); }

        public Color Color { get => _color; set => SetProperty(ref _color, value); }

        public bool Modified { get => _modified; set => SetProperty(ref _modified, value); }

        public bool Reviewed { get; set; }
    }
}
