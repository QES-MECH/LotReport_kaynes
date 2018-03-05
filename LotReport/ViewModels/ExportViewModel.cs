using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.MVVM;
using LotReport.Models;
using Microsoft.Win32;
using OfficeOpenXml;

namespace LotReport.ViewModels
{
    public class ExportViewModel : PropertyChangedBase
    {
        private string _exportPath;

        public ExportViewModel()
        {
            WireCommands();
        }

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
                excelFile.Delete();
            }

            using (var package = new ExcelPackage(excelFile))
            {
                GenerateSummary(LeadFrameMap.Type.Modified, lotData, package);
                GenerateSummary(LeadFrameMap.Type.Vision, lotData, package);
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
            summaryWorksheet.Cells["B15"].Value = lotData.UPH;
            summaryWorksheet.Cells["B16"].Value = lotData.LeadFramesInspected;
            summaryWorksheet.Cells["B17"].Value = lotData.UnitsPassed;
            summaryWorksheet.Cells["B18"].Value = lotData.UnitsRejected;
            summaryWorksheet.Cells["B19"].Value = lotData.UnitsOverRejected;
            summaryWorksheet.Cells["B20"].Value = lotData.UnitsYieldPercentage;
            summaryWorksheet.Cells["B21"].Value = lotData.OverRejectPercentage;
            summaryWorksheet.Cells["B22"].Value = lotData.MarkedUnits;
            summaryWorksheet.Cells["B23"].Value = lotData.UnmarkedUnits;
            summaryWorksheet.Cells["B24"].Value = lotData.MarkedUnitsPassed;
            summaryWorksheet.Cells["B25"].Value = lotData.MarkedUnitsRejected;
            summaryWorksheet.Cells["B26"].Value = lotData.MarkedUnitsYieldPercentage;

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

            summaryWorksheet.Cells["B1:B100"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells.AutoFitColumns();
        }
    }
}
