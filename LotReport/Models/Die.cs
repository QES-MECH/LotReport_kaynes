using System.Windows;
using System.Windows.Media;

namespace LotReport.Models
{
    public class Die : PropertyChangedBase
    {
        private RejectCode rejectCode;
        private string imagePath;
        private Brush color;
        private Point coordinate;

        public Die()
        {
            RejectCode = new RejectCode();
        }

        public RejectCode RejectCode
        {
            get
            {
                return this.rejectCode;
            }

            set
            {
                this.rejectCode = value;
                this.OnPropertyChanged();
            }
        }

        public string ImagePath
        {
            get
            {
                return this.imagePath;
            }

            set
            {
                this.imagePath = value;
                this.OnPropertyChanged();
            }
        }

        public Brush Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.color = value;
                this.OnPropertyChanged();
            }
        }

        public Point Coordinate
        {
            get
            {
                return this.coordinate;
            }

            set
            {
                this.coordinate = value;
                this.OnPropertyChanged();
            }
        }
    }
}
