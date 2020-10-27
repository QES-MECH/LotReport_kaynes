using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using LotReport.Models;

namespace LotReport.Views.ReusableControls
{
    /// <summary>
    /// Interaction logic for LeadFrameMapControl.xaml
    /// </summary>
    public partial class LeadFrameMapControl : UserControl
    {
        // Using a DependencyProperty as the backing store for LeadFrameTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeadFrameTableProperty =
            DependencyProperty.Register(
            "LeadFrameTable",
            typeof(LeadFrameMap),
            typeof(LeadFrameMapControl),
            new PropertyMetadata(default(LeadFrameMap), RefreshTableCallback));

        // Using a DependencyProperty as the backing store for SelectedDie.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDieProperty =
            DependencyProperty.Register(
            "SelectedDie",
            typeof(Die),
            typeof(LeadFrameMapControl),
            new FrameworkPropertyMetadata(default(Die), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for DoubleClickCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleClickCellProperty =
            DependencyProperty.Register("DoubleClickCell", typeof(ICommand), typeof(LeadFrameMapControl), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for GridMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridMaxWidthProperty =
            DependencyProperty.Register("GridMaxWidth", typeof(double), typeof(LeadFrameMapControl), new PropertyMetadata(default(double)));

        // Using a DependencyProperty as the backing store for GridMaxHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridMaxHeightProperty =
            DependencyProperty.Register("GridMaxHeight", typeof(double), typeof(LeadFrameMapControl), new PropertyMetadata(default(double)));

        // Using a DependencyProperty as the backing store for LotId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LotIdProperty =
            DependencyProperty.Register("LotId", typeof(string), typeof(LeadFrameMapControl), new PropertyMetadata(default(string)));

        // Using a DependencyProperty as the backing store for MagazineId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagazineIdProperty =
            DependencyProperty.Register("MagazineId", typeof(string), typeof(LeadFrameMapControl), new PropertyMetadata(default(string)));

        // Using a DependencyProperty as the backing store for LeadFrameId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeadFrameIdProperty =
            DependencyProperty.Register("LeadFrameId", typeof(string), typeof(LeadFrameMapControl), new PropertyMetadata(default(string)));

        public LeadFrameMapControl()
        {
            InitializeComponent();
            LeadFrameGrid.Visibility = Visibility.Collapsed;
        }

        public LeadFrameMap LeadFrameTable
        {
            get { return (LeadFrameMap)GetValue(LeadFrameTableProperty); }
            set { SetValue(LeadFrameTableProperty, value); }
        }

        public Die SelectedDie
        {
            get { return (Die)GetValue(SelectedDieProperty); }
            set { SetValue(SelectedDieProperty, value); }
        }

        public ICommand DoubleClickCell
        {
            get { return (ICommand)GetValue(DoubleClickCellProperty); }
            set { SetValue(DoubleClickCellProperty, value); }
        }

        public double GridMaxWidth
        {
            get { return (double)GetValue(GridMaxWidthProperty); }
            set { SetValue(GridMaxWidthProperty, value); }
        }

        public double GridMaxHeight
        {
            get { return (double)GetValue(GridMaxHeightProperty); }
            set { SetValue(GridMaxHeightProperty, value); }
        }

        public string LotId
        {
            get { return (string)GetValue(LotIdProperty); }
            set { SetValue(LotIdProperty, value); }
        }

        public string MagazineId
        {
            get { return (string)GetValue(MagazineIdProperty); }
            set { SetValue(MagazineIdProperty, value); }
        }

        public string LeadFrameId
        {
            get { return (string)GetValue(LeadFrameIdProperty); }
            set { SetValue(LeadFrameIdProperty, value); }
        }

        public void RefreshTable()
        {
            if (LeadFrameTable == null)
            {
                LeadFrameGrid.Visibility = Visibility.Collapsed;
                return;
            }

            LeadFrameGrid.Visibility = Visibility.Visible;

            _dataGridMap.Columns.Clear();

            for (int x = 0; x < LeadFrameTable.SumOfXDies; x++)
            {
                FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
                gridFactory.SetBinding(Panel.BackgroundProperty, new Binding($"Dies[{x}].Color") { Converter = new Converters.ColorToBrushConverter() });
                FrameworkElementFactory tbFactory = new FrameworkElementFactory(typeof(TextBlock));
                tbFactory.SetBinding(TextBlock.TextProperty, new Binding($"Dies[{x}].BinCode.Value"));
                tbFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                tbFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121")));
                tbFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                tbFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                tbFactory.SetValue(MarginProperty, new Thickness(3));
                gridFactory.AppendChild(tbFactory);

                DataTemplate template = new DataTemplate();
                template.VisualTree = gridFactory;

                DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
                templateColumn.CellTemplate = template;

                switch (LeadFrameTable.MapOrigin)
                {
                    case Origin.Top_Left:
                    case Origin.Bottom_Left:
                        templateColumn.Header = (x + 1).ToString("D2");
                        break;
                    case Origin.Top_Right:
                    case Origin.Bottom_Right:
                        templateColumn.Header = (LeadFrameTable.SumOfXDies - x).ToString("D2");
                        break;
                    default:
                        break;
                }

                _dataGridMap.Columns.Add(templateColumn);
            }

            _dataGridMap.ItemsSource = LeadFrameTable.Rows;
        }

        private static void RefreshTableCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LeadFrameMapControl leadFrameMapControl = d as LeadFrameMapControl;
            leadFrameMapControl.RefreshTable();
        }

        private void DataGridMap_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            switch (LeadFrameTable.MapOrigin)
            {
                case Origin.Top_Left:
                case Origin.Top_Right:
                    e.Row.Header = (e.Row.GetIndex() + 1).ToString("D2");
                    break;
                case Origin.Bottom_Left:
                case Origin.Bottom_Right:
                    e.Row.Header = (LeadFrameTable.SumOfYDies - e.Row.GetIndex()).ToString("D2");
                    break;
                default:
                    break;
            }
        }

        private void DataGridMap_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Die dieData = GetSelectedDie();

            if (dieData == null)
            {
                return;
            }

            status.Text = dieData.Coordinate.ToString();
            SelectedDie = dieData;
        }

