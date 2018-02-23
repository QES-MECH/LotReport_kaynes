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
                return;
            }

            _dataGridMap.Columns.Clear();

            for (int x = 1; x <= LeadFrameTable.SumOfXDies; x++)
            {
                DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
                templateColumn.Header = x.ToString("D2");

                int dieIndex = x - 1;

                var xamlString =
                    "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Grid Background=\"{Binding Dies[" + dieIndex + "].Color}\">" +
                            "<TextBlock" +
                            " Text=\"{Binding Dies[" + dieIndex + "].BinCode.Value}\"" +
                            " FontWeight=\"Bold\"" +
                            " Foreground=\"#212121\"" +
                            " HorizontalAlignment=\"Center\"" +
                            " VerticalAlignment=\"Center\"" +
                            " Margin=\"3\"" +
                            "/>" +
                        "</Grid>" +
                    "</DataTemplate>";

                XmlReader xr = XmlReader.Create(new StringReader(xamlString));

                DataTemplate dataTemplate = (DataTemplate)XamlReader.Load(xr);
                templateColumn.CellTemplate = dataTemplate;

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
            e.Row.Header = (e.Row.GetIndex() + 1).ToString("D2");
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

            status.Text = dieData.Coordinate.ToString();
            SelectedDie = dieData;
        }

        private void DataGridMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Die dieData = GetSelectedDie();

            if (dieData == null)
            {
                return;
            }

            status.Text = dieData.Coordinate.ToString();
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
    }
}
