using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemsonLabApp.MVVM.Models;
using System.Data.OleDb;
using PropertyChanged;
using Microsoft.Win32;
using System.Windows;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command;
using ChemsonLabApp.RestAPI;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Documents;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Net.Http;
using ChemsonLabApp.Utilities;
using Windows.UI.Composition.Interactions;
using System.ComponentModel;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Remoting.Lifetime;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Services.DataLoaderService;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM
{
    [AddINotifyPropertyChangedInterface]
    public class NewDataLoaderViewModel
    {
        // services
        private readonly IProductService _productService;
        private readonly IInstrumentService _instrumentService;
        private readonly IDataLoaderService _dataLoaderService;

        // Public properties
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Instrument> Instruments { get; set; } = new List<Instrument>();
        public List<string> InstrumentsNameList { get; set; } = new List<string>();
        public bool isTwoX { get; set; }
        public ObservableCollection<TestResult> TestResults { get; set; } = new ObservableCollection<TestResult>();
        public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
        public List<string> SuffixComboBox { get; set; } = new List<string> { "", "RS", "RRS", "RT", "Remix", "Rework", "2.00min", "4.00min", "Cal" };

        // Command properties
        public ImportMTFCommand ImportMTFCommand { get; set; }
        public DataLoaderSaveCommand DataLoaderSaveCommand { get; set; }
        public RemoveLoaderCommand RemoveLoaderCommand { get; set; }
        public ReBatchNumberCommand ReBatchNumberCommand { get; set; }
        public SuffixChangedCommand SuffixChangedCommand { get; set; }
        public TestTypeChangedCommand TestTypeChangedCommand { get; set; }

        public NewDataLoaderViewModel(
            IProductService productService,
            IInstrumentService instrumentService,
            IDataLoaderService dataLoaderService
            )
        {
            // services
            this._productService = productService;
            this._instrumentService = instrumentService;
            this._dataLoaderService = dataLoaderService;

            // commands
            ImportMTFCommand = new ImportMTFCommand(this);
            DataLoaderSaveCommand = new DataLoaderSaveCommand(this);
            RemoveLoaderCommand = new RemoveLoaderCommand(this);
            ReBatchNumberCommand = new ReBatchNumberCommand(this);
            SuffixChangedCommand = new SuffixChangedCommand(this);
            TestTypeChangedCommand = new TestTypeChangedCommand(this);

            // initialize
            InitializeParameters();
        }

        /// <summary>
        /// Initializes the parameters for the NewDataLoaderViewModel by loading active products and instruments.
        /// Updates the Products, Instruments, and InstrumentsNameList properties.
        /// Handles exceptions related to server connectivity and data loading, and manages the cursor display state.
        /// </summary>
        private async void InitializeParameters()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                Products = await _productService.LoadActiveProducts();
                Instruments = await _instrumentService.GetAllActiveInstrument();
                foreach (var instrument in Instruments)
                {
                    InstrumentsNameList.Add(instrument.name);
                }
            }
            catch (HttpRequestException ex)
            {
                LoggerUtility.LogError(ex);
                NotificationUtility.ShowError("Error: Fail to connect to server. Please check the internet connection and try again later.");
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError(ex);
                NotificationUtility.ShowError("Error: Fail to load data. Try again later.");

            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Opens a file dialog for the user to select one or more MTF or TXT files, processes each file according to its extension,
        /// and loads the resulting test data into the TestResults collection. After processing, assigns default values and arranges
        /// the results, then updates the TestResults collection and sets the X2 value. Handles errors and manages the cursor display state.
        /// </summary>
        public async void ImportFiles()
        {
            string defaultPath = "S:\\Lab\\B3_Data";

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MTF files (*.mtf)|*.mtf|Text files (*.txt)|*.txt",
                Multiselect = true,
                InitialDirectory = Directory.Exists(defaultPath) ? defaultPath : null
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CursorUtility.DisplayCursor(true);
                try
                {
                    var sortedFiles = openFileDialog.FileNames.OrderBy(Path.GetFileName).ToArray();
                    TestResults.Clear();

                    foreach (var file in sortedFiles)
                    {
                        string extension = Path.GetExtension(file).ToLower();

                        switch (extension)
                        {
                            case ".mtf":
                                ProcessB3MTFFile(file);
                                break;
                            case ".txt":
                                ProcessTxtFile(file);
                                break;
                            default:
                                NotificationUtility.ShowWarning($"{extension} extension is not compatible");
                                break;
                        }
                    }

                    var testResults = await _dataLoaderService.AssignDefaultValuesAndArrangeResults(TestResults.ToList());
                    TestResults.Clear();
                    PopulateTestResultsObservation(testResults);
                    SetX2Value();
                }
                catch (Exception ex)
                {
                    NotificationUtility.ShowError("Error: Fail to import data loader. Please try again later.");
                    LoggerUtility.LogError(ex);
                }
                finally
                {
                    CursorUtility.DisplayCursor(false);
                }
                ;
            }
        }

        /// <summary>
        /// Processes a B3 MTF file by opening a database connection to the file, extracting test results, evaluations, 
        /// and measurements using the data loader service, and adding them to the corresponding collections.
        /// </summary>
        private void ProcessB3MTFFile(string file)
        {
            string fileName = Path.GetFileName(file);
            string connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={file}";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                // Work with Test Results
                var testResult = _dataLoaderService.ProcessMTFTestResults(connection, fileName, Products, Instruments);
                TestResults.Add(testResult);
                //PopulateTestResultsObservation(testResults);

                // Work with Evaluations
                var evaluations = _dataLoaderService.ProcessMTFEvaluations(connection, fileName);
                Evaluations.AddRange(evaluations);
                // Work with Measurements
                var measurements = _dataLoaderService.ProcessMTFMeasurements(connection, fileName);
                Measurements.AddRange(measurements);
            }
        }

        /// <summary>
        /// Processes a TXT file by extracting the test result, measurements, and evaluations using the data loader service.
        /// Adds the extracted test result to the TestResults collection, measurements to the Measurements collection,
        /// and evaluations to the Evaluations collection.
        /// </summary>
        private void ProcessTxtFile(string file)
        {
            var testResult = _dataLoaderService.ProcessTxtTestResult(file, Products, Instruments);
            TestResults.Add(testResult);

            var (measurements, times, torques) = _dataLoaderService.ProcessTXTMeasurement(file);
            Measurements.AddRange(measurements);

            var evaluations = _dataLoaderService.ProcessTXTEvaluation(file, times, torques);
            Evaluations.AddRange(evaluations);
        }

        /// <summary>
        /// Populates the TestResults ObservableCollection with the provided list of test results.
        /// </summary>
        private void PopulateTestResultsObservation(List<TestResult> testResults)
        {
            foreach (var testResult in testResults)
            {
                TestResults.Add(testResult);
            }
        }

        /// <summary>
        /// Sets the isTwoX property based on the comment of the product in the first TestResult.
        /// If the comment is "x2" (case-insensitive), isTwoX is set to true; otherwise, false.
        /// </summary>
        private void SetX2Value()
        {
            isTwoX = TestResults[0].product.comment.ToLower() == "x2" ? true : false;
        }

        /// <summary>
        /// Automatically rebatches the names of the test results using the data loader service and the isTwoX flag.
        /// Clears the current TestResults collection and repopulates it with the updated results.
        /// </summary>
        public void AutoReBatchName()
        {
            var testResults = _dataLoaderService.AutoReBatchName(TestResults.ToList(), isTwoX);
            TestResults.Clear();
            PopulateTestResultsObservation(testResults);
        }

        /// <summary>
        /// Saves the current test results, evaluations, and measurements using the data loader service.
        /// Displays a success notification if the save is successful, otherwise logs and notifies of errors.
        /// Clears the TestResults, Evaluations, and Measurements collections after the operation.
        /// Manages the cursor display state during the save process.
        /// </summary>
        public async void SavingDataLoader()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var isSuccess = await _dataLoaderService.SavingDataLoader(TestResults.ToList(), Evaluations, Measurements);
                if (isSuccess) NotificationUtility.ShowSuccess("Test Results have been saved.");
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError(ex);
                NotificationUtility.ShowError("Failed to save the data. Please try again later.");
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
                TestResults.Clear();
                Evaluations.Clear();
                Measurements.Clear();
            }
        }

        /// <summary>
        /// Updates the test number of the given TestResult based on its suffix.
        /// If the suffix is not null or whitespace, assigns the test number from the SuffixTestAttemptPair dictionary.
        /// Otherwise, sets the test number to 1.
        /// </summary>
        public void SuffixChanged(TestResult testResult)
        {
            if (!string.IsNullOrWhiteSpace(testResult.suffix))
            {
                testResult.testNumber = Constants.Constants.SuffixTestAttemptPair[testResult.suffix];
            }
            else
            {
                testResult.testNumber = 1;
            }
        }

        /// <summary>
        /// Updates the test number of the given TestResult based on its test type.
        /// Sets testNumber to 0 for "W/U" and "STD" types, and to 1 for "BCH" type.
        /// Throws an exception if the test type is not valid.
        /// </summary>
        public void TestTypeChanged(TestResult testResult)
        {
            try
            {
                switch (testResult.testType)
                {
                    case "W/U":
                        testResult.testNumber = 0;
                        break;
                    case "STD":
                        testResult.testNumber = 0;
                        break;
                    case "BCH":
                        testResult.testNumber = 1;
                        break;
                }
            }
            catch
            {
                throw new Exception("Test Type is not valid.");
            }
        }

        /// <summary>
        /// Removes the specified TestResult from the TestResults collection.
        /// If the removal fails, logs the error and shows a notification.
        /// </summary>
        public void RemoveDataLoaderFunction(TestResult testResult)
        {
            for (int i = 0; i < TestResults.Count; i++)
            {
                if (testResult == TestResults[i])
                {
                    try
                    {
                        TestResults.RemoveAt(i);
                    }
                    catch (Exception ex)
                    {
                        NotificationUtility.ShowError("Failed to remove the test result.");
                        LoggerUtility.LogError(ex);
                    }
                }
            }
        }
    }
}
