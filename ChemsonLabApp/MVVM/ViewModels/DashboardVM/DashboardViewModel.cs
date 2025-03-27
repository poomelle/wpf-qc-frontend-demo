using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.DashboardVM.Command;
using ChemsonLabApp.MVVM.Views.Dashboard;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Utilities;
using ChemsonLabApp.Constants;
using PropertyChanged;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Win32;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Bibliography;
using ClosedXML;
using OfficeOpenXml.Drawing.Chart;
using ChemsonLabApp.Services.IService;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using ChemsonLabApp.RestAPI.IRestAPI;

namespace ChemsonLabApp.MVVM.ViewModels.DashboardVM
{
    [AddINotifyPropertyChangedInterface]
    public class DashboardViewModel
    {
        public List<string> DashboardMenus { get; set; }
        public string SelectedMenu { get; set; }
        public List<string> Products { get; set; }
        public string SelectedProduct { get; set; } = "All";
        public List<string> Instruments { get; set; }
        public string SelectedInstrument { get; set; } = "All";
        public List<string> Attempts { get; set; }
        public string SelectedAttempt { get; set; }
        public List<string> Years { get; set; }
        public string FromYear { get; set; }
        public string ToYear { get; set; }
        public List<string> Months { get; set; }
        public string FromMonth { get; set; }
        public string ToMonth { get; set; }
        public bool IsYearly { get; set; } = true;
        public ContentControl KPIContentControl { get; set; }

        // local properties
        private List<QcPerformanceKpi> qcPerformanceKpis = new List<QcPerformanceKpi>();
        private List<QCLabel> qCLabels = new List<QCLabel>();
        private List<QcAveTestTimeKpi> qcAveTestTimeKpis = new List<QcAveTestTimeKpi>();

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IProductService _productService;
        private readonly IInstrumentService _instrumentService;
        private readonly IQcLabelRestAPI _qcLabelRestAPI;
        private readonly IQcAveTimeKpiRestApi _qcAveTimeKpiRestApi;
        private readonly IQcPerformanceKpiRestAPI _qcPerformanceKpiRestAPI;

        // Commands
        public GenerateKPICommand GenerateKPICommand { get; set; }
        public ExportKPICommand ExportKPICommand { get; set; }

        public DashboardViewModel(
            IServiceScopeFactory serviceScopeFactory,
            IProductService productService,
            IInstrumentService instrumentService,
            IQcLabelRestAPI qcLabelRestAPI,
            IQcAveTimeKpiRestApi qcAveTimeKpiRestApi,
            IQcPerformanceKpiRestAPI qcPerformanceKpiRestAPI
            )
        {
            // Services
            this._serviceScopeFactory = serviceScopeFactory;
            this._productService = productService;
            this._instrumentService = instrumentService;
            this._qcLabelRestAPI = qcLabelRestAPI;
            this._qcAveTimeKpiRestApi = qcAveTimeKpiRestApi;
            this._qcPerformanceKpiRestAPI = qcPerformanceKpiRestAPI;

            // Commands
            GenerateKPICommand = new GenerateKPICommand(this);
            ExportKPICommand = new ExportKPICommand(this);

            // Initialise
            initializeParameter();
            GenerateKPI();
        }

        private async void initializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                LoadDashboardMenusComboBoxes();
                LoadYearsMonths();
                await LoadProductsAndInstrument();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error loading Products and Instruments, Please check internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error loading Products and Instruments");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        private async Task LoadProductsAndInstrument()
        {
            try
            {
                Products = new List<string> { "All" }.Concat(await _productService.GetAllActiveProductName()).ToList();
                Instruments = new List<string> { "All" }.Concat(await _instrumentService.GetAllActiveInstrumentName()).ToList();
            }
            catch (Exception e)
            {
                NotificationUtility.ShowError("Error loading Products and Instruments");
                LoggerUtility.LogError(e);
            }
        }

        private void LoadDashboardMenusComboBoxes()
        {
            DashboardMenus = new List<string>
            {
                "Conformance",
                "QC First Time Pass",
                "Average Test Time",
                "QC Label Printed",
            };
            SelectedMenu = "Conformance";

            Attempts = new List<string>
            {
                "1",
                "2",
                "3",
            };
            SelectedAttempt = "3";
        }

