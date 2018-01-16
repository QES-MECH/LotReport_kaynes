using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LotReport.Views.ReusableControls
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(BitmapImage), typeof(ImageControl), new PropertyMetadata(default(BitmapImage), FitImageCallback));

        // Using a DependencyProperty as the backing store for ScaleX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register("ScaleX", typeof(double), typeof(ImageControl), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for ScaleY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(ImageControl), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private Point? lastCenterPositionOnImage;
        private Point? lastMousePositionOnImage;
        private Point? lastPanPosition;

        public ImageControl()
        {
            this.InitializeComponent();
        }

        public BitmapImage Image
        {
            get { return (BitmapImage)this.GetValue(ImageProperty); }
            set { this.SetValue(ImageProperty, value); }
        }

        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { this.SetValue(ScaleXProperty, value); }
        }

        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { this.SetValue(ScaleYProperty, value); }
        }

        private static void FitImageCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageControl imageControl = d as ImageControl;
            imageControl.FitImage();
        }

        private void FitImage()
        {
            if (this.Image != null && this.Image.IsFrozen)
            {
                this.ScaleX = this.scrollViewer.ActualWidth / this.Image.PixelWidth;
                this.ScaleY = this.scrollViewer.ActualHeight / this.Image.PixelHeight;
            }
        }

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPosition = e.GetPosition(this.scrollViewer);
            if (currentPosition.X <= this.scrollViewer.ViewportWidth && currentPosition.Y <= this.scrollViewer.ViewportHeight)
            {
                this.lastPanPosition = currentPosition;
                Mouse.Capture(this.scrollViewer);
            }
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.lastPanPosition.HasValue)
            {
                return;
            }

            Point currentPosition = e.GetPosition(this.scrollViewer);
            double dX = currentPosition.X - this.lastPanPosition.Value.X;
            double dY = currentPosition.Y - this.lastPanPosition.Value.Y;

            this.lastPanPosition = currentPosition;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
        }

        private void ScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.scrollViewer.ReleaseMouseCapture();
            this.lastPanPosition = null;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.lastMousePositionOnImage = Mouse.GetPosition(this.imageView);

            if (e.Delta > 0)
            {
                this.zoomSlider.Value += 0.1;
            }

            if (e.Delta < 0)
            {
                this.zoomSlider.Value -= 0.1;
            }

            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(this.scrollViewer);
            if (currentMousePosition.X <= this.scrollViewer.ViewportWidth && currentMousePosition.Y <= this.scrollViewer.ViewportHeight)
            {
                this.lastMousePositionOnImage = Mouse.GetPosition(this.imageView);

                if (this.ScaleX >= 1 || this.ScaleY >= 1)
                {
                    this.FitImage();
                }
                else
                {
                    this.ScaleX = 1;
                    this.ScaleY = 1;
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? previousPositionOnImage = null;
                Point? currentPositionOnImage = null;

                if (this.lastMousePositionOnImage.HasValue)
                {
                    previousPositionOnImage = this.lastMousePositionOnImage;
                    currentPositionOnImage = Mouse.GetPosition(this.imageView);

                    this.lastMousePositionOnImage = null;
                }
                else
                {
                    if (this.lastCenterPositionOnImage.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, this.imageView);

                        previousPositionOnImage = this.lastCenterPositionOnImage;
                        currentPositionOnImage = centerOfTargetNow;
                    }
                }

                if (previousPositionOnImage.HasValue)
                {
                    double displacementXInTargetPixels = currentPositionOnImage.Value.X - previousPositionOnImage.Value.X;
                    double displaycementYInTargetPixels = currentPositionOnImage.Value.Y - previousPositionOnImage.Value.Y;

                    double multiplicatorX = e.ExtentWidth / this.imageView.ActualWidth;
                    double multiplicatorY = e.ExtentHeight / this.imageView.ActualHeight;

                    double newOffsetX = this.scrollViewer.HorizontalOffset - (displacementXInTargetPixels * multiplicatorX);
                    double newOffsetY = this.scrollViewer.VerticalOffset - (displaycementYInTargetPixels * multiplicatorY);

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    this.scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    this.scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ScaleX = e.NewValue;
            this.ScaleY = e.NewValue;

            var centerOfViewport = new Point(this.scrollViewer.ViewportWidth / 2, this.scrollViewer.ViewportHeight / 2);

            this.lastCenterPositionOnImage = this.scrollViewer.TranslatePoint(centerOfViewport, this.imageView);
        }
    }
}
