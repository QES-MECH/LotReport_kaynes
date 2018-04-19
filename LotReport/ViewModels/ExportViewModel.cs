using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Framework.MVVM;
using LotReport.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LotReport.ViewModels
{
    public class ExportViewModel : PropertyChangedBase
    {
        private string _exportPath;

        public ExportViewModel()
        {
            WireCommands();
        }

        public IMessageBoxService MessageBoxService { get; set; } = new MessageBoxService();

        public string ExportPath { get => _exportPath; set => SetProperty(ref _exportPath, value); }

        public AsyncCommand<object> ExportCommand { get; private set; }

        private void WireCommands()
        {
            ExportCommand = AsyncCommand.Create(
                async param =>
                {
                    LotData lotData = param as LotData;
                    if (lotData == null)
                    {
                        return;
                    }

                    SaveFileDialog dialog = new SaveFileDialog
                    {
                        Filter = "Excel Workbook|*.xlsx",
                        FileName = Path.GetFileNameWithoutExtension(lotData.FileInfo.Name)
                    };

                    bool? result = dialog.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        ExportPath = dialog.FileName;
                        await Task.Run(() => GenerateExcelReport(ExportPath, lotData));
                    }
                });
        }

        private void GenerateExcelReport(string excelPath, LotData lotData)
        {
            var excelFile = new FileInfo(excelPath);
            if (excelFile.Exists)
            {
                try
                {
                    excelFile.Delete();
                }
                catch (Exception e)
                {
                    ShowMessage(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            using (var package = new ExcelPackage(excelFile))
            {
                GenerateSummary(LeadFrameMap.Type.Modified, lotData, package);
                GenerateMapping(LeadFrameMap.Type.Modified, lotData, package);
                GenerateSummary(LeadFrameMap.Type.Vision, lotData, package);
                GenerateMapping(LeadFrameMap.Type.Vision, lotData, package);
                GenerateMarkVerification(lotData, package);
                package.Save();
            }
        }

        private void GenerateSummary(LeadFrameMap.Type type, LotData lotData, ExcelPackage package)
        {
            ExcelWorksheet summaryWorksheet = null;

            switch (type)
            {
                case LeadFrameMap.Type.Vision:
                    summaryWorksheet = package.Workbook.Worksheets.Add("Machine Summary");
                    break;
                case LeadFrameMap.Type.Modified:
                    summaryWorksheet = package.Workbook.Worksheets.Add("Operator Summary");
                    break;
            }

            summaryWorksheet.Cells["A1"].Value = "Macine ID";
            summaryWorksheet.Cells["A2"].Value = "Lot ID";
            summaryWorksheet.Cells["A3"].Value = "Operator ID";
            summaryWorksheet.Cells["A4"].Value = "Product Code";
            summaryWorksheet.Cells["A5"].Value = "Die Attach";
            summaryWorksheet.Cells["A6"].Value = "Wire Bond #1";
            summaryWorksheet.Cells["A7"].Value = "Wire Bond #2";
            summaryWorksheet.Cells["A8"].Value = "Bonding Diagram";
            summaryWorksheet.Cells["A9"].Value = "Recipe";
            summaryWorksheet.Cells["A10"].Value = "LeadFrame Unit(s)";
            summaryWorksheet.Cells["A11"].Value = "LeadFrame X Unit(s)";
            summaryWorksheet.Cells["A12"].Value = "LeadFrame Y Unit(s)";
            summaryWorksheet.Cells["A13"].Value = "Start Time";
            summaryWorksheet.Cells["A14"].Value = "End Time";
            summaryWorksheet.Cells["A15"].Value = "UPH";
            summaryWorksheet.Cells["A16"].Value = "LeadFrame(s) Inspected";
            summaryWorksheet.Cells["A17"].Value = "Unit(s) Passed";
            summaryWorksheet.Cells["A18"].Value = "Unit(s) Rejected";
            summaryWorksheet.Cells["A19"].Value = "Unit(s) False-Called";
            summaryWorksheet.Cells["A20"].Value = "Unit(s) Yield (%)";
            summaryWorksheet.Cells["A21"].Value = "False-Called (%)";
            summaryWorksheet.Cells["A22"].Value = "Marked Unit(s)";
            summaryWorksheet.Cells["A23"].Value = "Unmarked Unit(s)";
            summaryWorksheet.Cells["A24"].Value = "Marked Unit(s) Passed";
            summaryWorksheet.Cells["A25"].Value = "Marked Unit(s) Rejected";
            summaryWorksheet.Cells["A26"].Value = "Marked Unit(s) Yield (%)";

            summaryWorksheet.Cells["B1"].Value = lotData.MachineId;
            summaryWorksheet.Cells["B2"].Value = lotData.LotId;
            summaryWorksheet.Cells["B3"].Value = lotData.OperatorId;
            summaryWorksheet.Cells["B4"].Value = lotData.ProductCode;
            summaryWorksheet.Cells["B5"].Value = lotData.DieAttach;
            summaryWorksheet.Cells["B6"].Value = lotData.WireBond0;
            summaryWorksheet.Cells["B7"].Value = lotData.WireBond1;
            summaryWorksheet.Cells["B8"].Value = lotData.BondingDiagram;
            summaryWorksheet.Cells["B9"].Value = lotData.RecipeName;
            summaryWorksheet.Cells["B10"].Value = lotData.LeadFrameUnits;
            summaryWorksheet.Cells["B11"].Value = lotData.LeadFrameXUnits;
            summaryWorksheet.Cells["B12"].Value = lotData.LeadFrameYUnits;
            summaryWorksheet.Cells["B13"].Value = lotData.StartTime.ToString();
            summaryWorksheet.Cells["B14"].Value = lotData.EndTime.ToString();
            summaryWorksheet.Cells["B15"].Value = Math.Round(lotData.UPH, 3);
            summaryWorksheet.Cells["B16"].Value = lotData.LeadFramesInspected;
            summaryWorksheet.Cells["B17"].Value = lotData.UnitsPassed;
            summaryWorksheet.Cells["B18"].Value = lotData.UnitsRejected;
            summaryWorksheet.Cells["B19"].Value = lotData.UnitsOverRejected;
            summaryWorksheet.Cells["B20"].Value = Math.Round(lotData.UnitsYieldPercentage, 3);
            summaryWorksheet.Cells["B21"].Value = Math.Round(lotData.OverRejectPercentage, 3);
            summaryWorksheet.Cells["B22"].Value = lotData.MarkedUnits;
            summaryWorksheet.Cells["B23"].Value = lotData.UnmarkedUnits;
            summaryWorksheet.Cells["B24"].Value = lotData.MarkedUnitsPassed;
            summaryWorksheet.Cells["B25"].Value = lotData.MarkedUnitsRejected;
            summaryWorksheet.Cells["B26"].Value = Math.Round(lotData.MarkedUnitsYieldPercentage, 3);

            BinCodeRepository repository = new BinCodeRepository();
            repository.LoadFromFile();

            summaryWorksheet.Cells["A28"].Value = "Defect Type";
            summaryWorksheet.Cells["B28"].Value = "Count";
            summaryWorksheet.Cells["A28:B28"].Style.Font.Bold = true;

            int row = 29;

            switch (type)
            {
                case LeadFrameMap.Type.Vision:
                    foreach (var bin in lotData.VisionBinCount)
                    {
                        if (bin.Key != 0)
                        {
                            BinCode bc = repository.BinCodes.FirstOrDefault(b => b.Id == bin.Key);
                            summaryWorksheet.Cells[row, 1].Value = string.Format("{0}: {1}", bc.Value, bc.Description);
                            summaryWorksheet.Cells[row, 2].Value = bin.Value;
                            row++;
                        }
                    }

                    break;
                case LeadFrameMap.Type.Modified:
                    foreach (var bin in lotData.ModifiedBinCount)
                    {
                        if (bin.Key != 0)
                        {
                            BinCode bc = repository.BinCodes.FirstOrDefault(b => b.Id == bin.Key);
                            summaryWorksheet.Cells[row, 1].Value = string.Format("{0}: {1}", bc.Value, bc.Description);
                            summaryWorksheet.Cells[row, 2].Value = bin.Value;
                            row++;
                        }
                    }

                    break;
            }

            summaryWorksheet.Cells["B1:B100"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells.AutoFitColumns();
        }

        private void GenerateMapping(LeadFrameMap.Type type, LotData lotData, ExcelPackage package)
        {
            ExcelWorksheet mappingWorksheet = null;

            switch (type)
            {
                case LeadFrameMap.Type.Vision:
                    mappingWorksheet = package.Workbook.Worksheets.Add("Machine Mapping");
                    break;
                case LeadFrameMap.Type.Modified:
                    mappingWorksheet = package.Workbook.Worksheets.Add("Operator Mapping");
                    break;
            }

            mappingWorksheet.Cells["A1"].Value = "Machine ID";
            mappingWorksheet.Cells["B1"].Value = lotData.MachineId;
            mappingWorksheet.Cells["A2"].Value = "Lot ID";
            mappingWorksheet.Cells["B2"].Value = lotData.LotId;
            mappingWorksheet.Cells["A3"].Value = "Operator";
            mappingWorksheet.Cells["B3"].Value = lotData.OperatorId;
            mappingWorksheet.Cells["A4"].Value = "Recipe";
            mappingWorksheet.Cells["B4"].Value = lotData.RecipeName;

            mappingWorksheet.Cells["A6"].Value = "Pass";
            mappingWorksheet.Cells["B6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            mappingWorksheet.Cells["B6"].Style.Fill.BackgroundColor.SetColor(Color.Green);
            mappingWorksheet.Cells["A7"].Value = "Fail";
            mappingWorksheet.Cells["B7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            mappingWorksheet.Cells["B7"].Style.Fill.BackgroundColor.SetColor(Color.Red);
            mappingWorksheet.Cells["A8"].Value = "False Call";
            mappingWorksheet.Cells["B8"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            mappingWorksheet.Cells["B8"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

            string[] leadFramePaths = Directory.GetFiles(lotData.FileInfo.Directory.FullName, "*.xml", SearchOption.AllDirectories);
            List<LeadFrameMap> leadFrameMaps = new List<LeadFrameMap>();

            foreach (string lfPath in leadFramePaths)
            {
                LeadFrameMap leadFrameMap = null;
                switch (type)
                {
                    case LeadFrameMap.Type.Vision:
                        leadFrameMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Vision);
                        break;
                    case LeadFrameMap.Type.Modified:
                        leadFrameMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Modified);
                        break;
                }

                leadFrameMaps.Add(leadFrameMap);
            }

            int startingRow = 9;
            foreach (var lfMap in leadFrameMaps)
            {
                startingRow++;
                int headerRow = startingRow;
                mappingWorksheet.Cells[headerRow, 1].Value = lfMap.LeadFrameId;

                for (int x = 0; x < lfMap.SumOfXDies; x++)
                {
                    mappingWorksheet.Cells[startingRow, 3 + x].Value = x + 1;
                }

                int startingColumn = 2;
                int imageStartingColumn = startingColumn + lfMap.SumOfXDies;
                int y = 0;
                foreach (DieRow dieRow in lfMap.Rows)
                {
                    startingRow++;
                    mappingWorksheet.Cells[startingRow, startingColumn].Value = ++y;

                    foreach (Die die in dieRow.Dies)
                    {
                        mappingWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        mappingWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        if (die.BinCode.Id == 0)
                        {
                            mappingWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        }
                        else
                        {
                            mappingWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }

                        if (die.Modified)
                        {
                            if (die.BinCode.Id == 0)
                            {
                                mappingWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                            }
                        }
                    }
                }

                startingRow++;
            }

            mappingWorksheet.Cells.AutoFitColumns();
        }

        private void GenerateMarkVerification(LotData lotData, ExcelPackage package)
        {
            ExcelWorksheet markWorksheet = package.Workbook.Worksheets.Add("Mark Verification");

            markWorksheet.Cells["A1"].Value = "Machine ID";
            markWorksheet.Cells["B1"].Value = lotData.MachineId;
            markWorksheet.Cells["A2"].Value = "Lot ID";
            markWorksheet.Cells["B2"].Value = lotData.LotId;
            markWorksheet.Cells["A3"].Value = "Operator";
            markWorksheet.Cells["B3"].Value = lotData.OperatorId;
            markWorksheet.Cells["A4"].Value = "Recipe";
            markWorksheet.Cells["B4"].Value = lotData.RecipeName;

            markWorksheet.Cells["A6"].Value = "Good";
            markWorksheet.Cells["B6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            markWorksheet.Cells["B6"].Style.Fill.BackgroundColor.SetColor(Color.Green);
            markWorksheet.Cells["A7"].Value = "Mark Passed";
            markWorksheet.Cells["B7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            markWorksheet.Cells["B7"].Style.Fill.BackgroundColor.SetColor(Color.Cyan);
            markWorksheet.Cells["A8"].Value = "Mark Failed";
            markWorksheet.Cells["B8"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            markWorksheet.Cells["B8"].Style.Fill.BackgroundColor.SetColor(Color.Magenta);
            markWorksheet.Cells["A9"].Value = "Not Marked";
            markWorksheet.Cells["B9"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            markWorksheet.Cells["B9"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

            string[] leadFramePaths = Directory.GetFiles(lotData.FileInfo.Directory.FullName, "*.xml", SearchOption.AllDirectories);
            List<LeadFrameMap> leadFrameMaps = new List<LeadFrameMap>();

            foreach (string lfPath in leadFramePaths)
            {
                LeadFrameMap leadFrameMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Modified);
                leadFrameMaps.Add(leadFrameMap);
            }

            int startingRow = 10;
            foreach (var lfMap in leadFrameMaps)
            {
                startingRow++;
                int headerRow = startingRow;
                markWorksheet.Cells[headerRow, 1].Value = lfMap.LeadFrameId;

                for (int x = 0; x < lfMap.SumOfXDies; x++)
                {
                    markWorksheet.Cells[startingRow, 3 + x].Value = x + 1;
                }

                int startingColumn = 2;
                int imageStartingColumn = startingColumn + lfMap.SumOfXDies;
                int y = 0;
                foreach (DieRow dieRow in lfMap.Rows)
                {
                    startingRow++;
                    markWorksheet.Cells[startingRow, startingColumn].Value = ++y;

                    foreach (Die die in dieRow.Dies)
                    {
                        markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        switch (die.MarkStatus)
                        {
                            case Die.Mark.NA:
                                markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                break;
                            case Die.Mark.Pass:
                                markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                                break;
                            case Die.Mark.Fail:
                                markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Magenta);
                                break;
                        }

                        if (die.BinCode.Id != 0 && die.MarkStatus == Die.Mark.NA)
                        {
                            markWorksheet.Cells[startingRow, startingColumn + (int)die.Coordinate.X].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        }
                    }
                }

                startingRow++;
            }

            markWorksheet.Cells.AutoFitColumns();
        }

        private bool ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                return MessageBoxService.Show(text, caption, button, image);
            }
            else
            {
                bool result = false;
                Application.Current.Dispatcher.Invoke(() =>
                    result = MessageBoxService.Show(text, caption, button, image));
                return result;
            }
        }
    }
}
