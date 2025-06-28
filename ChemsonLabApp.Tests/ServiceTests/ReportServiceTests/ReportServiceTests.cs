using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ChemsonLabApp.Tests.ServiceTests.ReportServiceTests
{
    [TestClass]
    public class ReportServiceTests
    {
        private Mock<IReportRestAPI> _reportRestAPIMock;
        private Mock<ITestResultReportRestAPI> _testResultReportRestAPIMock;
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;
        private Mock<IEvaluationRestAPI> _evaluationRestAPIMock;
        private ReportService _reportService;

        [TestInitialize]
        public void Setup()
        {
            _reportRestAPIMock = new Mock<IReportRestAPI>();
            _testResultReportRestAPIMock = new Mock<ITestResultReportRestAPI>();
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();
            _evaluationRestAPIMock = new Mock<IEvaluationRestAPI>();
            _reportService = new ReportService(
                _reportRestAPIMock.Object,
                _testResultReportRestAPIMock.Object,
                _batchTestResultRestAPIMock.Object,
                _evaluationRestAPIMock.Object
            );
        }

        /// <summary>
        /// Tests that CalculateAveTestTimeTick returns 0 for empty input and calculates correct average for valid input.
        /// </summary>
        [TestMethod]
        public void CalculateAveTestTimeTick_ValidAndEmptyInput_ReturnsExpected()
        {
            // Empty input
            var empty = new List<BatchTestResult>();
            var resultEmpty = _reportService.CalculateAveTestTimeTick(empty);
            Assert.AreEqual(0, resultEmpty);

            // Valid input with known time difference - using exact format expected by the service
            var baseTime = new DateTime(2024, 1, 1, 10, 0, 0);
            var format = "dd/MM/yyyy HH:mm";
            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    testResult = new MVVM.Models.TestResult
                    {
                        testDate = baseTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture)
                    }
                },
                new BatchTestResult
                {
                    testResult = new MVVM.Models.TestResult
                    {
                        testDate = baseTime.AddMinutes(10).ToString(format, System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            };

            var result = _reportService.CalculateAveTestTimeTick(batchTestResults);

            // The result should be greater than 0 for valid time differences
            Assert.IsTrue(result > 0, "Expected positive ticks for time difference");
        }

        /// <summary>
        /// Tests that SaveOrUpdateReports returns true when all operations succeed.
        /// </summary>
        [TestMethod]
        public async Task SaveOrUpdateReports_ValidInput_ReturnsTrue()
        {
            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    batch = new Batch { batchName = "24001" },
                    testResult = new MVVM.Models.TestResult { testDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), product = new Product { name = "P1" } }
                }
            };
            var report = new Report { id = 1 };
            _reportRestAPIMock.Setup(x => x.CreateReportAsync(It.IsAny<Report>())).ReturnsAsync(report);
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), "")).ReturnsAsync(batchTestResults);
            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), "")).ReturnsAsync(new List<TestResultReport>());
            _testResultReportRestAPIMock.Setup(x => x.CreateTestResultReportAsync(It.IsAny<TestResultReport>())).ReturnsAsync(new TestResultReport());

            var result = await _reportService.SaveOrUpdateReports(batchTestResults, 1000);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that getFileLocation returns a valid file path string for a single batch.
        /// </summary>
        [TestMethod]
        public void GetFileLocation_SingleBatch_ReturnsFilePath()
        {
            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    batch = new Batch { batchName = "24001" },
                    testResult = new MVVM.Models.TestResult { testType = "BCH", product = new Product { name = "P1" } }
                }
            };
            var result = _reportService.getFileLocation(batchTestResults);
            Assert.IsTrue(result.Contains("P1_24001") || result.Contains("P1-24001"));
        }

        /// <summary>
        /// Tests that CalculateTorDiffAndFusDiff calculates differences and sets result flag.
        /// </summary>
        [TestMethod]
        public void CalculateTorDiffAndFusDiff_ValidInput_UpdatesResults()
        {
            var product = new Product { torqueFail = 10, fusionFail = 10 };
            var std = new BatchTestResult
            {
                batch = new Batch { batchName = "STD1" },
                testResult = new MVVM.Models.TestResult { torque = 100, fusion = 100, product = product }
            };
            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    batch = new Batch { batchName = "24001" },
                    testResult = new MVVM.Models.TestResult { testType = "BCH", torque = 105, fusion = 110, product = product }
                }
            };
            var result = _reportService.CalculateTorDiffAndFusDiff(batchTestResults, std);
            Assert.AreEqual("STD1", result[0].standardReference);
            Assert.IsTrue(result[0].torqueDiff > 0);
            Assert.IsTrue(result[0].fusionDiff > 0);
            Assert.IsTrue(result[0].result);
        }

        /// <summary>
        /// Tests that CheckAndUpdateEvaluationResults updates evaluation if values differ.
        /// </summary>
        [TestMethod]
        public async Task CheckAndUpdateEvaluationResults_ValidInput_UpdatesEvaluations()
        {
            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    batch = new Batch { batchName = "24001" },
                    testResult = new MVVM.Models.TestResult { torque = 100, fusion = 200, torqueId = 1, fusionId = 2 }
                }
            };
            var torqueEval = new Evaluation { id = 1, torque = 90, testResult = new MVVM.Models.TestResult { id = 1 } };
            var fusionEval = new Evaluation { id = 2, timeEvalInt = 150, testResult = new MVVM.Models.TestResult { id = 1 } };
            _evaluationRestAPIMock.Setup(x => x.GetEvaluationByIdAsync(1)).ReturnsAsync(torqueEval);
            _evaluationRestAPIMock.Setup(x => x.GetEvaluationByIdAsync(2)).ReturnsAsync(fusionEval);
            _evaluationRestAPIMock.Setup(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>())).ReturnsAsync(new Evaluation());

            var result = await _reportService.CheckAndUpdateEvaluationResults(batchTestResults);

            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        /// Tests that DeleteTestResultReport returns the deleted report when confirmation is valid.
        /// </summary>
        [TestMethod]
        public async Task DeleteTestResultReport_ValidConfirmation_ReturnsDeletedReport()
        {
            var report = new TestResultReport { id = 1 };
            _testResultReportRestAPIMock.Setup(x => x.DeleteTestResultReportAsync(report)).ReturnsAsync(report);

            var result = await _reportService.DeleteTestResultReport(report, "DELETE");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that DeleteTestResultReport returns null when confirmation is invalid.
        /// </summary>
        [TestMethod]
        public async Task DeleteTestResultReport_InvalidConfirmation_ReturnsNull()
        {
            var report = new TestResultReport { id = 1 };

            var result = await _reportService.DeleteTestResultReport(report, "INVALID");

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange returns filtered reports by suffix.
        /// </summary>
        [TestMethod]
        public async Task GetProductTestResultReportsWithBatchRange_ValidInput_ReturnsFilteredReports()
        {
            var product = new Product { name = "P1" };
            var batch = new Batch { batchName = "24001", suffix = "A" };
            var batchTestResult = new BatchTestResult { batch = batch };
            var report = new TestResultReport { batchTestResult = batchTestResult };
            var reports = new List<TestResultReport> { report };

            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(reports);

            var result = await _reportService.GetProductTestResultReportsWithBatchRange(product, "24001", "24001", "1", "A");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("A", result[0].batchTestResult.batch.suffix);
        }
    }
}

