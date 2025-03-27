using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public class DataLoaderService : IDataLoaderService
    {
        // readonly constants
        private readonly string _testResultTableName = "Testparameters";
        private readonly string _evaluationTableName = "Evaluation_Table";
        private readonly string _measurementTableName = "Measurement";
        private readonly string _testMethod = "Rheology";
        private readonly string _warmUp = "W/U";
        private readonly string _standard = "STD";
        private readonly string _batchTestType = "BCH";
        private readonly int _defaultAttempt = 1;
        private readonly List<string> _pointNames = new List<string> { "A", "B", "G", "X", "E", "t" };
        private readonly List<char> _haPointNames = new List<char> { 'A', 'B', 'X', 't' };
        private readonly string _defaultTxtFileMachine = "Hapro";
        private readonly double _torqueTolerance = 0.5;
        private readonly List<string> _allowedDuplicateSuffixInDatabase = new List<string> { "2.00min", "4.00min", "Cal", "RT", "Remix", "Rework" };
        private readonly List<string> _allowedDuplicateSuffixWithInNew = new List<string> { "2.00min", "4.00min", "Cal" };

        // services
        private readonly IProductRestAPI _productRestAPI;
        private readonly IInstrumentRestAPI _instrumentRestAPI;
        private readonly IBatchRestAPI _batchRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;
        private readonly ITestResultRestAPI _testResultRestAPI;
        private readonly IEvaluationRestAPI _evaluationRestAPI;
        private readonly IMeasurementRestAPI _measurementRestAPI;

        public DataLoaderService(
            IProductRestAPI productRestAPI, 
            IInstrumentRestAPI instrumentRestAPI, 
            IBatchRestAPI batchRestAPI, 
            IBatchTestResultRestAPI batchTestResultRestAPI,
            ITestResultRestAPI testResultRestAPI,
            IEvaluationRestAPI evaluationRestAPI,
            IMeasurementRestAPI measurementRestAPI)
        {
            this._productRestAPI = productRestAPI;
            this._instrumentRestAPI = instrumentRestAPI;
            this._batchRestAPI = batchRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
            this._testResultRestAPI = testResultRestAPI;
            this._evaluationRestAPI = evaluationRestAPI;
            this._measurementRestAPI = measurementRestAPI;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        //////////////////// New Data Loader Service ////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////
        public TestResult ProcessMTFTestResults(OleDbConnection connection, string fileName, List<Product> products, List<MVVM.Models.Instrument> instruments)
        {
            //var testResults = new List<TestResult>();
            TestResult result = new TestResult();

            OleDbCommand command = new OleDbCommand($"SELECT * FROM [{_testResultTableName}]", connection);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {

                    string paramName = reader["Parameter_Name"].ToString();
                    string paramValue = reader["Parameter_Value"].ToString();

                    switch (paramName)
                    {
                        case "Sample":
                            result.sampleNameFromMTF = paramValue;
                            result.product = FindProduct(fileName, products);
                            result.productId = result.product != null ? result.product.id : -1;
                            result.colour = "";
                            result.batchGroup = FindBatchGroup(paramValue, fileName);
                            break;
                        case "Order":
                            result.machine = FindMachine(paramValue, instruments);
                            result.machineId = result.machine != null ? result.machine.id : -1;
                            break;
                        case "Test_Date":
                            result.testDate = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Operator":
                            result.operatorName = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Drive_Unit":
                            result.driveUnit = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Mixer":
                            result.mixer = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Loading_Chute":
                            result.loadingChute = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Additive":
                            result.additive = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Speed":
                            result.speed = double.TryParse(paramValue, out double speed) ? (double?)speed : null;
                            break;
                        case "Mixer_Temp":
                            result.mixerTemp = double.TryParse(paramValue, out double mixerTemp) ? (double?)mixerTemp : null;
                            break;
                        case "Start_Temp":
                            result.startTemp = double.TryParse(paramValue, out double startTemp) ? (double?)startTemp : null;
                            break;
                        case "Meas_Range":
                            result.measRange = int.TryParse(paramValue, out int measRange) ? (int?)measRange : null;
                            break;
                        case "Damping":
                            result.damping = int.TryParse(paramValue, out int damping) ? (int?)damping : null;
                            break;
                        case "Test_Time":
                            result.testTime = double.TryParse(paramValue, out double testTime) ? (double?)testTime : null;
                            break;
                        case "Sample_Weight":
                            result.sampleWeight = double.TryParse(paramValue, out double sampleWeight) ? (double?)sampleWeight : null;
                            break;
                        case "Code_Number":
                            result.codeNumber = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "Plasticizer":
                            result.plasticizer = paramValue == "NULL" ? null : paramValue;
                            break;
                        case "Plast_Weight":
                            result.plastWeight = double.TryParse(paramValue, out double plastWeight) ? (double?)plastWeight : null;
                            break;
                        case "Load_Time":
                            result.loadTime = double.TryParse(paramValue, out double loadTime) ? (double?)loadTime : null;
                            break;
                        case "Load_Speed":
                            result.loadSpeed = double.TryParse(paramValue, out double tempLoadSpeed) ? (double?)tempLoadSpeed : null;
                            break;
                        case "Liquid":
                            result.liquid = paramValue == "NULL" ? null : paramValue; ;
                            break;
                        case "titrate":
                            result.titrate = double.TryParse(paramValue, out double titrate) ? (double?)titrate : null;
                            break;
                    }
                }
                result.testMethod = _testMethod;
                result.status = true;
                result.testNumber = _defaultAttempt;
                result.fileName = fileName;

                //testResults.Add(result);
            }

            return result;
        }

        public TestResult ProcessTxtTestResult(string file, List<Product> products, List<MVVM.Models.Instrument> instruments)
        {
            TestResult result = new TestResult();
            string fileName = Path.GetFileName(file);

            // populate result properties
            result.sampleNameFromMTF = string.Empty;
            result.product = FindProduct(fileName, products);
            result.productId = result.product != null ? result.product.id : -1;
            result.colour = string.Empty;
            result.batchGroup = FindBatchGroup(fileName.Split('.')[0]);

            result.machine = FindMachine(_defaultTxtFileMachine, instruments);
            result.machineId = result.machine != null ? result.machine.id : -1;

            DateTime lastWriteTime = File.GetLastWriteTime(file);
            result.testDate = lastWriteTime.ToString("dd/MM/yyyy HH:mm");

            result.operatorName = Constants.Constants.Username;

            result.driveUnit = string.Empty;
            result.mixer = string.Empty;
            result.loadingChute = string.Empty;
            result.additive = string.Empty;
            result.speed = null;
            result.mixerTemp = null;
            result.startTemp = null;
            result.measRange = null;
            result.damping = null;
            result.testTime = null;
            result.sampleWeight = null;
            result.codeNumber = string.Empty;
            result.plasticizer = string.Empty;
            result.plastWeight = null;
            result.loadTime = null;
            result.loadSpeed = null;
            result.liquid = null;
            result.titrate = null;

            result.testMethod = _testMethod;
            result.status = true;
            result.testNumber = _defaultAttempt;
            result.fileName = fileName;

            return result;
        }

        public (List<Measurement> Measurements, List<int> Times, List<double> Torques) ProcessTXTMeasurement(string file)
        {
            var measurements = new List<Measurement>();

            var torques = new List<double>();
            var times = new List<int>();
            string fileName = Path.GetFileName(file);

            using (var reader = new StreamReader(file))
            {
                List<string> headers = new List<string>();
                var headerLine = reader.ReadLine().Split('\t').ToList();

                foreach (var header in headerLine)
                {
                    headers.Add(header.Trim().ToLower());
                }

                var timeColumnIndex = headers.IndexOf("time");
                var torqueColumnIndex = headers.IndexOf("tq");

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var columns = line.Split('\t');

                        if (columns.Count() == headers.Count())
                        {
                            int time = int.Parse(columns[timeColumnIndex].Substring(1));
                            double torque = double.Parse(columns[torqueColumnIndex].Trim());

                            times.Add(time);
                            torques.Add(torque);

                            Measurement measurement = new Measurement
                            {
                                timeAct = GetTimeFormatFromInt(time),
                                torque = torque,
                                bandwidth = null,
                                stockTemp = null,
                                speed = null,
                                fileName = fileName,
                            };
                            measurements.Add(measurement);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerUtility.LogError(ex);
                        continue;
                    }
                }
            }

            return (measurements, times, torques);
        }

        public List<Evaluation> ProcessTXTEvaluation(string file, List<int> times, List<double> torques)
        {
            var evaluations = new List<Evaluation>();

            int startPoint = 0;
            var timeTorquePair = GetTorqueTxt(times, torques);

            string fileName = Path.GetFileName(file);

            foreach (var point in _haPointNames)
            {
                try
                {
                    int timeInt = timeTorquePair[point].Keys.First();
                    double torque = timeTorquePair[point].Values.First();

                    if (timeTorquePair != null)
                    {
                        Evaluation evaluation = new Evaluation
                        {
                            point = startPoint,
                            pointName = point.ToString(),
                            timeEval = GetTimeFormatFromInt(timeInt),
                            torque = torque,
                            bandwidth = null,
                            stockTemp = null,
                            speed = null,
                            energy = null,
                            timeRange = "00:00:00",
                            torqueRange = 0,
                            timeEvalInt = timeInt,
                            timeRangeInt = 0,
                            fileName = fileName,
                        };
                        startPoint++;
                        evaluations.Add(evaluation);
                    }

                }
                catch (Exception ex)
                {
                    LoggerUtility.LogError(ex);
                    continue;
                }
            }

            return evaluations;
        }

        private string GetTimeFormatFromInt(int totalSecond)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSecond);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        private Dictionary<char, Dictionary<int, double>> GetTorqueTxt(List<int> times, List<double> torques)
        {
            var torquesTimesDict = new Dictionary<char, Dictionary<int, double>>();
            var aPoint = new Dictionary<int, double>();
            var bPoint = new Dictionary<int, double>();
            var xPoint = new Dictionary<int, double>();
            var tPoint = new Dictionary<int, double>();


            // Find A (Peak) Torque
            double aTorque = torques.Max();
            int indexOfAPoint = torques.IndexOf(aTorque);
            int aTime = times[indexOfAPoint];

            aPoint.Add(aTime, aTorque);
            torquesTimesDict.Add('A', aPoint);

            // Find B and X Torque
            List<double> bPointTorquesList = torques.GetRange(indexOfAPoint + 1, torques.Count - (indexOfAPoint + 1));
            List<int> bPointTimesList = times.GetRange(indexOfAPoint + 1, times.Count - (indexOfAPoint + 1));

            double bTorque = 0;
            double xTorque = 0;
            int indexOfBPoint = 0;
            int indexOfXPoint = 0;
            int bTime = 0;
            int xTime = 0;

            for (int i = 1; i < bPointTorquesList.Count(); i++)
            {
                if (xTorque == 0 && bPointTorquesList[i] < aTorque && bPointTorquesList[i] < (bPointTorquesList[i - 1] + 0.5))
                {
                    bTorque = bPointTorquesList[i];
                    indexOfBPoint = i;
                }

                if (bTorque > 0 && bTorque < bPointTorquesList[i] && xTorque < bPointTorquesList[i])
                {
                    xTorque = bPointTorquesList[i];
                    indexOfXPoint = i;
                }
            }
            bTime = bPointTimesList[indexOfBPoint];
            xTime = bPointTimesList[indexOfXPoint];

            bPoint.Add(bTime, bTorque);
            torquesTimesDict.Add('B', bPoint);


            // Re-calculate X Torque and X time in case flat curve

            List<int> xPointTimesList = bPointTimesList.GetRange(indexOfBPoint + 1, bPointTimesList.Count - (indexOfBPoint + 1));
            List<double> xPointTorquesList = bPointTorquesList.GetRange(indexOfBPoint + 1, bPointTorquesList.Count - (indexOfBPoint + 1));

            var newIndexOfXPoint = xPointTimesList.IndexOf(xTime);

            int startIndex = newIndexOfXPoint + 15;

            // Ensure that the startIndex is within the list bounds
            if (startIndex < xPointTorquesList.Count)
            {
                // Calculate the number of elements to remove
                int lastListRange = xPointTorquesList.Count - startIndex;

                // Adjust the lastListRange to ensure it doesn't exceed the list bounds
                lastListRange = Math.Min(lastListRange, xPointTorquesList.Count - startIndex);

                if (lastListRange > 0)
                {
                    xPointTimesList.RemoveRange(startIndex, lastListRange);
                    xPointTorquesList.RemoveRange(startIndex, lastListRange);
                }
            }

            var relevantIndices = xPointTorquesList
                .Select((value, index) => new { Index = index, Value = value })
                .Where(item => Math.Abs(item.Value - xTorque) < _torqueTolerance)
                .ToList();

            if (relevantIndices.Any())
            {
                var firstIndex = relevantIndices.First().Index;
                var lastIndex = relevantIndices.Last().Index;


                var firstTimeX = xPointTimesList[firstIndex];
                var lastTimeX = xPointTimesList[lastIndex];

                xTime = (int)Math.Floor((firstTimeX + lastTimeX) / 2.0);

                var indexOfAveTimeX = xPointTimesList.IndexOf(xTime);

                xTorque = xPointTorquesList[indexOfAveTimeX];
            }

            xPoint.Add(xTime, xTorque);
            torquesTimesDict.Add('X', xPoint);

            // Find t (fusion) Time and Torque
            int tTime = torquesTimesDict['X'].Keys.First() - torquesTimesDict['A'].Keys.First();
            double tTorque = torquesTimesDict['A'].Values.First() - torquesTimesDict['X'].Values.First();

            tPoint.Add(tTime, tTorque);
            torquesTimesDict.Add('t', tPoint);

            return torquesTimesDict;
        }

        private Product FindProduct(string paramValue, List<Product> products)
        {
            var sample = paramValue as string;
            string importProductName = sample.Split('_')[0];

            foreach (var product in products)
            {
                string processedProductName = ProcessProductName(product.name);

                if (string.Equals(processedProductName, importProductName, StringComparison.OrdinalIgnoreCase))
                {
                    return product;
                }
            }
            return null;
        }

        private string ProcessProductName(string productName)
        {
            foreach (var wordToExclude in Constants.Constants.ClippingProductName)
            {
                productName = productName.Replace(wordToExclude, "");
            }
            return productName;
        }

        private string FindBatchGroup(object v, string fileName = "")
        {
            var batchGroupFromMachine = v as string;
            string batchGroupFromFileName = "";

            if (fileName != "")
            {
                batchGroupFromFileName = fileName.Split('_')[1];
            }
            else
            {
                batchGroupFromFileName = batchGroupFromMachine.Split('_')[1];
            }


            if (!string.IsNullOrWhiteSpace(fileName) && (batchGroupFromFileName != batchGroupFromMachine))
            {
                return batchGroupFromFileName;
            }

            return batchGroupFromFileName;
        }

        private MVVM.Models.Instrument FindMachine(string paramValue, List<MVVM.Models.Instrument> instruments)
        {
            var machineName = paramValue as string;

            foreach (var instrument in instruments)
            {
                if (string.Equals(machineName, instrument.name, StringComparison.OrdinalIgnoreCase))
                {
                    return instrument;
                }
            }
            return null;
        }

        public async Task<List<TestResult>> AssignDefaultValuesAndArrangeResults(List<TestResult> testResults)
        {
            if (testResults[0].product == null)
            {
                NotificationUtility.ShowError("Cannot find the product or product is inactive");
                return null;
            }

            //var product = testResults[0].product;
            var isTwoX = string.IsNullOrWhiteSpace(testResults[0].product.comment) ? false : testResults[0].product.comment.ToLower() == "x2" ? true : false;
            testResults[0].testType = _warmUp;
            testResults[0].testNumber = 0;
            string yearMonthBatch = testResults[0].batchGroup.Length >= 3 ? testResults[0].batchGroup.Substring(0, 3) : testResults[0].batchGroup;
            var nextBatchNumber = await FindNextBatchName(yearMonthBatch, testResults[0].product.name);

            // Find if standard test exist in the same product and batch group
            var batchTestResultAPI = new BatchTestResultRestAPI();
            string filterExistingBatchTestResult = $"?batchName=STD&productName={testResults[0].product.name}&batchGroup={testResults[0].batchGroup}&machineName={testResults[0].machine.name}";
            var existingSTDTestResults = await batchTestResultAPI.GetAllBatchTestResultsAsync(filter: filterExistingBatchTestResult);

            if (existingSTDTestResults.Count == 0)
            {
                testResults[1].testType = _standard;
                testResults[1].batchName = "1";
                testResults[1].testNumber = 0;
                testResults[2].testType = _standard;
                testResults[2].batchName = "2";
                testResults[2].testNumber = 0;


                for (int i = 3; i < testResults.Count; i++)
                {
                    if (isTwoX)
                    {
                        testResults[i].testType = _batchTestType;
                        testResults[i].batchName = $"{yearMonthBatch}{nextBatchNumber++}+{nextBatchNumber++}";
                    }
                    else
                    {
                        testResults[i].testType = _batchTestType;
                        testResults[i].batchName = $"{yearMonthBatch}{nextBatchNumber++}";
                    }
                }
            }
            else
            {
                for (int i = 1; i < testResults.Count; i++)
                {
                    if (isTwoX)
                    {
                        testResults[i].testType = _batchTestType;
                        testResults[i].batchName = $"{yearMonthBatch}{nextBatchNumber++}+{nextBatchNumber++}";
                    }
                    else
                    {
                        testResults[i].testType = _batchTestType;
                        testResults[i].batchName = $"{yearMonthBatch}{nextBatchNumber++}";
                    }
                }
            }

            return testResults;
        }

        private async Task<int> FindNextBatchName(string yearMonthBatch, string productName)
        {
            List<int> currentBatchesNumber = new List<int>();
            string batchesFilter = $"?batchName={yearMonthBatch}&productName={productName}";

            var batches = await _batchRestAPI.GetAllBatchesAsync(filter: batchesFilter);

            if (batches.Count > 0 && batches != null)
            {
                foreach (var batch in batches)
                {
                    string stringBatchNumber = batch.batchName.Length > 3 ? batch.batchName.Substring(3) : string.Empty;
                    if (int.TryParse(stringBatchNumber, out int intBatchNumber))
                    {
                        currentBatchesNumber.Add(intBatchNumber);
                    }
                }

                int batchNumber = currentBatchesNumber.Max() + 1;

                return batchNumber;
            }
            return 1;
        }

        public List<Evaluation> ProcessMTFEvaluations(OleDbConnection connection, string fileName)
        {
            var evaluations = new List<Evaluation>();

            OleDbCommand command = new OleDbCommand($"SELECT * FROM [{_evaluationTableName}]", connection);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                int startPoint = 0;
                while (reader.Read())
                {
                    Evaluation evaluation = new Evaluation
                    {
                        point = startPoint,
                        pointName = _pointNames[startPoint],
                        timeEval = reader["Time_Eval"].ToString(),
                        torque = Convert.ToDouble(reader["Torque"]),
                        bandwidth = Convert.ToDouble(reader["Bandwidth"]),
                        stockTemp = Convert.ToInt32(reader["Stock_Temp"]),
                        speed = Convert.ToDouble(reader["Speed"]),
                        energy = Convert.ToDouble(reader["Energy"]),
                        timeRange = reader["Time_Range"].ToString(),
                        torqueRange = Convert.ToInt32(reader["Torque_Range"]),
                        timeEvalInt = Convert.ToInt32(reader["Time_Eval_Int"]),
                        timeRangeInt = Convert.ToInt32(reader["Time_Range_Int"]),
                        fileName = fileName,
                    };
                    startPoint++;
                    evaluations.Add(evaluation);
                }
            }

            return evaluations;
        }

        public List<Measurement> ProcessMTFMeasurements(OleDbConnection connection, string fileName)
        {
            var measurements = new List<Measurement>();

            OleDbCommand command = new OleDbCommand($"SELECT * FROM [{_measurementTableName}]", connection);
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Measurement measurement = new Measurement
                    {
                        timeAct = reader["Time_Act"].ToString(),
                        torque = Convert.ToDouble(reader["Torque"]),
                        bandwidth = Convert.ToDouble(reader["Bandwidth"]),
                        stockTemp = Convert.ToDouble(reader["Stock_Temp"]),
                        speed = Convert.ToDouble(reader["Speed"]),
                        fileName = fileName,
                    };
                    measurements.Add(measurement);
                }
            }

            return measurements;
        }

        public List<TestResult> AutoReBatchName(List<TestResult> testResults, bool isTwoX)
        {
            try
            {
                var startBatchName = testResults.FirstOrDefault(TestResults => TestResults.testType == _batchTestType).batchName ?? "";

                if (startBatchName.Contains('+'))
                {
                    startBatchName = startBatchName.Split('+')[0];
                }
                else if (startBatchName.Contains('-'))
                {
                    startBatchName = startBatchName.Split('-')[0];
                }

                string yearMonthBatch = startBatchName.Length >= 3 ? startBatchName.Substring(0, 3) : "";
                int startBatchNumber = startBatchName.Length > 3 ? int.Parse(startBatchName.Substring(3)) : 1;

                foreach (var result in testResults)
                {
                    if (result.testType == _batchTestType)
                    {
                        if (isTwoX)
                        {
                            result.batchName = $"{yearMonthBatch}{startBatchNumber++}+{startBatchNumber++}";
                        }
                        else
                        {
                            result.batchName = $"{yearMonthBatch}{startBatchNumber++}";

                        }
                    }
                }
                return testResults;

            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Cannot find the batch name to start the re-batch number");
                LoggerUtility.LogError(ex);
                return testResults;
            }
        }

        public async Task<bool> SavingDataLoader(List<TestResult> testResults, List<Evaluation> evaluations, List<Measurement> measurements)
        {
            try
            {
                var arrangedTestResults = ReArrangeTestResults(testResults);

                // verify if the batch name is valid 
                bool isBatchNameValid = await BatchNameVerification(arrangedTestResults);

                if (isBatchNameValid)
                {
                    foreach (var result in arrangedTestResults)
                    {
                        // Find if the result is a new batch or existing batch
                        // if not exist or suffix is in allow duplicate in database => create new data in database  => TestResult, Batch, BatchTestResult, Evaluation, Measurement
                        // if exist => update the data in database => TestResult, BatchTestResult, Evaluation, Measurement

                        if (result.testType == _batchTestType)
                        {
                            string batchTestResultFilter = $"?exactBatchName={result.batchName}&productName={result.product.name}&testNumber={result.testNumber}";
                            var existingBatchTestResult = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchTestResultFilter);

                            if (existingBatchTestResult.Count > 0 && !_allowedDuplicateSuffixInDatabase.Contains(result.suffix))
                            {
                                await UpdateOldData(result, existingBatchTestResult, evaluations, measurements);
                                continue;
                            }

                        }
                        await CreateNewData(result, evaluations, measurements);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to save data to database");
                LoggerUtility.LogError(ex);
                return false;
            }
        }

        private async Task CreateNewData(TestResult result, List<Evaluation> evaluations, List<Measurement> measurements)
        {
            // Save TestResult data to database
            var createdTestResult = await _testResultRestAPI.CreateTestResultAsync(result);
            // Save Batch
            Batch createdBatch = await CreateBatchFunction(result);
            // Save BatchTestResult
            await CreateBatchTestResultFunction(batchId: createdBatch.id, testResultId: createdTestResult.id);

            // Save Evaluations
            await CreateEvaluation(result.fileName, createdTestResult.id, evaluations);

            // Save Measurements
            await CreateMeasurement(result.fileName, createdTestResult.id, measurements);
        }

        private async Task<Batch> CreateBatchFunction(TestResult result)
        {
            Batch createdBatch = new Batch
            {
                sampleBy = result.sampleBy,
                productId = result.product.id,
                batchName = result.batchName.ToUpper(),
                suffix = result.suffix
            };

            createdBatch = await _batchRestAPI.CreateBatchAsync(createdBatch);
            return createdBatch;
        }

        private async Task CreateBatchTestResultFunction(int batchId, int testResultId)
        {
            BatchTestResult batchTestResult = new BatchTestResult
            {
                batchId = batchId,
                testResultId = testResultId,
            };

            await _batchTestResultRestAPI.CreateBatchTestResultAsync(batchTestResult);
        }

        private async Task CreateEvaluation(string fileName, int testResultId, List<Evaluation> evaluations)
        {
            var createEvaluations = evaluations.FindAll(ev => ev.fileName == fileName);

            if (createEvaluations.Count > 0)
            {
                foreach (var evaluation in createEvaluations)
                {
                    evaluation.testResultId = testResultId;
                    await _evaluationRestAPI.CreateEvaluationAsync(evaluation);
                }
            }
        }

        private async Task CreateMeasurement(string fileName, int testResultId, List<Measurement> measurements)
        {
            var createMeasurements = measurements.FindAll(ms => ms.fileName == fileName);
            if (createMeasurements.Count > 0)
            {
                foreach (var measurement in createMeasurements)
                {
                    measurement.testResultId = testResultId;
                    await _measurementRestAPI.CreateMeasurementAsync(measurement);
                }
            }
        }


        private async Task UpdateOldData(TestResult result, List<BatchTestResult> existingBatchTestResult, List<Evaluation> evaluations, List<Measurement> measurements)
        {
            var testResultId = existingBatchTestResult.LastOrDefault().testResult.id;

            var batchId = existingBatchTestResult.LastOrDefault().batch.id;

            // Update TestResult data
            await UpdateTestResultData(result, testResultId);

            // Update Batch
            await UpdateBatchData(result, batchId);

            // Update Evaluation
            await UpdateEvaluationsData(result.fileName, testResultId, evaluations);

            // Update Measurement
            await UpdateMeasurementData(result.fileName, testResultId, measurements);
        }

        private async Task UpdateTestResultData(TestResult result, int testResultId)
        {
            var existingTestResult = await _testResultRestAPI.GetTestResultByIdAsync(testResultId);

            if (existingTestResult != null)
            {
                existingTestResult.productId = result.product.id;
                existingTestResult.machineId = result.machine.id;
                existingTestResult.testDate = result.testDate;
                existingTestResult.operatorName = result.operatorName;
                existingTestResult.driveUnit = result.driveUnit;
                existingTestResult.mixer = result.mixer;
                existingTestResult.loadingChute = result.loadingChute;
                existingTestResult.additive = result.additive;
                existingTestResult.speed = result.speed;
                existingTestResult.mixerTemp = result.mixerTemp;
                existingTestResult.startTemp = result.startTemp;
                existingTestResult.measRange = result.measRange;
                existingTestResult.damping = result.damping;
                existingTestResult.testTime = result.testTime;
                existingTestResult.sampleWeight = result.sampleWeight;
                existingTestResult.codeNumber = result.codeNumber;
                existingTestResult.plasticizer = result.plasticizer;
                existingTestResult.plastWeight = result.plastWeight;
                existingTestResult.loadTime = result.loadTime;
                existingTestResult.loadSpeed = result.loadSpeed;
                existingTestResult.liquid = result.liquid;
                existingTestResult.titrate = result.titrate;
                existingTestResult.testNumber = result.testNumber;
                existingTestResult.testType = result.testType;
                existingTestResult.batchGroup = result.batchGroup;
                existingTestResult.testMethod = result.testMethod;
                existingTestResult.colour = result.colour;
                existingTestResult.status = true;
                existingTestResult.fileName = result.fileName;

                await _testResultRestAPI.UpdateTestResultAsync(existingTestResult);
            }
        }

        private async Task UpdateBatchData(TestResult result, int batchId)
        {
            var existingBatch = await _batchRestAPI.GetBatchByIdAsync(batchId);

            if (existingBatch != null)
            {
                existingBatch.sampleBy = result.sampleBy;
                existingBatch.productId = result.product.id;
                existingBatch.batchName = result.batchName.ToUpper();
                existingBatch.suffix = result.suffix;

                await _batchRestAPI.UpdateBatchAsync(existingBatch);
            }
        }

        private async Task UpdateEvaluationsData(string fileName, int testResultId, List<Evaluation> evaluations)
        {
            string evaluationFilter = $"?testResultId={testResultId}";
            //var existingEvaluations = await GetAllEvaluationAsync(filter: evaluationFilter);
            var existingEvaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: evaluationFilter);

            var updateEvaluation = evaluations.Where(ev => ev.fileName == fileName).ToList();

            if (updateEvaluation.Count > 0)
            {
                // update the updateEvaluation to Database given id from existingEvaluation matching by point name
                foreach (var evaluation in updateEvaluation)
                {
                    var existingEvaluation = existingEvaluations.FirstOrDefault(ev => ev.pointName == evaluation.pointName);

                    if (existingEvaluation != null)
                    {
                        existingEvaluation.testResultId = testResultId;
                        existingEvaluation.point = evaluation.point;
                        existingEvaluation.pointName = evaluation.pointName;
                        existingEvaluation.timeEval = evaluation.timeEval;
                        existingEvaluation.torque = evaluation.torque;
                        existingEvaluation.bandwidth = evaluation.bandwidth;
                        existingEvaluation.stockTemp = evaluation.stockTemp;
                        existingEvaluation.speed = evaluation.speed;
                        existingEvaluation.energy = evaluation.energy;
                        existingEvaluation.timeRange = evaluation.timeRange;
                        existingEvaluation.torqueRange = evaluation.torqueRange;
                        existingEvaluation.timeEvalInt = evaluation.timeEvalInt;
                        existingEvaluation.timeRangeInt = evaluation.timeRangeInt;
                        existingEvaluation.fileName = evaluation.fileName;

                        await _evaluationRestAPI.UpdateEvaluationAsync(existingEvaluation);
                    }
                }

            }
        }

        private async Task UpdateMeasurementData(string fileName, int testResultId, List<Measurement> measurements)
        {
            string measurementFilter = $"?testResultId={testResultId}";
            var existingMeasurements = await _measurementRestAPI.GetAllMeasurementsAsync(filter: measurementFilter);

            var updateMeasurements = measurements.Where(me => me.fileName == fileName).ToList();

            if (updateMeasurements.Count > 0)
            {
                foreach (var measurement in updateMeasurements)
                {
                    var existingMeasurement = existingMeasurements.FirstOrDefault(me => me.timeAct == measurement.timeAct);
                    if (existingMeasurement != null)
                    {
                        existingMeasurement.testResultId = testResultId;
                        existingMeasurement.timeAct = measurement.timeAct;
                        existingMeasurement.torque = measurement.torque;
                        existingMeasurement.bandwidth = measurement.bandwidth;
                        existingMeasurement.stockTemp = measurement.stockTemp;
                        existingMeasurement.speed = measurement.speed;
                        existingMeasurement.fileName = measurement.fileName;

                        await _measurementRestAPI.UpdateMeasurementAsync(measurement);
                    }
                }
            }

        }

        private List<TestResult> ReArrangeTestResults(List<TestResult> results)
        {
            var testResults = new List<TestResult>();

            foreach (var result in results)
            {
                // Get the batch name from different test type and Remove suffix
                result.batchName = GetBatchName(result);

                // Duplicate the batch test result with batch name contains "+" (double batches test)
                if (result.testType == _batchTestType && (result.batchName.Contains("+") || result.batchName.Contains('-')))
                {
                    string yearMonth = result.batchName.Substring(0, 3);
                    string firstBatchName = result.batchName.Split('+', '-')[0].Trim();
                    string secondBatchName = yearMonth + result.batchName.Split('+', '-')[1].Trim();

                    TestResult firstResult = DuplicateTestResultWithNewBatchName(result, firstBatchName);
                    TestResult secondResult = DuplicateTestResultWithNewBatchName(result, secondBatchName);
                    testResults.Add(firstResult);
                    testResults.Add(secondResult);
                }
                else
                {
                    testResults.Add(result);
                }
            }

            return testResults;
        }

        private string GetBatchName(TestResult result)
        {
            string batchName;

            switch (result.testType)
            {
                case "W/U":
                    batchName = $"{result.testType}";
                    break;
                case "STD":
                    batchName = $"{result.testType}{result.batchName}";
                    break;
                case "BCH":
                    batchName = BatchUtility.RemoveTrailingAlphabetBatchName(result.batchName).Trim();
                    break;
                default:
                    batchName = BatchUtility.RemoveTrailingAlphabetBatchName(result.batchName).Trim();
                    break;
            }
            return batchName;
        }

        private TestResult DuplicateTestResultWithNewBatchName(TestResult result, string newBatchName)
        {
            return new TestResult
            {
                product = result.product,
                machine = result.machine,
                productId = result.productId,
                machineId = result.machineId,
                testDate = result.testDate,
                operatorName = result.operatorName,
                driveUnit = result.driveUnit,
                mixer = result.mixer,
                loadingChute = result.loadingChute,
                additive = result.additive,
                speed = result.speed,
                mixerTemp = result.mixerTemp,
                startTemp = result.startTemp,
                measRange = result.measRange,
                damping = result.damping,
                testTime = result.testTime,
                sampleWeight = result.sampleWeight,
                codeNumber = result.codeNumber,
                plasticizer = result.plasticizer,
                plastWeight = result.plastWeight,
                loadTime = result.loadTime,
                loadSpeed = result.loadSpeed,
                liquid = result.liquid,
                titrate = result.titrate,
                testNumber = result.testNumber,
                testType = result.testType,
                batchGroup = result.batchGroup,
                testMethod = result.testMethod,
                colour = result.colour,
                status = result.status,
                fileName = result.fileName,
                batchName = newBatchName,
                sampleBy = result.sampleBy,
                sampleNameFromMTF = result.sampleNameFromMTF,
                torque = result.torque,
                fusion = result.fusion,
                suffix = result.suffix,
            };
        }


        private async Task<bool> BatchNameVerification(List<TestResult> results)
        {
            // check if new input batch names, product name, test number are duplicated each other in the new input batch names except the suffix is 2.00min or 4.00min or Cal.
            // check if new input batch names, product name, test number are duplicated with existing batch names in the database except RT, 2.00min, 4.00min, Cal.

            bool isSTDBathNameVerified = await IsStandardBatchNameVerified(results);

            bool isBCHBatchNameVerified = await IsBCHBatchNameVerified(results);

            return isSTDBathNameVerified && isBCHBatchNameVerified;

        }

        private async Task<bool> IsStandardBatchNameVerified(List<TestResult> results)
        {
            // check if new input batch names duplicated each other in the new input batch names (for the standard test type such as STD1, STD2, STD3 and so on)
            var standardTestResults = results.Where(TestResults => TestResults.testType == _standard).ToList();

            if (standardTestResults.Count == 0)
            {
                return true;
            }
            else
            {
                return IsNewInputSTDVerified(standardTestResults) && !await IsNewInputSTDExisted(standardTestResults);
            }

        }

        private async Task<bool> IsBCHBatchNameVerified(List<TestResult> results)
        {
            var bchTestResults = results.Where(TestResults => TestResults.testType == _batchTestType).ToList();
            if (bchTestResults.Count == 0)
            {
                return true;
            }
            else
            {
                return IsSuffixFormatValid(bchTestResults) && IsNewInputBCHValid(bchTestResults) && !await IsExistingBCHExisted(bchTestResults);

            }
        }

        private async Task<bool> IsExistingBCHExisted(List<TestResult> bchTestResults)
        {
            // condition:
            // 1. if no suffix, check if the batch name & testNumber = 1 is duplicated with existing batch names in the database => not allow => ask for replacement or not
            // 2. if suffix is RS, check if the batch name & testNumber = 2 is duplicated with existing batch names in the database => not allow => ask for replacement or not
            // 3. if suffix is RRS, check of the batch name & testNumber = 3 is duplicated with existing batch names in the database => not allow => ask for replacement or not
            // 4. if other suffix, allow to save as duplicate batch name.

            var filteredBCHTestResults = bchTestResults.Where(TestResults => !_allowedDuplicateSuffixInDatabase.Contains(TestResults.suffix)).ToList();

            if (filteredBCHTestResults.Count > 0)
            {
                var coExistingBatchTestResults = new List<BatchTestResult>();

                foreach (var result in filteredBCHTestResults)
                {
                    string filterExistingBatchTestResult = $"?exactBatchName={result.batchName}&productName={result.product.name}&testNumber={result.testNumber}";
                    //var existingBatchTestResult = await GetBatchTestResultAsync(filter: filterExistingBatchTestResult);
                    var existingBatchTestResult = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: filterExistingBatchTestResult);

                    if (existingBatchTestResult != null && existingBatchTestResult.Count > 0)
                    {
                        coExistingBatchTestResults.AddRange(existingBatchTestResult);
                    }
                }

                if (coExistingBatchTestResults.Count > 0)
                {
                    string message = "The following batch names have existed in database.\n\n";
                    foreach (var existingBatchTestResult in coExistingBatchTestResults)
                    {
                        message += $"{existingBatchTestResult.testResult.product.name} \n" +
                                   $"{existingBatchTestResult.batch.batchName} {existingBatchTestResult.batch.suffix}\n" +
                                   $"Test Attempt {existingBatchTestResult.testResult.testNumber}\n\n";
                    }

                    message += "Do you want to replace the existing test results with the new input test results?";

                    // Display confirmation message using MessageBox
                    var result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> IsNewInputSTDExisted(List<TestResult> standardTestResults)
        {
            var standardBatchNames = standardTestResults.Select(TestResults => TestResults.batchName).ToList();

            foreach (var standardBatchName in standardBatchNames)
            {
                string filterExistingBatchTestResult = $"?exactBatchName=STD{standardBatchName}&productName={standardTestResults[0].product.name}&batchGroup={standardTestResults[0].batchGroup}";
                var existingStandardBatchTestResult = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: filterExistingBatchTestResult);

                if (existingStandardBatchTestResult != null && existingStandardBatchTestResult.Count > 0)
                {
                    NotificationUtility.ShowWarning("The Standard Number has existed in database.");
                    return true;
                }
            }
            return false;
        }

        private bool IsNewInputSTDVerified(List<TestResult> standardTestResults)
        {
            var standardBatchNames = standardTestResults.Select(TestResults => TestResults.batchName).ToList();

            if (standardBatchNames.Count != standardBatchNames.Distinct().Count())
            {
                NotificationUtility.ShowWarning("The Standard Number are duplicated each other.");
                return false;
            }

            return true;
        }

        private bool IsNewInputBCHValid(List<TestResult> bchTestResults)
        {
            // check if the suffix is not 2.00min or 4.00min or Cal, duplicate batch name is not allow (check with in the new input batch names)

            // Remove the allowed suffix from the list
            var filteredBCHTestResults = bchTestResults.Where(TestResults => !_allowedDuplicateSuffixWithInNew.Contains(TestResults.suffix)).ToList();

            if (filteredBCHTestResults.Count > 0)
            {
                // Get Batch names list
                var batchNames = filteredBCHTestResults.Select(TestResults => TestResults.batchName).ToList();

                // Check if the batch names are duplicated
                if (batchNames.Count != batchNames.Distinct().Count())
                {
                    NotificationUtility.ShowWarning("The Batch Name are duplicated each other.");
                    return false;
                }
            }

            return true;
        }

        private bool IsSuffixFormatValid(List<TestResult> bchTestResults)
        {
            foreach (var result in bchTestResults)
            {
                // Check if suffix is in Key of Constant SuffixTestAttemptPair Dictionary
                if (!string.IsNullOrWhiteSpace(result.suffix) && !Constants.Constants.SuffixTestAttemptPair.ContainsKey(result.suffix))
                {
                    NotificationUtility.ShowWarning("The suffix of the batch name is invalid.");
                    return false;
                }
            }
            return true;
        }
    }
}