        private void DataGridMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Die dieData = GetSelectedDie();

            if (dieData == null)
            {
                return;
            }

            status.Text = GetMapCoordinate(dieData).ToString();
            SelectedDie = dieData;
        }

        private void DataGridMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Die dieData = GetSelectedDie();

            if (dieData == null)
            {
                return;
            }

            status.Text = GetMapCoordinate(dieData).ToString();
            SelectedDie = dieData;

            if (DoubleClickCell != null)
            {
                if (DoubleClickCell.CanExecute(null))
                {
                    DoubleClickCell.Execute(dieData);
                }
            }
        }

        private Die GetSelectedDie()
        {
            if (_dataGridMap.CurrentCell.Column == null)
            {
                return null;
            }

            int columnIndex = _dataGridMap.CurrentCell.Column.DisplayIndex;

            DieRow dieRowData = _dataGridMap.SelectedItem as DieRow;

            if (dieRowData == null)
            {
                return null;
            }

            Die selectedDieData = dieRowData.Dies[columnIndex];

            return selectedDieData;
        }

        private Point GetMapCoordinate(Die dieData)
        {
            Point mapCoordinate = default(Point);
            switch (LeadFrameTable.MapOrigin)
            {
                case Origin.Top_Left:
                    mapCoordinate = new Point(
                        dieData.Coordinate.X,
                        dieData.Coordinate.Y);
                    break;
                case Origin.Top_Right:
                    mapCoordinate = new Point(
                        LeadFrameTable.SumOfXDies - dieData.Coordinate.X + 1,
                        dieData.Coordinate.Y);
                    break;
                case Origin.Bottom_Left:
                    mapCoordinate = new Point(
                            dieData.Coordinate.X,
                            LeadFrameTable.SumOfYDies - dieData.Coordinate.Y + 1);
                    break;
                case Origin.Bottom_Right:
                    mapCoordinate = new Point(
                            LeadFrameTable.SumOfXDies - dieData.Coordinate.X + 1,
                            LeadFrameTable.SumOfYDies - dieData.Coordinate.Y + 1);
                    break;
                default:
                    break;
            }

            return mapCoordinate;
        }
    }
}
