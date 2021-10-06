using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table.PivotTable;

namespace LotReport.Models
{
    public class SummaryReport
    {
        private string _chartType;
        private List<LotData> _filteredData;
        private bool _autoLaunch;
        private string _excelPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "SummaryReport.xlsx";

        public SummaryReport(List<LotData> inputData, string chartType, bool autoLaunchApp)
        {
            _filteredData = inputData;
            _chartType = chartType;
            _autoLaunch = autoLaunchApp;
        }

        public void GenerateSummaryReportToExcel()
        {
            var excelFile = new FileInfo(_excelPath);
            if (excelFile.Exists)
            {
                try
                {
                    excelFile.Delete();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelFile))
            {
                // Fill the Rawdata sheet
                ExcelWorksheet rawDataSheet = package.Workbook.Worksheets.Add("RawData");
                rawDataSheet.Cells["A1"].Value = "Date";
                rawDataSheet.Cells["B1"].Value = "Work Week";
                rawDataSheet.Cells["C1"].Value = "Month";

                rawDataSheet.Cells["D1"].Value = "Machine ID";
                rawDataSheet.Cells["E1"].Value = "Lot ID";
                rawDataSheet.Cells["F1"].Value = "Operator ID";
                rawDataSheet.Cells["G1"].Value = "Product Code";
                rawDataSheet.Cells["H1"].Value = "Die Attach";
                rawDataSheet.Cells["I1"].Value = "Wire Bond #1";
                rawDataSheet.Cells["J1"].Value = "Wire Bond #2";
                rawDataSheet.Cells["K1"].Value = "Bonding Diagram";
                rawDataSheet.Cells["L1"].Value = "Recipe";
                rawDataSheet.Cells["M1"].Value = "LeadFrame Unit(s)";
                rawDataSheet.Cells["N1"].Value = "LeadFrame X Unit(s)";
                rawDataSheet.Cells["O1"].Value = "LeadFrame Y Unit(s)";
                rawDataSheet.Cells["P1"].Value = "Start Time";
                rawDataSheet.Cells["Q1"].Value = "End Time";
                rawDataSheet.Cells["R1"].Value = "UPH";
                rawDataSheet.Cells["S1"].Value = "LeadFrame(s) Inspected";
                rawDataSheet.Cells["T1"].Value = "Unit(s) Passed";
                rawDataSheet.Cells["U1"].Value = "Unit(s) Rejected";
                rawDataSheet.Cells["V1"].Value = "Unit(s) False-Called";
                rawDataSheet.Cells["W1"].Value = "Unit(s) Yield (%)";
                rawDataSheet.Cells["X1"].Value = "False-Called (%)";
                rawDataSheet.Cells["Y1"].Value = "Marked Unit(s)";
                rawDataSheet.Cells["Z1"].Value = "Unmarked Unit(s)";
                rawDataSheet.Cells["AA1"].Value = "Marked Unit(s) Passed";
                rawDataSheet.Cells["AB1"].Value = "Marked Unit(s) Rejected";
                rawDataSheet.Cells["AC1"].Value = "Marked Unit(s) Yield (%)";

                int rowIdx = 2; // count from 1
                foreach (LotData lotData in _filteredData)
                {
                    CultureInfo myCI = new CultureInfo("en-US");
                    Calendar myCalender = myCI.Calendar;
                    CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                    rawDataSheet.Cells[rowIdx, 1].Value = lotData.EndTime.ToString("dd/MM/yyyy");
                    rawDataSheet.Cells[rowIdx, 2].Value = myCalender.GetWeekOfYear(lotData.EndTime, myCWR, DayOfWeek.Sunday);
                    rawDataSheet.Cells[rowIdx, 3].Value = myCalender.GetMonth(lotData.EndTime);

                    rawDataSheet.Cells[rowIdx, 4].Value = lotData.MachineId;
                    rawDataSheet.Cells[rowIdx, 5].Value = lotData.LotId;
                    rawDataSheet.Cells[rowIdx, 6].Value = lotData.OperatorId;
                    rawDataSheet.Cells[rowIdx, 7].Value = lotData.ProductCode;
                    rawDataSheet.Cells[rowIdx, 8].Value = lotData.DieAttach;
                    rawDataSheet.Cells[rowIdx, 9].Value = lotData.WireBond0;
                    rawDataSheet.Cells[rowIdx, 10].Value = lotData.WireBond1;
                    rawDataSheet.Cells[rowIdx, 11].Value = lotData.BondingDiagram;
                    rawDataSheet.Cells[rowIdx, 12].Value = lotData.RecipeName;
                    rawDataSheet.Cells[rowIdx, 13].Value = lotData.LeadFrameUnits;
                    rawDataSheet.Cells[rowIdx, 14].Value = lotData.LeadFrameXUnits;
                    rawDataSheet.Cells[rowIdx, 15].Value = lotData.LeadFrameYUnits;
                    rawDataSheet.Cells[rowIdx, 16].Value = lotData.StartTime;
                    rawDataSheet.Cells[rowIdx, 17].Value = lotData.EndTime;
                    rawDataSheet.Cells[rowIdx, 18].Value = Math.Round(lotData.UPH, 3);
                    rawDataSheet.Cells[rowIdx, 19].Value = lotData.LeadFramesInspected;
                    rawDataSheet.Cells[rowIdx, 20].Value = lotData.UnitsPassed;
                    rawDataSheet.Cells[rowIdx, 21].Value = lotData.UnitsRejected;
                    rawDataSheet.Cells[rowIdx, 22].Value = lotData.UnitsOverRejected;
                    rawDataSheet.Cells[rowIdx, 23].Value = Math.Round(lotData.UnitsYieldPercentage, 3);
                    rawDataSheet.Cells[rowIdx, 24].Value = Math.Round(lotData.OverRejectPercentage, 3);
                    rawDataSheet.Cells[rowIdx, 25].Value = lotData.MarkedUnits;
                    rawDataSheet.Cells[rowIdx, 26].Value = lotData.UnmarkedUnits;
                    rawDataSheet.Cells[rowIdx, 27].Value = lotData.MarkedUnitsPassed;
                    rawDataSheet.Cells[rowIdx, 28].Value = lotData.MarkedUnitsRejected;
                    rawDataSheet.Cells[rowIdx, 29].Value = Math.Round(lotData.MarkedUnitsYieldPercentage, 3);

                    rowIdx++;
                }

                rawDataSheet.Column(16).Style.Numberformat.Format = "dd/MM/yyyy hh:mm";
                rawDataSheet.Column(17).Style.Numberformat.Format = "d/MM/yyyy hh:mm";
                rawDataSheet.Cells.AutoFitColumns();

                ExcelWorksheet pivotSheet = package.Workbook.Worksheets.Add("Report");
                ExcelRange dataRange = rawDataSheet.Cells["A1:" + rawDataSheet.Dimension.End.Address];

                var pivotTable = pivotSheet.PivotTables.Add(pivotSheet.Cells["A1"], dataRange, "PivotTable");
                if (_chartType == "Yield vs WW")
                {
                    pivotTable.RowFields.Add(pivotTable.Fields["Work Week"]);
                    pivotTable.DataOnRows = false;
                }
                else
                {
                    pivotTable.RowFields.Add(pivotTable.Fields["Month"]);
                    pivotTable.DataOnRows = false;
                }

                // data fields
                pivotTable.Fields.AddCalculatedField("Bin1%", "= 'Unit(s) Passed'/( 'Unit(s) Passed'+ 'Unit(s) Rejected')");

                var field = pivotTable.DataFields.Add(pivotTable.Fields["Bin1%"]);
                field.Name = "Sum of Bin1%";
                field.Function = DataFieldFunctions.Sum;
                field.Format = "0.00%";

                var chart = pivotSheet.Drawings.AddChart("PivotChart", eChartType.Line, pivotTable);
                chart.SetPosition(1, 0, 4, 0);
                chart.SetSize(720, 240);

                if (_chartType == "Yield vs WW")
                {
                    pivotTable.RowHeaderCaption = "WorkWeek";
                }
                else
                {
                    pivotTable.RowHeaderCaption = "Month";
                }

                pivotSheet.Select("A1");

                package.Save();
                if (_autoLaunch)
                {
                    System.Diagnostics.Process.Start(_excelPath);
                }
            }
        }
    }
}
