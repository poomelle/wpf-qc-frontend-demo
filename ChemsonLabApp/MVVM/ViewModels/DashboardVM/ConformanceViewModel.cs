using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Office.Interop.Outlook;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.DashboardVM
{
    [AddINotifyPropertyChangedInterface]
    public class ConformanceViewModel
    {
        // Live Chart parameters
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public List<string> Labels { get; set; } = new List<string>();
        public ChartValues<double> ConformanceDataValues { get; set; } = new ChartValues<double>();
        public ChartValues<long> AveTestTimeDataValues { get; set; } = new ChartValues<long>();
        public Func<double, string> Formatter { get; set; }
        public string YAxisTitle { get; set; }
        public double YAxisMinValue { get; set; } = 0;
        public double YAxisMaxValue { get; set; } = 100;

        // Properties for exporting to excel
        public List<string> ExportXAxisValues { get; set; } = new List<string>();
        public List<double> ExportYAxisValues { get; set; } = new List<double>();
        public List<long> ExportYAxisAveTimeValues { get; set; } = new List<long>();

        // Fetch Data from database
        public List<QcPerformanceKpi> QcPerformanceKpis { get; set; } = new List<QcPerformanceKpi>();
        public List<QcAveTestTimeKpi> QcAveTestTimeKpis { get; set; } = new List<QcAveTestTimeKpi>();
        public List<QCLabel> QcLabels { get; set; } = new List<QCLabel>();

        // User input parameters
        public string FromMonth { get; set; }
        public string ToMonth { get; set; }
        public string SelectedPerformance { get; set; }
        public string SelectedProduct { get; set; }
        public string SelectedInstrument { get; set; }
        public string SelectedAttempt { get; set; }
        public bool IsYearly { get; set; }

        // Private variables
        private int fromStartYear;
        private int toEndYear;
        private int fromStartMonthIndex;
        private int toEndMonthIndex;
        private string columnSeriesTitle;

        // services
        private readonly IQcLabelRestAPI _qcLabelRestAPI;
        private readonly IQcPerformanceKpiRestAPI _qcPerformanceKpiRestAPI;
        private readonly IQcAveTimeKpiRestApi _qcAveTimeKpiRestApi;

        public ConformanceViewModel(
            IQcLabelRestAPI qcLabelRestAPI,
            IQcPerformanceKpiRestAPI qcPerformanceKpiRestAPI,
            IQcAveTimeKpiRestApi qcAveTimeKpiRestApi
            )
        {
            // services
            this._qcLabelRestAPI = qcLabelRestAPI;
            this._qcPerformanceKpiRestAPI = qcPerformanceKpiRestAPI;
            this._qcAveTimeKpiRestApi = qcAveTimeKpiRestApi;
        }

        /// <summary>
        /// Initializes the parameters required for generating the conformance graph.
        /// Parses the provided year strings, determines the month indices for the range,
        /// and sets the column series title based on the selected product.
        /// </summary>
        /// <param name="fromYear">The starting year as a string.</param>
        /// <param name="toYear">The ending year as a string.</param>
        /// <param name="selectedProduct">The selected product name.</param>
        public void InitializeParameters(string fromYear, string toYear, string selectedProduct)
        {
            fromStartYear = int.TryParse(fromYear, out fromStartYear) ? fromStartYear : 0;
            toEndYear = int.TryParse(toYear, out toEndYear) ? toEndYear : 0;
            fromStartMonthIndex = Constants.Constants.Months.IndexOf(FromMonth) == 0 ? 1 : Constants.Constants.Months.IndexOf(FromMonth);
            toEndMonthIndex = Constants.Constants.Months.IndexOf(ToMonth) == 0 ? 12 : Constants.Constants.Months.IndexOf(ToMonth);
            columnSeriesTitle = selectedProduct == "All" ? "All Products" : selectedProduct;
        }

        /// <summary>
        /// Updates the Y-axis formatting for the conformance graph based on the selected performance metric.
        /// Sets the Y-axis title and value formatter according to the type of performance selected.
        /// Also updates the maximum value of the Y-axis based on the current data values.
        /// </summary>
        private void UpdateYAxisFormat()
        {
            switch (SelectedPerformance)
            {
                case "Conformance":
                case "QC First Time Pass":
                    YAxisTitle = "QC Pass (%)";
                    Formatter = value => value.ToString("N");
                    break;

                case "Average Test Time":
                    YAxisTitle = "Average Test Time (mm:ss)";
                    Formatter = value => value.ToString("N");
                    break;

                case "QC Label Printed":
                    YAxisTitle = "Printed QC Label (%)";
                    Formatter = value => value.ToString("N");
                    break;
            }

            YAxisMaxValue = ConformanceDataValues.Max() + 10;
        }

        /// <summary>
        /// Asynchronously loads QC performance KPI data and generates the QC conformance graph.
        /// </summary>
        public async void CreateGrph()
        {
            await LoadQcPerformanceKpis();
            GenerateQcConformanceGraph();
        }

        /// <summary>
        /// Asynchronously loads QC performance KPI data and populates the export data for Excel.
        /// </summary>
        public async Task LoadDataForExportAsync()
        {
            await LoadQcPerformanceKpis();
            PopulateExportData();
        }

        /// <summary>
        /// Populates the export data collections for Excel export.
        /// Sets the X-axis values from the chart labels and the Y-axis values from the conformance data values.
        /// </summary>
        private void PopulateExportData()
        {
            // Data for exporting to excel
            // Add X Axis Value
            ExportXAxisValues = Labels;

            // Add Y Axis Value
            ExportYAxisValues = ConformanceDataValues.Select(x => x).ToList();
            //ExportYAxisAveTimeValues = AveTestTimeDataValues.Select(x => x).ToList();
        }

        /// <summary>
        /// Generates the QC conformance graph by populating the data for either yearly or monthly view,
        /// adds a column series to the chart with appropriate labels and formatting, and updates the Y-axis format.
        /// </summary>
        public void GenerateQcConformanceGraph()
        {
            if (IsYearly)
            {
                PopulateYearlyQcConformanceGraphData();
            }
            else
            {
                PopulateMonthlyQcConformanceGraphData();
            }

            SeriesCollection.Add(new ColumnSeries
            {
                Title = columnSeriesTitle,
                Values = ConformanceDataValues,
                DataLabels = true,
                LabelPoint = point => point.Y == 0 ? "No Data"
                    : SelectedPerformance == "Average Test Time" ? TimeSpan.FromMinutes(point.Y).ToString(@"mm\:ss")
                    : SelectedPerformance == "Conformance" || SelectedPerformance == "QC First Time Pass" ? point.Y.ToString("N") + "%"
                    : SelectedPerformance == "QC Label Printed" ? point.Y.ToString("N") + "%"
                    : point.Y.ToString("N")
            });

            // Assign Y Axis Title and Y Axis Min and Max Value
            UpdateYAxisFormat();
        }

        /// <summary>
        /// Populates the yearly QC conformance graph data by iterating from the starting year to the ending year.
        /// For each year in the range, it fetches and adds the corresponding data point to the graph.
        /// </summary>
        private void PopulateYearlyQcConformanceGraphData()
        {
            // Yearly data
            for (var i = fromStartYear; i <= toEndYear; i++)
            {
                FetchDataToGraphData(i, 0, 0);
            }
        }

        /// <summary>
        /// Populates the monthly QC conformance graph data by iterating through the specified range of years and months.
        /// For the case where the start and end years are the same, it fetches data for the months between the start and end month indices.
        /// If the range spans multiple years, it fetches data for each year, handling the first and last years with their respective month ranges,
        /// and all intermediate years with the full range of months.
        /// </summary>
        private void PopulateMonthlyQcConformanceGraphData()
        {
            // monthly data

            // if from year and to year are same
            if (fromStartYear == toEndYear)
            {
                FetchDataToGraphData(fromStartYear, fromStartMonthIndex, toEndMonthIndex);
            }
            // if from year and to year are different
            else if (fromStartYear < toEndYear)
            {
                for (var year = fromStartYear; year <= toEndYear; year++)
                {
                    bool firstYear = year == fromStartYear;
                    bool lastYear = year == toEndYear;

                    int startMonth = firstYear ? fromStartMonthIndex : 1;
                    int endMonth = lastYear ? toEndMonthIndex : 12;

                    FetchDataToGraphData(year, startMonth, endMonth);
                }
            }
        }

        /// <summary>
        /// Populates the graph data for the specified year and month range.
        /// If both startMonth and endMonth are provided (non-zero), it populates monthly data for each month in the range.
        /// Otherwise, it populates yearly data for the given year.
        /// For each data point, it fetches the corresponding QC performance graph data and adds the Y-axis value and X-axis label to the chart collections.
        /// </summary>
        private void FetchDataToGraphData(int year, int startMonth = 0, int endMonth = 0)
        {
            if (startMonth != 0 && endMonth != 0) // Populate monthly data
            {
                for (var monthIndex = startMonth; monthIndex <= endMonth; monthIndex++)
                {
                    var qcPerformanceGraphData = FetchDataToGraphDataEachPoint(year, monthIndex);
                    ConformanceDataValues.Add(qcPerformanceGraphData.YAxisValue);
                    Labels.Add(qcPerformanceGraphData.XAxisValue);
                }
            }
            else // Populate yearly data
            {
                var qcPerformanceGraphData = FetchDataToGraphDataEachPoint(year, 0);
                ConformanceDataValues.Add(qcPerformanceGraphData.YAxisValue);
                Labels.Add(qcPerformanceGraphData.XAxisValue);
            }
        }

        /// <summary>
        /// Fetches and constructs the data point for the QC performance graph for a given year and month index.
        /// Determines which type of data to fetch (Average Test Time, Conformance, QC First Time Pass, or QC Label Printed)
        /// based on the selected performance metric, and returns the corresponding QcPerformanceGraphData object.
        /// </summary>
        /// <param name="year">The year for which to fetch the data.</param>
        /// <param name="monthIndex">The month index (0 for yearly data, otherwise the month number).</param>
        /// <returns>A QcPerformanceGraphData object containing the X and Y axis values for the graph.</returns>
        private QcPerformanceGraphData FetchDataToGraphDataEachPoint(int year, int monthIndex)
        {
            var month = TestMonthUtility.ConvertMonth(Constants.Constants.Months[monthIndex]);
            QcPerformanceGraphData qcPerformanceGraphData = new QcPerformanceGraphData();

            switch (SelectedPerformance)
            {
                case "Average Test Time":
                    qcPerformanceGraphData = FetchAverageTestTimeData(year, monthIndex, month);
                    break;

                case "Conformance":
                case "QC First Time Pass":
                    qcPerformanceGraphData = FetchConformanceData(year, monthIndex, month);
                    break;

                case "QC Label Printed":
                    qcPerformanceGraphData = FetchQcLabelData(year, monthIndex, month);
                    break;
            }

            return qcPerformanceGraphData;
        }

        /// <summary>
        /// Fetches and prepares the data required for the "QC Label Printed" performance metric.
        /// For monthly data (when monthIndex is not zero), it filters QC labels and performance KPIs by year and month.
        /// For yearly data (when monthIndex is zero), it filters only by year.
        /// Returns a QcPerformanceGraphData object containing the calculated percentage of printed QC labels
        /// relative to the total number of passed tests for the specified period.
        /// </summary>
        private QcPerformanceGraphData FetchQcLabelData(int year, int monthIndex, string month)
        {
            List<QCLabel> qcLabels = new List<QCLabel>();
            List<QcPerformanceKpi> qcPerformanceKpis = new List<QcPerformanceKpi>();

            if (monthIndex != 0) // monthly
            {
                qcLabels = QcLabels.Where(x => x.year == year.ToString() && x.month == month).ToList();
                qcPerformanceKpis = QcPerformanceKpis.Where(x => x.year == year.ToString() && x.month == month).ToList();
                return CreateQcPerformanceGraphDataModel(year, month, qcLabels, qcPerformanceKpis);
            }
            else // yearly
            {
                qcLabels = QcLabels.Where(x => x.year == year.ToString()).ToList();
                qcPerformanceKpis = QcPerformanceKpis.Where(x => x.year == year.ToString()).ToList();
                return CreateQcPerformanceGraphDataModel(year, "", qcLabels, qcPerformanceKpis);
            }
        }

        /// <summary>
        /// Fetches and prepares the data required for the "Average Test Time" performance metric.
        /// For monthly data (when monthIndex is not zero), it filters average test time KPIs by year and month.
        /// For yearly data (when monthIndex is zero), it filters only by year.
        /// Returns a QcPerformanceGraphData object containing the calculated average test time
        /// for the specified period.
        /// </summary>
        private QcPerformanceGraphData FetchAverageTestTimeData(int year, int monthIndex, string month)
        {
            List<QcAveTestTimeKpi> qcAveTestTimeKpis = new List<QcAveTestTimeKpi>();

            if (monthIndex != 0) // monthly
            {
                qcAveTestTimeKpis = QcAveTestTimeKpis.Where(x => x.year == year.ToString() && x.month == month).ToList();
                return CreateQcPerformanceGraphDataModel(year, month, qcAveTestTimeKpis);
            }
            else // yearly
            {
                qcAveTestTimeKpis = QcAveTestTimeKpis.Where(x => x.year == year.ToString()).ToList();
                return CreateQcPerformanceGraphDataModel(year, "", qcAveTestTimeKpis);
            }
        }

        /// <summary>
        /// Fetches and prepares the data required for the "Conformance" or "QC First Time Pass" performance metrics.
        /// For monthly data (when monthIndex is not zero), it filters QC performance KPIs by year and month.
        /// For yearly data (when monthIndex is zero), it filters only by year.
        /// Returns a QcPerformanceGraphData object containing the calculated pass percentage
        /// for the specified period.
        /// </summary>
        private QcPerformanceGraphData FetchConformanceData(int year, int monthIndex, string month)
        {
            List<QcPerformanceKpi> qcPerformanceKpis = new List<QcPerformanceKpi>();

            if (monthIndex != 0) // monthly
            {
                qcPerformanceKpis = QcPerformanceKpis.Where(x => x.year == year.ToString() && x.month == month).ToList();
                return CreateQcPerformanceGraphDataModel(year, month, qcPerformanceKpis);
            }
            else // yearly
            {
                qcPerformanceKpis = QcPerformanceKpis.Where(x => x.year == year.ToString()).ToList();
                return CreateQcPerformanceGraphDataModel(year, "", qcPerformanceKpis);
            }
        }

        /// <summary>
        /// Creates a QcPerformanceGraphData model for a given year and month using a list of QcPerformanceKpi.
        /// Sets the X axis value as "MM/YY" if month is provided, otherwise as the year.
        /// Calculates the total number of tests and the number of passes (first, second, third) from the KPI list,
        /// then computes the pass percentage using GetPassPercentageValue and assigns it to the Y axis value.
        /// </summary>
        private QcPerformanceGraphData CreateQcPerformanceGraphDataModel(int year, string month, List<QcPerformanceKpi> qcPerformanceKpis)
        {
            var qcPerformanceGraphData = new QcPerformanceGraphData();

            // assign x axis value
            qcPerformanceGraphData.XAxisValue = month != "" ? $"{TestMonthUtility.ConvertMonth(month)}/{year.ToString().Substring(2)}" : year.ToString();

            var totalTest = qcPerformanceKpis.Sum(x => x.totalTest);
            var firstPass = qcPerformanceKpis.Sum(x => x.firstPass);
            var secondPass = qcPerformanceKpis.Sum(x => x.secondPass);
            var thirdPass = qcPerformanceKpis.Sum(x => x.thirdPass);
            qcPerformanceGraphData.YAxisValue = GetPassPercentageValue(totalTest, firstPass, secondPass, thirdPass);

            return qcPerformanceGraphData;
        }

        /// <summary>
        /// Creates a QcPerformanceGraphData model for a given year and month using a list of QcAveTestTimeKpi.
        /// Sets the X axis value as "MM/YYYY" if month is provided, otherwise as the year.
        /// Calculates the average test time in ticks, converts it to a TimeSpan, and adds a constant sample preparation time if applicable.
        /// The Y axis value is set to the total minutes of the resulting TimeSpan.
        /// </summary>
        private QcPerformanceGraphData CreateQcPerformanceGraphDataModel(int year, string month, List<QcAveTestTimeKpi> qcAveTestTimeKpis)
        {
            var qcPerformanceGraphData = new QcPerformanceGraphData();

            // assign x axis value
            qcPerformanceGraphData.XAxisValue = month != "" ? $"{TestMonthUtility.ConvertMonth(month)}/{year}" : year.ToString();

            var aveTestTime = qcAveTestTimeKpis.Count() == 0 ? 0 : qcAveTestTimeKpis.Average(x => x.aveTestTime);
            long aveTestTimeTicks = Convert.ToInt64(aveTestTime);
            TimeSpan aveTestTimeSpan = TimeSpan.FromTicks(aveTestTimeTicks);

            // Adding 19 minute of sample preparing time (constant)
            if (aveTestTimeTicks != 0)
            {
                aveTestTimeSpan = aveTestTimeSpan.Add(Constants.Constants.SamplePrepareTimeSpan);
            }

            // Total Test including W/U, STD and BCH

            qcPerformanceGraphData.YAxisValue = aveTestTimeSpan.TotalMinutes;

            return qcPerformanceGraphData;
        }

        /// <summary>
        /// Creates a QcPerformanceGraphData model for a given year and month using lists of QCLabel and QcPerformanceKpi.
        /// Sets the X axis value as "MM/YYYY" if month is provided, otherwise as the year.
        /// Calculates the number of printed QC labels and the total number of passed tests (first, second, and third pass) from the KPI list.
        /// Computes the percentage of printed labels relative to the total passed tests and assigns it to the Y axis value.
        /// </summary>
        private QcPerformanceGraphData CreateQcPerformanceGraphDataModel(int year, string month, List<QCLabel> qCLabels, List<QcPerformanceKpi> qcPerformanceKpis)
        {
            var qcPerformanceGraphData = new QcPerformanceGraphData();

            // assign x axis value
            qcPerformanceGraphData.XAxisValue = month != "" ? $"{TestMonthUtility.ConvertMonth(month)}/{year}" : year.ToString();

            // number of printed QC Label
            var numberOfPrintedLabel = qCLabels.Count();
            var totalPassTest = qcPerformanceKpis.Sum(x => x.firstPass + x.secondPass + x.thirdPass);

            double percentageOfPrintedLabel = totalPassTest != 0 ? (double)numberOfPrintedLabel / totalPassTest * 100 : 0;

            qcPerformanceGraphData.YAxisValue = percentageOfPrintedLabel;

            return qcPerformanceGraphData;
        }

        /// <summary>
        /// Calculates the pass percentage value based on the total number of tests and the number of passes for each attempt.
        /// The calculation depends on the selected attempt:
        /// - "1": Only first pass is considered.
        /// - "2": First and second passes are summed.
        /// - Any other value: First, second, and third passes are summed.
        /// Returns 0 if the total number of tests is zero.
        /// </summary>
        private double GetPassPercentageValue(int totalTest, int firstPass, int secondPass, int thirdPass)
        {
            if (totalTest != 0)
            {
                double fraction = SelectedAttempt == "1" ? firstPass : SelectedAttempt == "2" ? firstPass + secondPass : firstPass + secondPass + thirdPass;
                return fraction / totalTest * 100;
            }
            return 0;
        }

        /// <summary>
        /// Asynchronously loads QC performance KPI data for the specified year and month range.
        /// If the start year is greater than the end year, a warning is shown and the operation is aborted.
        /// If the start and end years are the same, data is loaded for each month in the range.
        /// If the range spans multiple years, data is loaded for each relevant month and year, including partial years at the start and end.
        /// </summary>
        public async Task LoadQcPerformanceKpis()
        {
            if (fromStartYear > toEndYear)
            {
                NotificationUtility.ShowWarning("From Year should be less than To Year");
                return;
            }

            // case 1 : From Year and To Year are same
            if (fromStartYear == toEndYear)
            {
                for (var monthIndex = fromStartMonthIndex; monthIndex <= toEndMonthIndex; monthIndex++)
                {
                    await FetchDataToQcPerformanceKpis(fromStartYear, monthIndex);
                }
            }
            // case 2 : From Year and To Year are different
            else if (fromStartYear < toEndYear)
            {
                for (var year = fromStartYear; year <= toEndYear; year++)
                {
                    // if this is starting year
                    if (year == fromStartYear)
                    {
                        for (var monthIndex = fromStartMonthIndex; monthIndex <= 12; monthIndex++)
                        {
                            await FetchDataToQcPerformanceKpis(year, monthIndex);
                        }
                    }

                    // if this is between year
                    if (year > fromStartYear && year < toEndYear)
                    {
                        await FetchDataToQcPerformanceKpis(year, 0);
                    }

                    // if this is ending year
                    if (year == toEndYear)
                    {
                        for (var monthIndex = 1; monthIndex <= toEndMonthIndex; monthIndex++)
                        {
                            await FetchDataToQcPerformanceKpis(year, monthIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously fetches and loads QC performance KPI, average test time KPI, or printed QC label data
        /// for a given year and month index, depending on the selected performance metric.
        /// Constructs the appropriate filter string based on the selected product, instrument, year, and month.
        /// Calls the relevant data retrieval methods for the selected performance type.
        /// </summary>
        private async Task FetchDataToQcPerformanceKpis(int year, int monthIndex)
        {
            string filterProductName = SelectedProduct == "All" ? "" : SelectedProduct;
            string filterMachineName = SelectedInstrument == "All" ? "" : SelectedInstrument;

            string filterYear = year.ToString();
            string filterMonth = monthIndex == 0 ? "" : TestMonthUtility.ConvertMonth(Constants.Constants.Months[monthIndex]);

            string filter = $"?productName={filterProductName}&machineName={filterMachineName}&year={filterYear}&month={filterMonth}";

            switch (SelectedPerformance)
            {
                case "Average Test Time":
                    await GetAllQcAveTestTimeKpis(filter);
                    break;

                case "QC Label Printed":
                    await GetAllPrintedQcLabels(filter);
                    await GetAllQcPerformanceKpis(filter);
                    break;

                case "Conformance":
                case "QC First Time Pass":
                    await GetAllQcPerformanceKpis(filter);
                    break;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all QC performance KPI records using the provided filter and sort parameters,
        /// and adds them to the QcPerformanceKpis collection if any are returned.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the query.</param>
        /// <param name="sort">Optional sort string to apply to the query.</param>
        public async Task GetAllQcPerformanceKpis(string filter = "", string sort = "")
        {
            var qcPerformanceKpis = await _qcPerformanceKpiRestAPI.GetAllQcPerformanceKpisAsync(filter, sort);

            if (qcPerformanceKpis != null)
            {
                foreach (var qcPerformanceKpi in qcPerformanceKpis)
                {
                    QcPerformanceKpis.Add(qcPerformanceKpi);
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves all QC average test time KPI records using the provided filter and sort parameters,
        /// and adds them to the QcAveTestTimeKpis collection if any are returned.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the query.</param>
        /// <param name="sort">Optional sort string to apply to the query.</param>
        public async Task GetAllQcAveTestTimeKpis(string filter = "", string sort = "")
        {
            var qcAveTestTimeKpis = await _qcAveTimeKpiRestApi.GetAllQcAveTestTimeKpisAsync(filter, sort);

            if (qcAveTestTimeKpis != null)
            {
                foreach (var qcAveTestTimeKpi in qcAveTestTimeKpis)
                {
                    QcAveTestTimeKpis.Add(qcAveTestTimeKpi);
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves all printed QC label records using the provided filter and sort parameters,
        /// and adds them to the QcLabels collection if any are returned.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the query.</param>
        /// <param name="sort">Optional sort string to apply to the query.</param>
        public async Task GetAllPrintedQcLabels(string filter = "", string sort = "")
        {
            var qcLabels = await _qcLabelRestAPI.GetAllQCLabelsAsync(filter, sort);
            if (qcLabels != null)
            {
                foreach (var qcLabel in qcLabels)
                {
                    QcLabels.Add(qcLabel);
                }
            }
        }
    }
}