        private void LoadYearsMonths()
        {
            string startYear = "2020";
            string currentYear = DateTime.Now.Year.ToString();

            Years = new List<string>();
            for (int i = int.Parse(startYear); i <= int.Parse(currentYear); i++)
            {
                Years.Add(i.ToString());
            }

            FromYear = currentYear;
            ToYear = currentYear;

            Months = Constants.Constants.Months;
            FromMonth = "All";
            ToMonth = "All";
        }

        public void GenerateKPI()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var conformanceViewModel = scope.ServiceProvider.GetRequiredService<ConformanceViewModel>();
                    conformanceViewModel.SelectedPerformance = SelectedMenu;
                    conformanceViewModel.SelectedProduct = SelectedProduct;
                    conformanceViewModel.SelectedInstrument = SelectedInstrument;
                    conformanceViewModel.FromMonth = FromMonth;
                    conformanceViewModel.ToMonth = ToMonth;
                    conformanceViewModel.SelectedAttempt = SelectedMenu == "QC First Time Pass"? "1" : SelectedAttempt;
                    conformanceViewModel.IsYearly = IsYearly;

                    conformanceViewModel.InitializeParameters(FromYear, ToYear, SelectedMenu);
                    conformanceViewModel.CreateGrph();

                    KPIContentControl = new ConformanceView(conformanceViewModel);
                }

            }
            catch (Exception e)
            {
                NotificationUtility.ShowError("Error generating KPI");
                LoggerUtility.LogError(e);
            }
        }

        // Export Dashboard to Excel
        public async void ExportKPI()
        {
            // 3. Create Excel file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "KPI_Report.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel Files|*.xlsx|All Files|*.*",
                InitialDirectory = @"S:\Lab"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    string fullPath = saveFileDialog.FileName;

                    // 1. Get data from database based on selected parameters
                    await LoadDataFromDatabase(SelectedProduct, SelectedInstrument, FromYear, ToYear, FromMonth, ToMonth);

                    // 2. Create data for each KPI
                    List<QcPerformanceGraphData> qcConformanceData = CreateQcPerformanceData(qcPerformanceKpis, IsYearly, "Conformance");
                    List<QcPerformanceGraphData> qcFirstTimePassData = CreateQcPerformanceData(qcPerformanceKpis, IsYearly, "QC First Time Pass");
                    List<QcPerformanceGraphData> qcLabelPrintedData = CreateQcPerformanceData(qCLabels, IsYearly);
                    List<QcPerformanceGraphData> qcAveTestTime = CreateQcPerformanceData(qcAveTestTimeKpis, IsYearly);

                    // 2.1 Create dictionary for all KPIs data
                    Dictionary<string, List<QcPerformanceGraphData>> allKPIsData = new Dictionary<string, List<QcPerformanceGraphData>>
                    {
                        { "Conformance", qcConformanceData },
                        { "QC First Time Pass", qcFirstTimePassData },
                        { "QC Label Printed", qcLabelPrintedData },
                        { "Average Test Time", qcAveTestTime }
                    };

                    List<string> years = new List<string>();
                    for (int i = int.Parse(FromYear); i <= int.Parse(ToYear); i++)
                    {
                        years.Add(i.ToString());
                    }

                    int tableStartRow = 2;
                    int tableStartColumn = 2;
                    int tableGap = 4;

                    int graphStartRow = 1;
                    int graphStartColumn = 5;
                    int chartPlotWidth = 800;
                    int chartPlotHeight = 300;
                    int chartPlotGap = 5;

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage())
                    {
                        foreach (var kpiData in allKPIsData)
                        {
                            var worksheet = package.Workbook.Worksheets.Add(kpiData.Key);

                            // Add data to the worksheet
                            PopulateWorksheet(worksheet, kpiData.Key, kpiData.Value, IsYearly, years, tableStartRow, tableStartColumn, tableGap);

                            // Create Chart for each worksheet
                            CreateKPIChart(worksheet, kpiData.Key, kpiData.Value, IsYearly, years, graphStartRow, graphStartColumn, 
                                chartPlotWidth, chartPlotHeight, chartPlotGap, tableStartRow, tableGap);
                        }

                        FileInfo fileInfo = new FileInfo(fullPath);
                        package.SaveAs(fileInfo);

                    }

                    NotificationUtility.ShowSuccess("KPI Report Exported Successfully");
                }
                catch (Exception e)
                {
                    NotificationUtility.ShowError("Error exporting KPI Report");
                    LoggerUtility.LogError(e);
                }
            }
        }

        private void CreateKPIChart(ExcelWorksheet worksheet, string key, List<QcPerformanceGraphData> value, bool isYearly, List<string> years, int chartPlotStartRow, int chartPlotStartColumn, int chartPlotWidth, int chartPlotHeight, int chartPlotGap, int tableStartRow, int tableGap)
        {
            var dataStartRow = tableStartRow + 1;

            if (isYearly)
            {
                var chart = worksheet.Drawings.AddChart($"{key}", eChartType.ColumnClustered);
                chart.Title.Text = $"{key}";

                var dataEndRow = years.Count + dataStartRow - 1;

                var series = chart.Series.Add(worksheet.Cells[$"C{dataStartRow}:C{dataEndRow}"], worksheet.Cells[$"B{dataStartRow}:B{dataEndRow}"]);
                series.Header = SelectedProduct;
                // show data labels
                var barSeries = (ExcelBarChartSerie)series;
                barSeries.DataLabel.ShowValue = true;
                barSeries.DataLabel.Position = eLabelPosition.OutEnd;

                chart.XAxis.Title.Text = "Year";

                chart.YAxis.Title.Text = key == "Average Test Time" ? $"(mm:ss)" : $"(%)";
                chart.YAxis.MinValue = 0;
                chart.YAxis.MaxValue = key == "Average Test Time" ? 50 : 110;
                chart.YAxis.RemoveGridlines();

                chart.SetPosition(chartPlotStartRow, 0, chartPlotStartColumn, 0);
                chart.SetSize(chartPlotWidth, chartPlotHeight);
            }
            else
            {

                foreach (var year in years)
                {
                    var chart = worksheet.Drawings.AddChart($"{key} {year}", eChartType.ColumnClustered);
                    chart.Title.Text = $"{key} {year}";

                    var dataEndRow = 12 + dataStartRow - 1;

                    var series = chart.Series.Add(worksheet.Cells[$"C{dataStartRow}:C{dataEndRow}"], worksheet.Cells[$"B{dataStartRow}:B{dataEndRow}"]);
                    series.Header = SelectedProduct;
                    // show data labels
                    var barSeries = (ExcelBarChartSerie)series;
                    barSeries.DataLabel.ShowValue = true;
                    barSeries.DataLabel.Position = eLabelPosition.OutEnd;

                    chart.XAxis.Title.Text = "Month";

                    chart.YAxis.Title.Text = key == "Average Test Time" ? $"(mm:ss)" : $"(%)";
                    chart.YAxis.MinValue = 0;
                    chart.YAxis.MaxValue = key == "Average Test Time" ? 50 : 110; ;
                    chart.YAxis.RemoveGridlines();

                    chart.SetPosition(chartPlotStartRow, 0, chartPlotStartColumn, 0);
                    chart.SetSize(chartPlotWidth, chartPlotHeight);

                    chartPlotStartRow += (12 + chartPlotGap);

                    dataStartRow += (12 + tableGap + 1);
                    
                }
            }
        }

        private void PopulateWorksheet(ExcelWorksheet worksheet, string key, List<QcPerformanceGraphData> value, bool isYearly, List<string> years, int startRow, int startColumn, int gap)
        {
            if (isYearly)
            {
                // Populate table header
                worksheet.Cells[startRow, startColumn].Value = "Year";
                worksheet.Cells[startRow++, startColumn + 1].Value = key == "Average Test Time" ? $"{key} (mm:ss)" : $"{key} (%)";

                // Populate Graph Data
                foreach (var year in years)
                {
                    var graphData = value.Find(qc => qc.XAxisValue == year);
                    double yAxisValue = graphData?.YAxisValue ?? 0;

                    worksheet.Cells[startRow, startColumn].Value = year;
                    worksheet.Cells[startRow++, startColumn + 1].Value = Math.Round(yAxisValue, 2);
                }
            }
            else
            {
                foreach (var year in years)
                {
                    worksheet.Cells[startRow, startColumn].Value = year;
                    worksheet.Cells[startRow++, startColumn + 1].Value = key == "Average Test Time" ? $"{key} (mm:ss)" : $"{key} (%)";


                    for (int i = 1; i < Constants.Constants.Months.Count; i++)
                    {
                        var graphData = value.Find(qc => qc.XAxisValue.Contains(year) && qc.XAxisValue.Contains(Constants.Constants.Months[i]));
                        double yAxisValue = graphData?.YAxisValue ?? 0;

                        worksheet.Cells[startRow, startColumn].Value = Constants.Constants.Months[i];
                        worksheet.Cells[startRow++, startColumn + 1].Value = Math.Round(yAxisValue, 2);
                    }
                    startRow += gap;
                }
            }

            worksheet.Cells.AutoFitColumns(2);
        }

        private async Task LoadDataFromDatabase(string selectedProduct, string selectedInstrument, string fromYear, string toYear, string fromMonth, string toMonth)
        {
            qcPerformanceKpis.Clear();
            qCLabels.Clear();
            qcAveTestTimeKpis.Clear();

            List<string> filters = GetLoadDataFilter(selectedProduct, selectedInstrument, fromYear, toYear, fromMonth, toMonth);

            qcPerformanceKpis = await LoadQcPerformanceFromDatabase(filters);
            qCLabels = await LoadQcLabelFromDatabase(filters);
            qcAveTestTimeKpis = await LoadQcAveTestTimeFromDatabase(filters);
        }

        private List<QcPerformanceGraphData> CreateQcPerformanceData(List<QcAveTestTimeKpi> qcAveTestTimeKpis, bool isYearly)
        {
            List<QcPerformanceGraphData> qcPerformanceGraphDatas = new List<QcPerformanceGraphData>();
            qcAveTestTimeKpis = qcAveTestTimeKpis.OrderBy(qc => qc.year).ThenBy(qc => qc.month).ToList();

            if (isYearly) // Create yearly data
            {
                var years = qcAveTestTimeKpis.Select(qc => qc.year).Distinct().ToList();
                foreach (var year in years)
                {
                    var inYearQcAveTestTimeKpis = qcAveTestTimeKpis.Where(qc => qc.year == year).ToList();

                    // Create QcPerformanceGraphData for each year, X-axis: Year, Y-axis: Average value of aveTestTime
                    var aveTestTime = inYearQcAveTestTimeKpis.Count() == 0 ? 0 : inYearQcAveTestTimeKpis.Average(x => x.aveTestTime);

                    long aveTestTimeTicks = Convert.ToInt64(aveTestTime);
                    TimeSpan aveTestTimeSpan = TimeSpan.FromTicks(aveTestTimeTicks);

                    // Adding 19 minute of sample preparing time (constant)
                    if (aveTestTimeTicks != 0)
                    {
                        aveTestTimeSpan = aveTestTimeSpan.Add(Constants.Constants.SamplePrepareTimeSpan);
                    }

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = year,
                        YAxisValue = aveTestTimeSpan.TotalMinutes
                    };

                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }
            else // Create monthly data
            {
                var months = qcAveTestTimeKpis.Select(qc => $"{qc.year}/{qc.month}").Distinct().ToList();
                foreach (var month in months)
                {
                    var year = month.Split('/')[0];
                    var monthName = month.Split('/')[1];
                    var inMonthQcAveTestTimeKpis = qcAveTestTimeKpis.Where(qc => qc.year == year && qc.month == monthName).ToList();

                    // Create QcPerformanceGraphData for each month each year, X-axis: Year/Month, Y-axis: Average value of aveTestTime
                    var aveTestTime = inMonthQcAveTestTimeKpis.Count() == 0 ? 0 : inMonthQcAveTestTimeKpis.Average(x => x.aveTestTime);

                    long aveTestTimeTicks = Convert.ToInt64(aveTestTime);
                    TimeSpan aveTestTimeSpan = TimeSpan.FromTicks(aveTestTimeTicks);

                    // Adding 19 minute of sample preparing time (constant)
                    if (aveTestTimeTicks != 0)
                    {
                        aveTestTimeSpan = aveTestTimeSpan.Add(Constants.Constants.SamplePrepareTimeSpan);
                    }

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = $"{year}/{TestMonthUtility.ConvertMonth(monthName)}",
                        YAxisValue = aveTestTimeSpan.TotalMinutes
                    };

                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }
            return qcPerformanceGraphDatas;
        }
        

        private List<QcPerformanceGraphData> CreateQcPerformanceData(List<QcPerformanceKpi> qcPerformanceKpis, bool isYearly, string performance)
        {
            List<QcPerformanceGraphData> qcPerformanceGraphDatas = new List<QcPerformanceGraphData>();
            // Arrange qcPerformanceKpis by year and month
            qcPerformanceKpis = qcPerformanceKpis.OrderBy(qc => qc.year).ThenBy(qc => qc.month).ToList();

            if (isYearly)
            {
                // Create QcPerformanceGraphData for each year, X-axis: Year, Y-axis: Conformance (percentage)
                var years = qcPerformanceKpis.Select(qc => qc.year).Distinct().ToList();
                foreach (var year in years)
                {
                    var inYearQcPerformanceKpis = qcPerformanceKpis.Where(qc => qc.year == year).ToList();
                    // sum of totalTest
                    int totalTest = inYearQcPerformanceKpis.Sum(qc => qc.totalTest);
                    // sum of firstPass, secondPass and thirdPass
                    int totalPass = 0;
                    if (performance == "Conformance")
                    {
                        totalPass = inYearQcPerformanceKpis.Sum(qc => qc.firstPass + qc.secondPass + qc.thirdPass);
                    }
                    else if (performance == "QC First Time Pass")
                    {
                        totalPass = inYearQcPerformanceKpis.Sum(qc => qc.firstPass);
                    }

                    double conformance = totalTest == 0 ? 0 : Math.Round((double)totalPass / totalTest * 100, 2);

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = year,
                        YAxisValue = conformance
                    };
                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }
            else
            {
                // Create QcPerformanceGraphData for each month each year, X-axis: Year/Month, Y-axis: conformance (percentage)
                var months = qcPerformanceKpis.Select(qc => $"{qc.year}/{qc.month}").Distinct().ToList();
                foreach (var month in months)
                {
                    var year = month.Split('/')[0];
                    var monthName = month.Split('/')[1];
                    var inMonthQcPerformanceKpis = qcPerformanceKpis.Where(qc => qc.year == year && qc.month == monthName).ToList();
                    // sum of totalTest
                    int totalTest = inMonthQcPerformanceKpis.Sum(qc => qc.totalTest);
                    // sum of firstPass, secondPass and thirdPass
                    int totalPass = 0;
                    if (performance == "Conformance")
                    {
                        totalPass = inMonthQcPerformanceKpis.Sum(qc => qc.firstPass + qc.secondPass + qc.thirdPass);
                    }
                    else if (performance == "QC First Time Pass")
                    {
                        totalPass = inMonthQcPerformanceKpis.Sum(qc => qc.firstPass);
                    }

                    double conformance = totalTest == 0 ? 0 : Math.Round((double)totalPass / totalTest * 100, 2);

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = $"{year}/{TestMonthUtility.ConvertMonth(monthName)}",
                        YAxisValue = conformance
                    };

                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }

            return qcPerformanceGraphDatas;
        }

        private List<QcPerformanceGraphData> CreateQcPerformanceData(List<QCLabel> qCLabels, bool isYearly)
        {
            List<QcPerformanceGraphData> qcPerformanceGraphDatas = new List<QcPerformanceGraphData>();
            // Arrange qcLabels by year and month
            qCLabels = qCLabels.OrderBy(qc => qc.year).ThenBy(qc => qc.month).ToList();

            if (isYearly)
            {
                // Create QcPerformanceGraphData for each year, X-axis: Year, Y-axis: Number of labels printed (count)
                var years = qCLabels.Select(qc => qc.year).Distinct().ToList();
                foreach (var year in years)
                {
                    var numberOfPrintedLabel = qCLabels.Where(qc => qc.year == year).Count();
                    var numberOfTotalPass = qcPerformanceKpis.Where(qcPerformanceKpis => qcPerformanceKpis.year == year).Sum(qc => qc.firstPass + qc.secondPass + qc.thirdPass);

                    double conformance = numberOfTotalPass == 0 ? 0 : Math.Round((double)numberOfPrintedLabel / numberOfTotalPass * 100, 2);

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = year,
                        YAxisValue = conformance
                    };
                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }
            else
            {
                // Create QcPerformanceGraphData for each month each year, X-axis: Year/Month, Y-axis: Number of labels printed (count)
                var months = qCLabels.Select(qc => $"{qc.year}/{qc.month}").Distinct().ToList();
                foreach (var month in months)
                {
                    var year = month.Split('/')[0];
                    var monthName = month.Split('/')[1];
                    var numberOfPrintedLabel = qCLabels.Where(qc => qc.year == year && qc.month == monthName).Count();
                    var numberOfTotalPass = qcPerformanceKpis.Where(qcKpis => qcKpis.year == year && qcKpis.month == monthName).Sum(qc => qc.firstPass + qc.secondPass + qc.thirdPass);

                    double conformance = numberOfTotalPass == 0 ? 0 : Math.Round((double)numberOfPrintedLabel / numberOfTotalPass * 100, 2);

                    var qcPerformanceGraphData = new QcPerformanceGraphData
                    {
                        XAxisValue = $"{year}/{TestMonthUtility.ConvertMonth(monthName)}",
                        YAxisValue = conformance
                    };
                    qcPerformanceGraphDatas.Add(qcPerformanceGraphData);
                }
            }

            return qcPerformanceGraphDatas;
        }

        private async Task<List<QcPerformanceKpi>> LoadQcPerformanceFromDatabase(List<string> filters)
        {
            List<QcPerformanceKpi> qcKpis = new List<QcPerformanceKpi>();

            foreach (var filter in filters)
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var qcPerformanceKpi = await _qcPerformanceKpiRestAPI.GetAllQcPerformanceKpisAsync(filter);
                    qcKpis.AddRange(qcPerformanceKpi);
                }
            }

            return qcKpis;
        }

        private async Task<List<QCLabel>> LoadQcLabelFromDatabase(List<string> filters)
        {
            List<QCLabel> qCLabels = new List<QCLabel>();
            foreach (var filter in filters)
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var qcPrintedLabels = await _qcLabelRestAPI.GetAllQCLabelsAsync(filter);
                    qCLabels.AddRange(qcPrintedLabels);
                }
            }

            return qCLabels;
        }

        private async Task<List<QcAveTestTimeKpi>> LoadQcAveTestTimeFromDatabase(List<string> filters)
        {
            List<QcAveTestTimeKpi> qcAveTestTimeKpis = new List<QcAveTestTimeKpi>();

            foreach (var filter in filters)
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var qcAveTestTimeKpi = await _qcAveTimeKpiRestApi.GetAllQcAveTestTimeKpisAsync(filter);
                    qcAveTestTimeKpis.AddRange(qcAveTestTimeKpi);
                }
            }

            return qcAveTestTimeKpis;
        }

        private List<string> GetLoadDataFilter(string selectedProduct, string selectedInstrument, string fromYear, string toYear, string fromMonth, string toMonth)
        {
            List<string> filterList = new List<string>();

            string filterProductName = selectedProduct == "All" ? "" : selectedProduct;
            string filterMachineName = selectedInstrument == "All" ? "" : selectedInstrument;

            for (int i = int.Parse(fromYear); i <= int.Parse(toYear); i++)
            {
                if (i == int.Parse(fromYear))
                {
                    // filter from "fromMonth" to "December"
                    int startMonthIndex = fromMonth == "All"? 1 : Constants.Constants.Months.IndexOf(fromMonth);
                    int endMonthIndex = 12;

                    for (int j = startMonthIndex; j <= endMonthIndex; j++)
                    {
                        filterList.Add($"?year={i}&month={TestMonthUtility.ConvertMonth(Constants.Constants.Months[j])}&product={filterProductName}&machine={filterMachineName}");
                    }
                }
                else if (i == int.Parse(toYear))
                {
                    // filter from "January" to "toMonth"
                    int startMonthIndex = 1;
                    int endMonthIndex = toMonth == "All" ? 12 : Constants.Constants.Months.IndexOf(toMonth);

                    for (int j = startMonthIndex; j <= endMonthIndex; j++)
                    {
                        filterList.Add($"?year={i}&month={TestMonthUtility.ConvertMonth(Constants.Constants.Months[j])}&product={filterProductName}&machine={filterMachineName}");
                    }
                }
                else
                {
                    // filter from "January" to "December"
                    for (int j = 1; j <= 12; j++)
                    {
                        filterList.Add($"?year={i}&month={TestMonthUtility.ConvertMonth(Constants.Constants.Months[j])}&product={filterProductName}&machine={filterMachineName}");
                    }
                }
            }

            return filterList;
        }
    }
}
