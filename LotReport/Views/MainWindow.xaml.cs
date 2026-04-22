using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LotReport.Models;
using LotReport.ViewModels;
using LotReport.Views.ReusableControls;
using Newtonsoft.Json;

namespace LotReport.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _settingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ColumnSettings.json");

        public MainWindow()
        {
            InitializeComponent();
            LoadColumnSettings();
            var vm = new MainWindowViewModel();
            this.DataContext = vm;

            vm.OnLotDataRefreshed += () =>
            {
                Dispatcher.BeginInvoke(new Action(() => UpdateDynamicDefectColumns()));
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = DataContext as MainWindowViewModel;
            if (vm == null)
            {
                return;
            }

            await vm.LoadedCommand.ExecuteAsync(null);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (DataContext is MainWindowViewModel vm)
            {
                if (dataGrid.SelectedItem != null)
                {
                    vm.UpdateSelectedLotCommand.Execute(vm.SelectedLot);
                    e.Handled = true;
                }
            }
        }

        private void HideColumn_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem.Parent as ContextMenu;
            var header = contextMenu.PlacementTarget as DataGridColumnHeader;
            var column = header.Column;

            column.Visibility = Visibility.Collapsed;
            SyncSummaryTabVisibility(column.Header.ToString(), Visibility.Collapsed);

            UpdateUnhideMenu(contextMenu, column);
            SaveColumnSettings();
        }

        private void UpdateUnhideMenu(ContextMenu menu, DataGridColumn column)
        {
            var separator = menu.Items[1] as Separator;
            var unhideMenu = menu.Items[2] as MenuItem;

            separator.Visibility = Visibility.Visible;
            unhideMenu.Visibility = Visibility.Visible;

            var restoreItem = new MenuItem { Header = column.Header.ToString() };
            restoreItem.Click += (s, ev) =>
            {
                column.Visibility = Visibility.Visible;
                SyncSummaryTabVisibility(column.Header.ToString(), Visibility.Visible);
                unhideMenu.Items.Remove(restoreItem);
                if (unhideMenu.Items.Count == 0)
                {
                    unhideMenu.Visibility = Visibility.Collapsed;
                    separator.Visibility = Visibility.Collapsed;
                }

                SaveColumnSettings();
            };
            unhideMenu.Items.Add(restoreItem);
        }

        private void SyncSummaryTabVisibility(string header, Visibility visibility)
        {
            foreach (var child in LotSummaryPanel.Children)
            {
                if (child is LabelTextBox control)
                {
                    if (control.Label.Replace(" ", "").Equals(header.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                    {
                        control.Visibility = visibility;
                        break;
                    }
                }
            }
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var column in LotDataGrid.Columns)
            {
                column.Visibility = Visibility.Visible;
            }

            foreach (var child in LotSummaryPanel.Children)
            {
                if (child is LabelTextBox control)
                {
                    control.Visibility = Visibility.Visible;
                }
            }

            if (LotDataGrid.Resources["ColumnHeaderMenu"] is ContextMenu menu)
            {
                if (menu.Items.Count >= 3)
                {
                    var separator = menu.Items[1] as Separator;
                    var unhideMenu = menu.Items[2] as MenuItem;

                    if (unhideMenu != null)
                    {
                        unhideMenu.Items.Clear();
                        unhideMenu.Visibility = Visibility.Collapsed;
                    }

                    if (separator != null)
                    {
                        separator.Visibility = Visibility.Collapsed;
                    }
                }
            }

            File.Delete(_settingsFilePath);
        }

        private void SaveColumnSettings()
        {
            try
            {
                var hiddenColumns = LotDataGrid.Columns.Where(c => c.Visibility == Visibility.Collapsed).Select(c => c.Header.ToString()).ToList();
                string json = JsonConvert.SerializeObject(hiddenColumns, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save column settings: {ex.Message}");
            }
        }

        private void LoadColumnSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                var hiddenColumns = JsonConvert.DeserializeObject<List<string>>(json);

                if (hiddenColumns == null)
                {
                    return;
                }

                foreach (var headerName in hiddenColumns)
                {
                    var column = LotDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == headerName);
                    if (column != null)
                    {
                        column.Visibility = Visibility.Collapsed;

                        SyncSummaryTabVisibility(headerName, Visibility.Collapsed);

                        if (LotDataGrid.Resources["ColumnHeaderMenu"] is ContextMenu menu)
                        {
                            UpdateUnhideMenu(menu, column);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private void UpdateDynamicDefectColumns()
        {
            var viewmodel = this.DataContext as MainWindowViewModel;
            if (viewmodel == null)
            {
                return;
            }

            var dynamicColumns = LotDataGrid.Columns.Where(c => c.GetValue(FrameworkElement.NameProperty)?.ToString() == "DynamicColumn").ToList();
            foreach (var col in dynamicColumns)
            {
                LotDataGrid.Columns.Remove(col);
            }

            var dynamicSummaryControls = LotSummaryPanel.Children.Cast<FrameworkElement>().Where(c => c.GetValue(FrameworkElement.NameProperty)?.ToString() == "DynamicColumn").ToList();
            foreach (var control in dynamicSummaryControls)
            {
                LotSummaryPanel.Children.Remove(control);
            }

            var anchor = LotDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Units Rejected");
            if (anchor == null)
            {
                return;
            }

            int insertAt = LotDataGrid.Columns.IndexOf(anchor) + 1;

            var categories = viewmodel.LotDataView.Cast<LotData>().Where(lot => lot.DefectCategoryYieldsPercentage != null).SelectMany(lot => lot.DefectCategoryYieldsPercentage.Keys).Distinct();

            foreach (var catName in categories)
            {
                var col = new DataGridTextColumn
                {
                    Header = catName,
                    Binding = new Binding($"DefectCategoryYieldsPercentage[{catName}].Count"),
                    IsReadOnly = true,
                    MinWidth = 40
                };
                col.SetValue(FrameworkElement.NameProperty, "DynamicColumn");
                LotDataGrid.Columns.Insert(insertAt, col);
                insertAt++;

                var percentCol = new DataGridTextColumn
                {
                    Header = $"{catName} Yield Impact(%)",
                    Binding = new Binding($"DefectCategoryYieldsPercentage[{catName}].Impact") { StringFormat = "F3" },
                    IsReadOnly = true,
                    MinWidth = 80
                };
                percentCol.SetValue(FrameworkElement.NameProperty, "DynamicColumn");
                LotDataGrid.Columns.Insert(insertAt, percentCol);
                insertAt++;
            }
        }

        public class DefectColumn
        {
            public string Category { get; set; }

            public int Count { get; set; }

            public double YieldImpactPercentage { get; set; }
        }
    }
}
