using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.DataLoaderServiceTests
{
    [TestClass]
    public class DataLoaderServiceTests
    {
        private Mock<IProductRestAPI> _productRestAPIMock;
        private Mock<IInstrumentRestAPI> _instrumentRestAPIMock;
        private Mock<IBatchRestAPI> _batchRestAPIMock;
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;
        private Mock<ITestResultRestAPI> _testResultRestAPIMock;
        private Mock<IEvaluationRestAPI> _evaluationRestAPIMock;
        private Mock<IMeasurementRestAPI> _measurementRestAPIMock;

        private DataLoaderService _dataLoaderService;
        private List<Product> _sampleProducts;
        private List<Instrument> _sampleInstruments;

        /// <summary>
        /// Initializes test setup by creating mock objects and instantiating the DataLoaderService.
        /// Sets up sample products and instruments for testing.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _productRestAPIMock = new Mock<IProductRestAPI>();
            _instrumentRestAPIMock = new Mock<IInstrumentRestAPI>();
            _batchRestAPIMock = new Mock<IBatchRestAPI>();
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();
            _testResultRestAPIMock = new Mock<ITestResultRestAPI>();
            _evaluationRestAPIMock = new Mock<IEvaluationRestAPI>();
            _measurementRestAPIMock = new Mock<IMeasurementRestAPI>();

            _dataLoaderService = new DataLoaderService(
                _productRestAPIMock.Object,
                _instrumentRestAPIMock.Object,
                _batchRestAPIMock.Object,
                _batchTestResultRestAPIMock.Object,
                _testResultRestAPIMock.Object,
                _evaluationRestAPIMock.Object,
                _measurementRestAPIMock.Object
            );

            _sampleProducts = new List<Product>
            {
                new Product { id = 1, name = "TestProduct", status = true },
                new Product { id = 2, name = "AnotherProduct", status = true }
            };

            _sampleInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "Hapro", status = true },
                new Instrument { id = 2, name = "Machine2", status = true }
            };
        }

        #region ProcessTXTMeasurement Tests

        /// <summary>
        /// Tests that ProcessTXTMeasurement processes a TXT file and returns measurements, times, and torques.
        /// </summary>
        [TestMethod]
        public void ProcessTXTMeasurement_ValidTxtFile_ReturnsMeasurementsTimesTorques()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = "Time\tTQ\n" +
                         "S1\t10.5\n" +
                         "S2\t11.0\n" +
                         "S3\t12.5\n";
            File.WriteAllText(tempFile, content);

            try
            {
                // Act
                var result = _dataLoaderService.ProcessTXTMeasurement(tempFile);

                // Assert
                Assert.IsNotNull(result.Measurements);
                Assert.IsNotNull(result.Times);
                Assert.IsNotNull(result.Torques);
                Assert.AreEqual(3, result.Measurements.Count);
                Assert.AreEqual(3, result.Times.Count);
                Assert.AreEqual(3, result.Torques.Count);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region ProcessTXTEvaluation Tests

        /// <summary>
        /// Tests that ProcessTXTEvaluation creates evaluation points from provided times and torques data.
        /// </summary>
        [TestMethod]
        public void ProcessTXTEvaluation_ValidData_ReturnsEvaluations()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "dummy content");
            var times = new List<int> { 1, 2, 3, 4, 5, 10, 15, 20 };
            var torques = new List<double> { 5.0, 10.0, 15.0, 20.0, 18.0, 12.0, 14.0, 10.0 };

            try
            {
                // Act
                var result = _dataLoaderService.ProcessTXTEvaluation(tempFile, times, torques);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count > 0);
                foreach (var evaluation in result)
                {
                    Assert.IsNotNull(evaluation.pointName);
                    Assert.IsNotNull(evaluation.timeEval);
                    Assert.IsNotNull(evaluation.fileName);
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region AssignDefaultValuesAndArrangeResults Tests

        /// <summary>
        /// Tests that AssignDefaultValuesAndArrangeResults returns null when product is not found.
        /// </summary>
        [TestMethod]
        public async Task AssignDefaultValuesAndArrangeResults_NoProduct_ReturnsNull()
        {
            // Arrange
            var testResults = new List<MVVM.Models.TestResult>
            {
                new MVVM.Models.TestResult { product = null }
            };

            // Act
            var result = await _dataLoaderService.AssignDefaultValuesAndArrangeResults(testResults);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region AutoReBatchName Tests

        /// <summary>
        /// Tests that AutoReBatchName reassigns batch names sequentially for BCH test types.
        /// </summary>
        [TestMethod]
        public void AutoReBatchName_ValidTestResults_ReassignsBatchNames()
        {
            // Arrange
            var testResults = new List<MVVM.Models.TestResult>
            {
                new MVVM.Models.TestResult { testType = "W/U", batchName = "W/U" },
                new MVVM.Models.TestResult { testType = "BCH", batchName = "25A1" },
                new MVVM.Models.TestResult { testType = "BCH", batchName = "25A2" },
                new MVVM.Models.TestResult { testType = "BCH", batchName = "25A3" }
            };

            // Act
            var result = _dataLoaderService.AutoReBatchName(testResults, false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            var bchResults = result.Where(r => r.testType == "BCH").ToList();
            Assert.AreEqual(3, bchResults.Count);
            foreach (var bchResult in bchResults)
            {
                Assert.IsTrue(bchResult.batchName.StartsWith("25A"));
                Assert.IsFalse(bchResult.batchName.Contains("+"));
            }
        }

        /// <summary>
        /// Tests that AutoReBatchName creates double batch names when isTwoX is true.
        /// </summary>
        [TestMethod]
        public void AutoReBatchName_TwoXProduct_CreatesDoubleBatchNames()
        {
            // Arrange
            var testResults = new List<MVVM.Models.TestResult>
            {
                new MVVM.Models.TestResult { testType = "BCH", batchName = "25A1" },
                new MVVM.Models.TestResult { testType = "BCH", batchName = "25A2" }
            };

            // Act
            var result = _dataLoaderService.AutoReBatchName(testResults, true);

            // Assert
            Assert.IsNotNull(result);
            var bchResults = result.Where(r => r.testType == "BCH").ToList();
            foreach (var bchResult in bchResults)
            {
                Assert.IsTrue(bchResult.batchName.Contains("+"));
            }
        }

        #endregion

        #region SavingDataLoader Tests

        /// <summary>
        /// Tests that SavingDataLoader processes and saves test results, evaluations, and measurements successfully.
        /// </summary>
        [TestMethod]
        public async Task SavingDataLoader_ValidData_ReturnsTrue()
        {
            // Arrange
            var testResults = new List<MVVM.Models.TestResult>
            {
                new MVVM.Models.TestResult
                {
                    testType = "BCH",
                    batchName = "25A1",
                    product = _sampleProducts[0],
                    machine = _sampleInstruments[0],
                    suffix = "",
                    testNumber = 1
                }
            };
            var evaluations = new List<Evaluation>
            {
                new Evaluation { fileName = "test.txt" }
            };
            var measurements = new List<Measurement>
            {
                new Measurement { fileName = "test.txt" }
            };

            // Setup mocks
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());
            _testResultRestAPIMock.Setup(x => x.CreateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(new MVVM.Models.TestResult { id = 1 });
            _batchRestAPIMock.Setup(x => x.CreateBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(new Batch { id = 1 });
            _batchTestResultRestAPIMock.Setup(x => x.CreateBatchTestResultAsync(It.IsAny<BatchTestResult>()))
                .ReturnsAsync(new BatchTestResult());
            _evaluationRestAPIMock.Setup(x => x.CreateEvaluationAsync(It.IsAny<Evaluation>()))
                .ReturnsAsync(new Evaluation());
            _measurementRestAPIMock.Setup(x => x.CreateMeasurementAsync(It.IsAny<Measurement>()))
                .ReturnsAsync(new Measurement());

            // Act
            var result = await _dataLoaderService.SavingDataLoader(testResults, evaluations, measurements);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that SavingDataLoader returns false when an exception occurs during saving.
        /// </summary>
        [TestMethod]
        public async Task SavingDataLoader_ExceptionOccurs_ReturnsFalse()
        {
            // Arrange
            var testResults = new List<MVVM.Models.TestResult>
            {
                new MVVM.Models.TestResult
                {
                    testType = "BCH",
                    batchName = "25A1",
                    product = _sampleProducts[0],
                    machine = _sampleInstruments[0],
                    suffix = "",
                    testNumber = 1
                }
            };
            var evaluations = new List<Evaluation>();
            var measurements = new List<Measurement>();

            // Setup mock to throw exception
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _dataLoaderService.SavingDataLoader(testResults, evaluations, measurements);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
