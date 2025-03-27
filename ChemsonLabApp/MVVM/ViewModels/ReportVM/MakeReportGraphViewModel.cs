using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using LiveCharts;
using LiveCharts.Wpf;
using PropertyChanged;
using Windows.Media.Audio;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM
{
    [AddINotifyPropertyChangedInterface]
    public class MakeReportGraphViewModel
    {
        // User input parameters
        public TestResultReport TestResultReport { get; set; }
        public string ProductName { get; set; }
        public string BatchName { get; set; }

        // Live Chart parameters
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public List<string> XAxisLabels { get; set; } = new List<string>();
        public ChartValues<double> BatchTorqueData { get; set; } = new ChartValues<double>();
        public ChartValues<double> StdTorqueData { get; set; } = new ChartValues<double>();
        public Func<double, string> Formatter { get; set; }
        public string YAxisTitle { get; set; }
        public double YAxisMinValue { get; set; } = 0;
        public double YAxisMaxValue { get; set; } = 100;
        public string ChartTitle { get; set; }
        public string ChartFooter { get; set; }

        // local variables
        private List<Measurement> measurements = new List<Measurement>();
        private List<Evaluation> evaluations = new List<Evaluation>();
        private List<string> batchEvaluationTimePoints = new List<string>();

        private List<Evaluation> stdEvaluations = new List<Evaluation>();
        private List<Measurement> stdMeasurements = new List<Measurement>();
        private BatchTestResult stdBatchTestResult;
        private List<string> stdEvaluationTimePoints = new List<string>();

        private Specification productTestCondition;

        // Services
        private readonly IMeasurementRestAPI _measurementRestAPI;
        private readonly IEvaluationRestAPI _evaluationRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;
        private readonly ISpecificationRestAPI _specificationRestAPI;

        public MakeReportGraphViewModel(
            IMeasurementRestAPI measurementRestAPI,
            IEvaluationRestAPI evaluationRestAPI,
            IBatchTestResultRestAPI batchTestResultRestAPI,
            ISpecificationRestAPI specificationRestAPI
            )
        {
            // Services
            this._measurementRestAPI = measurementRestAPI;
            this._evaluationRestAPI = evaluationRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
            this._specificationRestAPI = specificationRestAPI;
        }

        public async Task GenerateTorqueGraph()
        {
            // Load measurement and evaluation data from database
            await LoadDataFromDatabase();
            PopulateDataToGraphData();
            AssignDataToSeriesCollection();
            AssignChartTitle();
        }

        private void AssignChartTitle()
        {
            ChartTitle = $"{ProductName} - {TestResultReport.standardReference} vs {BatchName}";

            string testDate = TestResultReport.batchTestResult.testResult.testDate.Split(' ')[0];
            string instrumentName = TestResultReport.batchTestResult.testResult.machine.name;
            var testTemp = productTestCondition?.temp ?? 0;
            var testRPM = productTestCondition?.rpm ?? 0;
            var testWeight = TestResultReport.batchTestResult.testResult.product.sampleAmount;
            var secondIntervals = instrumentName == "B3" ? 2 : 1;

            ChartFooter = $"{testDate} | {instrumentName} Condition: {testTemp}°C {testWeight}g {testRPM}rpm | {secondIntervals} second intervals";
        }

        private void AssignDataToSeriesCollection()
        {
            SeriesCollection.Add(new LineSeries
            {
                Title = $"{BatchName}",
                Values = BatchTorqueData,
                Fill = Brushes.Transparent,
                DataLabels = true,
                PointGeometrySize = 6,
                LabelPoint = GetBatchLabelPoint,
                ToolTip = new DefaultTooltip(),
                Foreground = Brushes.DarkBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2
            });

            SeriesCollection.Add(new LineSeries
            {
                Title = $"{stdBatchTestResult.batch.batchName}",
                Values = StdTorqueData,
                Fill = Brushes.Transparent,
                DataLabels = true,
                PointGeometrySize = 6,
                LabelPoint = GetStdLabelPoint,
                ToolTip = new DefaultTooltip(),
                Foreground = Brushes.Red,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            });

            YAxisTitle = "Torque (N.m)";
            YAxisMinValue = 0;
            YAxisMaxValue = BatchTorqueData.Max() > StdTorqueData.Max() ? BatchTorqueData.Max() + 10 : StdTorqueData.Max() + 10;
            Formatter = value => value.ToString("N2");
        }

        private string GetStdLabelPoint(ChartPoint point)
        {
            var xValue = XAxisLabels[(int)point.X];

            if (stdEvaluationTimePoints.Contains(xValue))
            {
                return point.Y.ToString("N2");
            }

            return string.Empty;
        }

        private string GetBatchLabelPoint(ChartPoint point)
        {
            // Show label only for specific seconds
            var xValue = XAxisLabels[(int)point.X];

            if (batchEvaluationTimePoints.Contains(xValue))
            {
                return point.Y.ToString("N2");
            }
            return string.Empty;
        }

        private void PopulateDataToGraphData()
        {
            if (measurements != null)
            {
                foreach (var measurement in measurements)
                {
                    XAxisLabels.Add(measurement.timeAct);
                    BatchTorqueData.Add(measurement.torque ?? 0);
                }
            }

            if (evaluations != null)
            {
                foreach (var evaluation in evaluations)
                {
                    if (evaluation.pointName != "t" && evaluation.pointName != "G" && evaluation.pointName != "B")
                    {
                        batchEvaluationTimePoints.Add(evaluation.timeEval);
                    }
                }
            }

            if (stdMeasurements != null)
            {
                foreach (var measurement in stdMeasurements)
                {
                    StdTorqueData.Add(measurement.torque ?? 0);
                }
            }

            if (stdEvaluations != null)
            {
                foreach (var evaluation in stdEvaluations)
                {
                    if (evaluation.pointName != "t" && evaluation.pointName != "G" && evaluation.pointName != "B")
                    {
                        stdEvaluationTimePoints.Add(evaluation.timeEval);
                    }
                }
            }
        }

        private async Task LoadDataFromDatabase()
        {
            // Load Batch Data
            string batchDataFilter = $"?testResultId={TestResultReport.batchTestResult.testResult.id}";
            measurements = await _measurementRestAPI.GetAllMeasurementsAsync(batchDataFilter);
            evaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(batchDataFilter);

            // Load Standard Data
            string batchGroupName = TestResultReport.batchTestResult.testResult.batchGroup;
            string stdBatchName = TestResultReport.standardReference;
            string stdBatchTestResultFilter = $"?productName={ProductName}&batchGroup={batchGroupName}&batchName={stdBatchName}";
            stdBatchTestResult = (await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(stdBatchTestResultFilter)).LastOrDefault();

            string stdDataFilter = $"?testResultId={stdBatchTestResult.testResult.id}";
            stdMeasurements = await _measurementRestAPI.GetAllMeasurementsAsync(stdDataFilter);
            stdEvaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(stdDataFilter);

            // Load Specification Data
            string testedInstrument = $"{TestResultReport.batchTestResult.testResult.machine.name}";
            string specificationFilter = $"?productName={ProductName}&machineName={testedInstrument}";
            productTestCondition = (await _specificationRestAPI.GetAllSpecificationsAsync(specificationFilter)).LastOrDefault();
        }
    }
}
