using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using SharpVectors.Scripting;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using PropertyChanged;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Utilities;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.IService;

namespace ChemsonLabApp.MVVM.Views.Report
{
    /// <summary>
    /// Interaction logic for MakeReportView.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class MakeReportView : Window
    {
        private int bigFontSize = 20;
        private int mediumFontSize = 14;
        private int smallFontSize = 12;
        private TextBox observationValue;
        private Label batchStatusValue;
        private string DisplayBatchStatus;
        private string displayResult = "Fail";
        private Grid TestReportGrid = new Grid();
        private string fontFamily = "Arial";
        public long aveTestTimeTick { get; set; }

        public MakeReportView(MakeReportViewModel makeReportViewModel)
        {
            try
            {
                InitializeComponent();

                // Create Grid for Test Report
                CreateTestResultsGrid(makeReportViewModel.BatchTestResults);
                makeReportViewModel.TestReportGrid = TestReportGrid;

                DataContext = makeReportViewModel;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to create report. Please try again later.");
                LoggerUtility.LogError(ex);
            }
        }

        public async Task<List<Models.Specification>> GetAllSpecificationsAsync(string filter= "", string sort = "")
        {
            var specificationAPI = new SpecificationRestAPI();
            var specifications = await specificationAPI.GetAllSpecificationsAsync(filter: filter, sort: sort);
            return specifications;
        }


        public async Task<Dictionary<string, string>> InitializeTestConditionValue(BatchTestResult batchTestResult)
        {
            Dictionary<string, string> testCondition = new Dictionary<string, string>();


            string filter = $"?productName={batchTestResult.testResult.product.name}&machineName={batchTestResult.testResult.machine.name}&InUse=true";
            var specifications = await GetAllSpecificationsAsync(filter: filter);

            if (specifications != null)
            {
                var specification = specifications.Last();
                testCondition.Add("Temp", specification.temp.ToString());
                testCondition.Add("Load", specification.load.ToString());
                testCondition.Add("RPM", specification.rpm.ToString());
            }
            else
            {
                // If no specification found, alert user
                throw new Exception("No specification found for this product and machine. Please add specification for this product and machine.");
            }

            return testCondition;
        }



        public async void CreateTestResultsGrid(List<BatchTestResult> batchTestResults) 
        {
            if (batchTestResults.Count > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    TestReportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                var numberOfRow = 7 + batchTestResults.Count;
                for (int i = 0; i < numberOfRow; i++)
                {
                    TestReportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }

                TestReportGrid.Background = new SolidColorBrush(Colors.White);

                // Adding Rheology method
                Label rheologyResultsLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.testMethod} Results",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = bigFontSize,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(rheologyResultsLabel, 0);
                Grid.SetColumn(rheologyResultsLabel, 0);
                Grid.SetColumnSpan(rheologyResultsLabel, 2);
                TestReportGrid.Children.Add(rheologyResultsLabel);

                // Adding Instrument
                Label instrumentLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.machine.name}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = bigFontSize,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(instrumentLabel, 0);
                Grid.SetColumn(instrumentLabel, 4);
                TestReportGrid.Children.Add(instrumentLabel);

                // Adding heading Product
                Label productHeadingLabel = new Label
                {
                    Content = $"Product: ",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(productHeadingLabel, 1);
                Grid.SetColumn(productHeadingLabel, 0);
                TestReportGrid.Children.Add(productHeadingLabel);

                // Adding product name
                Label productNameLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.product.name}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(productNameLabel, 1);
                Grid.SetColumn(productNameLabel, 1);
                Grid.SetColumnSpan(productNameLabel, 2);
                TestReportGrid.Children.Add(productNameLabel);

                // Adding X2 comment
                string xTwo = batchTestResults[0].testResult.product.comment == "x2" ? batchTestResults[0].testResult.product.comment : string.Empty;
                Label commentLabel = new Label
                {
                    Content = $"{xTwo}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(commentLabel, 1);
                Grid.SetColumn(commentLabel, 3);
                TestReportGrid.Children.Add(commentLabel);

                // Adding "Conditions" header
                Label conditionHeaderLabel = new Label
                {
                    Content = $"Conditions",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(conditionHeaderLabel, 1);
                Grid.SetColumn(conditionHeaderLabel, 4);
                TestReportGrid.Children.Add(conditionHeaderLabel);

                // Adding "Date" header
                Label dateHeaderLabel = new Label
                {
                    Content = $"Date: ",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(dateHeaderLabel, 2);
                Grid.SetColumn(dateHeaderLabel, 0);
                TestReportGrid.Children.Add(dateHeaderLabel);

                // Adding Test Report Create Date
                Label reportDateLabel = new Label
                {
                    Content = $"{DateTime.Now.ToString("d")}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(reportDateLabel, 2);
                Grid.SetColumn(reportDateLabel, 1);
                TestReportGrid.Children.Add(reportDateLabel);

                // Adding "Temp (°C)" header
                Label tempHeaderLabel = new Label
                {
                    Content = $"Temp (°C)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(tempHeaderLabel, 2);
                Grid.SetColumn(tempHeaderLabel, 3);
                TestReportGrid.Children.Add(tempHeaderLabel);

                // Adding "Load (g)" header
                Label loadHeaderLabel = new Label
                {
                    Content = $"Load (g)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(loadHeaderLabel, 2);
                Grid.SetColumn(loadHeaderLabel, 4);
                TestReportGrid.Children.Add(loadHeaderLabel);

                // Adding "RPM" header
                Label rpmHeaderLabel = new Label
                {
                    Content = $"RPM",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(rpmHeaderLabel, 2);
                Grid.SetColumn(rpmHeaderLabel, 5);
                TestReportGrid.Children.Add(rpmHeaderLabel);

                // Adding "Operator" header
                Label operatorHeaderLabel = new Label
                {
                    Content = $"Operator: ",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(operatorHeaderLabel, 3);
                Grid.SetColumn(operatorHeaderLabel, 0);
                TestReportGrid.Children.Add(operatorHeaderLabel);

                // Adding Operator name
                TextBox operatorNameLabel = new TextBox
                {
                    Text = $"{batchTestResults.Last().testResult.operatorName}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderThickness = new Thickness(0),
                };
                Grid.SetRow(operatorNameLabel, 3);
                Grid.SetColumn(operatorNameLabel, 1);
                TestReportGrid.Children.Add(operatorNameLabel);

                var testConditions = await InitializeTestConditionValue(batchTestResults[0]);

                Label tempValueLabel = new Label
                {
                    //Content = $"{batchTestResults.Last().testResult.mixerTemp}",
                    Content = testConditions["Temp"],
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(tempValueLabel, 3);
                Grid.SetColumn(tempValueLabel, 3);
                TestReportGrid.Children.Add(tempValueLabel);

                // Adding Load value
                Label loadValueLabel = new Label
                {
                    //Content = $"{batchTestResults.Last().testResult.sampleWeight}",
                    Content = testConditions["Load"],
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(loadValueLabel, 3);
                Grid.SetColumn(loadValueLabel, 4);
                TestReportGrid.Children.Add(loadValueLabel);

                // Adding RPM value
                Label rpmValueLabel = new Label
                {
                    //Content = $"{batchTestResults.Last().testResult.speed}",
                    Content = testConditions["RPM"],
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(rpmValueLabel, 3);
                Grid.SetColumn(rpmValueLabel, 5);
                TestReportGrid.Children.Add(rpmValueLabel);

                // Adding Deviation header
                Label deviationOneLabel = new Label
                {
                    Content = $"Deviation",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border deviationOneBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5, 0.5, 0, 0)
                };
                Grid.SetRow(deviationOneLabel, 4);
                Grid.SetColumn(deviationOneLabel, 1);
                TestReportGrid.Children.Add(deviationOneLabel);
                Grid.SetRow(deviationOneBorder, 4);
                Grid.SetColumn(deviationOneBorder, 1);
                TestReportGrid.Children.Add(deviationOneBorder);

                // Adding "warning +/-" header
                Label warningHeaderLabel = new Label
                {
                    Content = $"warning +/-",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border warningBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0, 0)
                };
                Grid.SetRow(warningHeaderLabel, 4);
                Grid.SetColumn(warningHeaderLabel, 2);
                TestReportGrid.Children.Add(warningHeaderLabel);
                Grid.SetRow(warningBorder, 4);
                Grid.SetColumn(warningBorder, 2);
                TestReportGrid.Children.Add(warningBorder);

                // Adding "Torque warning" value
                Label torqueWarningStdLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.product.torqueWarning}%",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border torqueWarningStdBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0, 0)
                };
                Grid.SetRow(torqueWarningStdLabel, 4);
                Grid.SetColumn(torqueWarningStdLabel, 3);
                TestReportGrid.Children.Add(torqueWarningStdLabel);
                Grid.SetRow(torqueWarningStdBorder, 4);
                Grid.SetColumn(torqueWarningStdBorder, 3);
                TestReportGrid.Children.Add(torqueWarningStdBorder);

                // Adding "Fusion warning" value
                Label fusionWarningStdLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.product.fusionWarning}%",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border fusionWarningStdBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0)
                };
                Grid.SetRow(fusionWarningStdLabel, 4);
                Grid.SetColumn(fusionWarningStdLabel, 4);
                TestReportGrid.Children.Add(fusionWarningStdLabel);
                Grid.SetRow(fusionWarningStdBorder, 4);
                Grid.SetColumn(fusionWarningStdBorder, 4);
                TestReportGrid.Children.Add(fusionWarningStdBorder);

                // Adding "Runtime" header
                Label runtimeHeaderLabel = new Label
                {
                    Content = $"Run Time",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(runtimeHeaderLabel, 4);
                Grid.SetColumn(runtimeHeaderLabel, 5);
                TestReportGrid.Children.Add(runtimeHeaderLabel);

                // Adding "Av. Min./trace" header
                Label avMinHeaderLabel = new Label
                {
                    Content = $"Av. Min./trace",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(avMinHeaderLabel, 4);
                Grid.SetColumn(avMinHeaderLabel, 6);
                TestReportGrid.Children.Add(avMinHeaderLabel);

                // Adding "Deviation 2" Header
                Label deviationTwoHeaderLabel = new Label
                {
                    Content = $"Deviation",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border deviationTwoHeaderBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5, 0, 0, 0)
                };
                Grid.SetRow(deviationTwoHeaderLabel, 5);
                Grid.SetColumn(deviationTwoHeaderLabel, 1);
                TestReportGrid.Children.Add(deviationTwoHeaderLabel);
                Grid.SetRow(deviationTwoHeaderBorder, 5);
                Grid.SetColumn(deviationTwoHeaderBorder, 1);
                TestReportGrid.Children.Add(deviationTwoHeaderBorder);

                // Adding "fail +/-" header
                Label failHeaderLabel = new Label
                {
                    Content = $"fail +/-",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(failHeaderLabel, 5);
                Grid.SetColumn(failHeaderLabel, 2);
                TestReportGrid.Children.Add(failHeaderLabel);

                // Adding "Torque Fail" value
                Label TorqueFailStdValueLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.product.torqueFail}%",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(TorqueFailStdValueLabel, 5);
                Grid.SetColumn(TorqueFailStdValueLabel, 3);
                TestReportGrid.Children.Add(TorqueFailStdValueLabel);

                // Adding "Fusion Fail" value
                Label fusionFailStdValueLabel = new Label
                {
                    Content = $"{batchTestResults[0].testResult.product.fusionFail}%",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border fusionFailStdValueBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0.5, 0)
                };
                Grid.SetRow(fusionFailStdValueLabel, 5);
                Grid.SetColumn(fusionFailStdValueLabel, 4);
                TestReportGrid.Children.Add(fusionFailStdValueLabel);
                Grid.SetRow(fusionFailStdValueBorder, 5);
                Grid.SetColumn(fusionFailStdValueBorder, 4);
                TestReportGrid.Children.Add(fusionFailStdValueBorder);


                // Adding RunTime value
                TimeSpan totalRunTime = TimeSpan.Zero;
                string format = "dd/MM/yyyy HH:mm";

                var sortedBatchTestResults = batchTestResults.OrderBy(result => DateTime.ParseExact(result.testResult.testDate, format, CultureInfo.InvariantCulture)).ToList();

                DateTime testDate = DateTime.ParseExact(sortedBatchTestResults.Last().testResult.testDate, format, CultureInfo.InvariantCulture);

                List<BatchTestResult> timeCalculationTests = new List<BatchTestResult>();
                foreach (var result in sortedBatchTestResults)
                {
                    DateTime checkTestDate = DateTime.ParseExact(result.testResult.testDate, format, CultureInfo.InvariantCulture);
                    if (checkTestDate.Date == testDate.Date)
                    {
                        timeCalculationTests.Add(result);
                    }
                }

                int testCount = 0;

                for (int i = timeCalculationTests.Count - 1; i >= 0; i--)
                {
                    if (i > 0)
                    {
                        string endDateTimeStr = timeCalculationTests[i].testResult.testDate;
                        string startDateTimeStr = timeCalculationTests[i - 1].testResult.testDate;
                        DateTime endTestTime = DateTime.ParseExact(endDateTimeStr, format, CultureInfo.InvariantCulture);
                        DateTime startTestTime = DateTime.ParseExact(startDateTimeStr, format, CultureInfo.InvariantCulture);
                        TimeSpan timeSpan = endTestTime - startTestTime;
                        if (timeSpan < TimeSpan.FromMinutes(20))
                        {
                            totalRunTime += timeSpan;
                            testCount++;
                        }
                    }
                    else
                    {
                        string endDateTimeStr = timeCalculationTests[i + 1].testResult.testDate;
                        string startDateTimeStr = timeCalculationTests[i].testResult.testDate;
                        DateTime endTestTime = DateTime.ParseExact(endDateTimeStr, format, CultureInfo.InvariantCulture);
                        DateTime startTestTime = DateTime.ParseExact(startDateTimeStr, format, CultureInfo.InvariantCulture);
                        TimeSpan timeSpan = endTestTime - startTestTime;
                        if (timeSpan < TimeSpan.FromMinutes(20))
                        {
                            totalRunTime += timeSpan;
                            testCount++;
                        }
                    }
                }

                Label runTimeValueLabel = new Label
                {
                    Content = $"{(int)totalRunTime.TotalHours:D2}:{totalRunTime.Minutes:D2}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(runTimeValueLabel, 5);
                Grid.SetColumn(runTimeValueLabel, 5);
                TestReportGrid.Children.Add(runTimeValueLabel);


                // Adding averageTrace Value
                long aveRunTimeTicks = totalRunTime.Ticks / timeCalculationTests.Count();
                //long aveRunTimeTicks = totalRunTime.Ticks / testCount;
                TimeSpan averageTrace = TimeSpan.FromTicks(aveRunTimeTicks);

                aveTestTimeTick = aveRunTimeTicks;

                Label averageTraceValueLabel = new Label
                {
                    Content = $"{(int)averageTrace.TotalMinutes:D2}:{averageTrace.Seconds:D2}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(averageTraceValueLabel, 5);
                Grid.SetColumn(averageTraceValueLabel, 6);
                TestReportGrid.Children.Add(averageTraceValueLabel);

                // Adding "Batch" Heading
                Label batchHeaderLabel = new Label
                {
                    Content = $"Batch",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border batchHeaderBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(batchHeaderLabel, 6);
                Grid.SetColumn(batchHeaderLabel, 0);
                TestReportGrid.Children.Add(batchHeaderLabel);
                Grid.SetRow(batchHeaderBorder, 6);
                Grid.SetColumn(batchHeaderBorder, 0);
                TestReportGrid.Children.Add(batchHeaderBorder);

                // Adding "TorqueNm" Heading
                Label torqueNmLabel = new Label
                {
                    Content = $"Torque (Nm)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border torqueNmBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(torqueNmLabel, 6);
                Grid.SetColumn(torqueNmLabel, 1);
                TestReportGrid.Children.Add(torqueNmLabel);
                Grid.SetRow(torqueNmBorder, 6);
                Grid.SetColumn(torqueNmBorder, 1);
                TestReportGrid.Children.Add(torqueNmBorder);

                // Adding "FusionS" Heading
                Label fusionSLabel = new Label
                {
                    Content = $"Fusion (s)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border fusionSBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(fusionSLabel, 6);
                Grid.SetColumn(fusionSLabel, 2);
                TestReportGrid.Children.Add(fusionSLabel);
                Grid.SetRow(fusionSBorder, 6);
                Grid.SetColumn(fusionSBorder, 2);
                TestReportGrid.Children.Add(fusionSBorder);

                // Adding "TorquePer" Heading
                Label torquePerLabel = new Label
                {
                    Content = $"Torque (%)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border torquePerBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(torquePerLabel, 6);
                Grid.SetColumn(torquePerLabel, 3);
                TestReportGrid.Children.Add(torquePerLabel);
                Grid.SetRow(torquePerBorder, 6);
                Grid.SetColumn(torquePerBorder, 3);
                TestReportGrid.Children.Add(torquePerBorder);

                // Adding "FusionPer" Heading
                Label fusionPerLabel = new Label
                {
                    Content = $"Fusion (%)",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border fusionPerBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(fusionPerLabel, 6);
                Grid.SetColumn(fusionPerLabel, 4);
                TestReportGrid.Children.Add(fusionPerLabel);
                Grid.SetRow(fusionPerBorder, 6);
                Grid.SetColumn(fusionPerBorder, 4);
                TestReportGrid.Children.Add(fusionPerBorder);

                // Adding "Result" Heading
                Label resultLabel = new Label
                {
                    Content = $"Result",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border resultBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(resultLabel, 6);
                Grid.SetColumn(resultLabel, 5);
                TestReportGrid.Children.Add(resultLabel);
                Grid.SetRow(resultBorder, 6);
                Grid.SetColumn(resultBorder, 5);
                TestReportGrid.Children.Add(resultBorder);

                // Adding "Observations" Heading
                Label observationsLabel = new Label
                {
                    Content = $"Observations",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border observationsBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.25)
                };
                Grid.SetRow(observationsLabel, 6);
                Grid.SetColumn(observationsLabel, 6);
                TestReportGrid.Children.Add(observationsLabel);
                Grid.SetRow(observationsBorder, 6);
                Grid.SetColumn(observationsBorder, 6);
                TestReportGrid.Children.Add(observationsBorder);

                // Adding "Batch_Status" Heading
                Label batchStatusLabel = new Label
                {
                    Content = $"Batch_Status",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border batchStatusBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.5)
                };
                Grid.SetRow(batchStatusLabel, 6);
                Grid.SetColumn(batchStatusLabel, 7);
                TestReportGrid.Children.Add(batchStatusLabel);
                Grid.SetRow(batchStatusBorder, 6);
                Grid.SetColumn(batchStatusBorder, 7);
                TestReportGrid.Children.Add(batchStatusBorder);

                // Adding "Colour" Heading
                Label colourHeaderLabel = new Label
                {
                    Content = $"Colour",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border colourHeaderBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.5)
                };
                Grid.SetRow(colourHeaderLabel, 6);
                Grid.SetColumn(colourHeaderLabel, 8);
                TestReportGrid.Children.Add(colourHeaderLabel);
                Grid.SetRow(colourHeaderBorder, 6);
                Grid.SetColumn(colourHeaderBorder, 8);
                TestReportGrid.Children.Add(colourHeaderBorder);

                // Adding "Sample_by" Heading
                Label sampleByHeaderLabel = new Label
                {
                    Content = $"Sample_by",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border sampleByHeaderBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0.5, 0.5, 0.5)
                };
                Grid.SetRow(sampleByHeaderLabel, 6);
                Grid.SetColumn(sampleByHeaderLabel, 9);
                TestReportGrid.Children.Add(sampleByHeaderLabel);
                Grid.SetRow(sampleByHeaderBorder, 6);
                Grid.SetColumn(sampleByHeaderBorder, 9);
                TestReportGrid.Children.Add(sampleByHeaderBorder);

                // Adding "Concessions" Heading
                Label concessionsHeaderLabel = new Label
                {
                    Content = $"Concessions",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = mediumFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Border concessionsHeaderBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.25, 0.25, 0.5, 0.5)
                };
                Grid.SetRow(concessionsHeaderLabel, 7);
                Grid.SetColumn(concessionsHeaderLabel, 6);
                TestReportGrid.Children.Add(concessionsHeaderLabel);
                Grid.SetRow(concessionsHeaderBorder, 7);
                Grid.SetColumn(concessionsHeaderBorder, 6);
                TestReportGrid.Children.Add(concessionsHeaderBorder);

                // Adding "ColourStd" Heading
                Label colourStdHeaderLabel = new Label
                {
                    Content = $"{batchTestResults.First().testResult.product.colour}",
                    FontFamily = new FontFamily(fontFamily),
                    FontSize = smallFontSize,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(colourStdHeaderLabel, 7);
                Grid.SetColumn(colourStdHeaderLabel, 8);
                TestReportGrid.Children.Add(colourStdHeaderLabel);

                //////////// Populate test result data to report /////////////////////


                int startRowOfTestResult = 7;
                var batchResults = batchTestResults.Where(bt => bt.testResult.testType == "BCH").ToList();
                string stdRef = batchResults.Last().standardReference;

                foreach (var result in batchTestResults)
                {
                    var resultBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB6962"));
                    var torqueBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB6962"));
                    var fusionBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB6962"));
                    bool isTorquePass = Math.Abs(result.torqueDiff) <= result.testResult.product.torqueWarning;
                    bool isFusionPass = Math.Abs(result.fusionDiff) <= result.testResult.product.fusionWarning;
                    bool isTorqueFail = Math.Abs(result.torqueDiff) > result.testResult.product.torqueFail;
                    bool isFusionFail = Math.Abs(result.fusionDiff) > result.testResult.product.fusionFail;

                    if (isTorquePass)
                    {
                        torqueBGColour = Brushes.Transparent;
                    }
                    else if (!isTorquePass && !isTorqueFail)
                    {
                        torqueBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFC99"));
                    }

                    if (isFusionPass)
                    {
                        fusionBGColour = Brushes.Transparent;
                    }
                    else if (!isFusionPass && !isFusionFail)
                    {
                        fusionBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFC99"));
                    }

                    if (result.result)
                    {
                        if (isTorquePass && isFusionPass)
                        {
                            result.reportDisplayResult = "Pass";
                            displayResult = "Pass";
                            resultBGColour = Brushes.Transparent;
                        }
                        else
                        {
                            result.reportDisplayResult = "Warning";
                            displayResult = "Warning";
                            resultBGColour = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFC99"));
                        }
                    }

                    int startColumnOfResult = 0;

                    //List<string> trailingSuffixList = new List<string> { "RS", "RRS", "RT" }; 

                    string batchNameContent =  $"{result.batch.batchName} {result.batch.suffix}";

                    TextBox batchName = new TextBox
                    {
                        Text = batchNameContent,
                        FontFamily = new FontFamily(fontFamily),
                        FontSize = smallFontSize,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        BorderThickness = new Thickness(0),
                    };
                    Border batchNameBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5, 0.25, 0.25, 0.25)
                    };
                    Grid.SetRow(batchName, startRowOfTestResult);
                    Grid.SetColumn(batchName, startColumnOfResult);
                    TestReportGrid.Children.Add(batchName);
                    Grid.SetRow(batchNameBorder, startRowOfTestResult);
                    Grid.SetColumn(batchNameBorder, startColumnOfResult++);
                    TestReportGrid.Children.Add(batchNameBorder);

                    Label torqueValue = new Label
                    {
                        Content = $"{result.testResult.torque}",
                        FontFamily = new FontFamily(fontFamily),
                        FontSize = smallFontSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Border torqueValueBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                    };
                    Grid.SetRow(torqueValue, startRowOfTestResult);
                    Grid.SetColumn(torqueValue, startColumnOfResult);
                    TestReportGrid.Children.Add(torqueValue);
                    Grid.SetRow(torqueValueBorder, startRowOfTestResult);
                    Grid.SetColumn(torqueValueBorder, startColumnOfResult++);
                    TestReportGrid.Children.Add(torqueValueBorder);

                    Label fusionValue = new Label
                    {
                        Content = $"{result.testResult.fusion}",
                        FontFamily = new FontFamily(fontFamily),
                        FontSize = smallFontSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    Border fusionValueBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                    };
                    Grid.SetRow(fusionValue, startRowOfTestResult);
                    Grid.SetColumn(fusionValue, startColumnOfResult);
                    TestReportGrid.Children.Add(fusionValue);
                    Grid.SetRow(fusionValueBorder, startRowOfTestResult);
                    Grid.SetColumn(fusionValueBorder, startColumnOfResult++);
                    TestReportGrid.Children.Add(fusionValueBorder);

                    if (result.batch.batchName == stdRef)
                    {
                        Label torqueRefIndi = new Label
                        {
                            Content = $"1",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        Border torqueRefIndiBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(torqueRefIndi, startRowOfTestResult);
                        Grid.SetColumn(torqueRefIndi, startColumnOfResult);
                        TestReportGrid.Children.Add(torqueRefIndi);
                        Grid.SetRow(torqueRefIndiBorder, startRowOfTestResult);
                        Grid.SetColumn(torqueRefIndiBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(torqueRefIndiBorder);

                        Label fusionRefIndi = new Label
                        {
                            Content = $"1",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        Border fusionRefIndiBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(fusionRefIndi, startRowOfTestResult);
                        Grid.SetColumn(fusionRefIndi, startColumnOfResult);
                        TestReportGrid.Children.Add(fusionRefIndi);
                        Grid.SetRow(fusionRefIndiBorder, startRowOfTestResult);
                        Grid.SetColumn(fusionRefIndiBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(fusionRefIndiBorder);

                        Border resultValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(resultValueBorder, startRowOfTestResult);
                        Grid.SetColumn(resultValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(resultValueBorder);
                    }
                    else if (result.batch.batchName != stdRef && result.testResult.testType != "BCH")
                    {
                        Border torqueRefIndiBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(torqueRefIndiBorder, startRowOfTestResult);
                        Grid.SetColumn(torqueRefIndiBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(torqueRefIndiBorder);

                        Border fusionRefIndiBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(fusionRefIndiBorder, startRowOfTestResult);
                        Grid.SetColumn(fusionRefIndiBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(fusionRefIndiBorder);

                        Border resultValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(resultValueBorder, startRowOfTestResult);
                        Grid.SetColumn(resultValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(resultValueBorder);
                    }


                    if (result.testResult.testType == "BCH")
                    {
                        TextBox torquePerValue = new TextBox
                        {
                            Text = $"{result.torqueDiff}%",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            Background = torqueBGColour,
                            Padding = new Thickness(0, 0, 5, 0),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            BorderThickness = new Thickness(0.25),
                        };
                        Border torquePerValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(torquePerValue, startRowOfTestResult);
                        Grid.SetColumn(torquePerValue, startColumnOfResult);
                        TestReportGrid.Children.Add(torquePerValue);
                        Grid.SetRow(torquePerValueBorder, startRowOfTestResult);
                        Grid.SetColumn(torquePerValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(torquePerValueBorder);

                        TextBox fusionPerValue = new TextBox
                        {
                            Text = $"{result.fusionDiff}%",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            Background = fusionBGColour,
                            Padding = new Thickness(0, 0, 5, 0),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            BorderThickness = new Thickness(0.25),
                        };
                        Border fusionPerValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(fusionPerValue, startRowOfTestResult);
                        Grid.SetColumn(fusionPerValue, startColumnOfResult);
                        TestReportGrid.Children.Add(fusionPerValue);
                        Grid.SetRow(fusionPerValueBorder, startRowOfTestResult);
                        Grid.SetColumn(fusionPerValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(fusionPerValueBorder);

                        //displayResult = result.result ? "Pass" : "Fail";
                        Label resultValue = new Label
                        {
                            Content = $"{result.reportDisplayResult}",
                            FontFamily = new FontFamily(fontFamily),
                            Background = resultBGColour,
                            FontSize = smallFontSize,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        };
                        Border resultValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(resultValue, startRowOfTestResult);
                        Grid.SetColumn(resultValue, startColumnOfResult);
                        TestReportGrid.Children.Add(resultValue);
                        Grid.SetRow(resultValueBorder, startRowOfTestResult);
                        Grid.SetColumn(resultValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(resultValueBorder);

                        //string observationText = trailingSuffixList.Contains(result.batch.suffix) ? "" : result.batch.suffix;

                        observationValue = new TextBox
                        {
                            Text = $"",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            BorderThickness = new Thickness(0),
                        };
                        Border observationValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(observationValue, startRowOfTestResult);
                        Grid.SetColumn(observationValue, startColumnOfResult);
                        TestReportGrid.Children.Add(observationValue);
                        Grid.SetRow(observationValueBorder, startRowOfTestResult);
                        Grid.SetColumn(observationValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(observationValueBorder);

                        observationValue.TextChanged += ObservationValue_TextChanged;

                        if (result.reportDisplayResult == "Pass" || result.reportDisplayResult == "Warning")
                        {
                            DisplayBatchStatus = "OK";
                        }
                        else if (result.reportDisplayResult == "Fail" && result.testResult.testNumber < 3)
                        {
                            DisplayBatchStatus = "Review";
                        }
                        else if (result.reportDisplayResult == "Fail" && result.testResult.testNumber >= 3)
                        {
                            DisplayBatchStatus = "Fail";
                        }


                        batchStatusValue = new Label
                        {
                            Content = $"{DisplayBatchStatus}",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        };
                        switch (DisplayBatchStatus)
                        {
                            case "OK":
                            case "Concessional Passed":
                                batchStatusValue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#79DE79"));
                                break;
                            case "Fail":
                                batchStatusValue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB6962"));
                                break;
                            case "Review":
                                batchStatusValue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFC99"));
                                break;
                            default:
                                batchStatusValue.Background = Brushes.Transparent;
                                break;
                        }
                        Border batchStatusValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(batchStatusValue, startRowOfTestResult);
                        Grid.SetColumn(batchStatusValue, startColumnOfResult);
                        TestReportGrid.Children.Add(batchStatusValue);
                        Grid.SetRow(batchStatusValueBorder, startRowOfTestResult);
                        Grid.SetColumn(batchStatusValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(batchStatusValueBorder);

                        TextBox colourValue = new TextBox
                        {
                            Text = $"{result.testResult.colour}",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            BorderThickness = new Thickness(0),
                        };
                        Border colourValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(colourValue, startRowOfTestResult);
                        Grid.SetColumn(colourValue, startColumnOfResult);
                        TestReportGrid.Children.Add(colourValue);
                        Grid.SetRow(colourValueBorder, startRowOfTestResult);
                        Grid.SetColumn(colourValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(colourValueBorder);

                        Label sampleByValue = new Label
                        {
                            Content = $"{result.batch.sampleBy}",
                            FontFamily = new FontFamily(fontFamily),
                            FontSize = smallFontSize,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        Border sampleByValueBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25, 0.25, 0.25, 0.25)
                        };
                        Grid.SetRow(sampleByValue, startRowOfTestResult);
                        Grid.SetColumn(sampleByValue, startColumnOfResult);
                        TestReportGrid.Children.Add(sampleByValue);
                        Grid.SetRow(sampleByValueBorder, startRowOfTestResult);
                        Grid.SetColumn(sampleByValueBorder, startColumnOfResult++);
                        TestReportGrid.Children.Add(sampleByValueBorder);
                    }

                    startRowOfTestResult++;
                }
            }

            var flowDoc = new FlowDocument();
            flowDoc.Blocks.Add(new BlockUIContainer(TestReportGrid));
            TestReportDoc.Document = flowDoc;

        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ObservationValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                if (textBox.Text == "MD Passed")
                {
                    batchStatusValue.Content = "Concessional Passed";
                    batchStatusValue.FontSize = 9;
                    batchStatusValue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#79DE79"));
                }
            }
        }
    }
}
