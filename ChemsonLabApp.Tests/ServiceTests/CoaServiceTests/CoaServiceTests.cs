using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.COAService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests
{
    /// <summary>
    /// Unit tests for ICoaService interface methods implemented by CoaService.
    /// Tests all three public methods: CreateCOAFromTestResultReportAsync, GetProductTestResultReportsWithBatchRange, and SortTestReportsByBatchNumber.
    /// Uses real model classes from ChemsonLabApp.MVVM.Models namespace.
    /// </summary>
    [TestClass]
    public class CoaServiceTests
    {
        // Mock the dependencies, not the service itself
        private Mock<ICoaRestAPI> _coaRestApiMock;
        private Mock<ITestResultReportRestAPI> _testResultReportRestApiMock;

        // Use the real service implementation
        private ICoaService _coaService;

        /// <summary>
        /// Initializes mock objects and creates the CoaService instance for testing.
        /// Sets up the test environment before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _coaRestApiMock = new Mock<ICoaRestAPI>();
            _testResultReportRestApiMock = new Mock<ITestResultReportRestAPI>();
            _coaService = new CoaService(_coaRestApiMock.Object, _testResultReportRestApiMock.Object);
        }

        #region CreateCOAFromTestResultReportAsync Tests

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync creates a new COA when none exists for the given product and batch.
        /// Verifies that the API is called to check for existing COAs and to create a new one with correct properties.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ValidTestResultReports_CreatesCOAWhenNoneExists()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("TestProduct", "25A001", 1)
            };

            _coaRestApiMock.Setup(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""))
                          .ReturnsAsync(new List<Coa>());

            _coaRestApiMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act
            await _coaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.CreateCoaAsync(It.Is<Coa>(c => c.productId == 1 && c.batchName == "25A001")), Times.Once);
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync does not create a COA when one already exists for the product and batch.
        /// Verifies that the API is called to check for existing COAs but not to create a new one.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ValidTestResultReports_DoesNotCreateCOAWhenAlreadyExists()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("TestProduct", "25A001", 1)
            };

            var existingCoas = new List<Coa>
            {
                new Coa { id = 1, productId = 1, batchName = "25A001" }
            };

            _coaRestApiMock.Setup(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""))
                          .ReturnsAsync(existingCoas);

            // Act
            await _coaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Never);
        }


        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync processes multiple test result reports correctly.
        /// Verifies that separate API calls are made for each unique product-batch combination and COAs are created for each.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_MultipleTestResultReports_ProcessesAllReports()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("Product1", "25A001", 1),
                CreateTestResultReport("Product2", "25A002", 2)
            };

            _coaRestApiMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                          .ReturnsAsync(new List<Coa>());

            _coaRestApiMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act
            await _coaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=Product1&batchName=25A001", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=Product2&batchName=25A002", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync continues processing subsequent reports when an exception occurs with one report.
        /// Verifies that the exception is caught and logged, and remaining reports are still processed.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ExceptionInProcessing_ContinuesWithNextReport()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("Product1", "25A001", 1),
                CreateTestResultReport("Product2", "25A002", 2)
            };

            _coaRestApiMock.SetupSequence(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                          .ThrowsAsync(new Exception("API Error"))
                          .ReturnsAsync(new List<Coa>());

            _coaRestApiMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act
            await _coaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=Product1&batchName=25A001", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.GetAllCoasAsync("?productName=Product2&batchName=25A002", ""), Times.Once);
            _coaRestApiMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Once);
        }

        #endregion

        #region GetProductTestResultReportsWithBatchRange Tests

        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange uses standard batch filter for short batch names (3 characters).
        /// Verifies that the correct API call is made with the batchName parameter.
        /// </summary>
        [TestMethod]
        public async Task GetProductTestResultReportsWithBatchRange_ShortBatchName_UsesStandardBatchFilter()
        {
            // Arrange
            var product = new Product { id = 1, name = "TestProduct" };
            var fromBatch = "25A";
            var toBatch = "25B";
            var expectedReports = new List<TestResultReport> { CreateTestResultReport("TestProduct", "25A", 1) };

            _testResultReportRestApiMock.Setup(x => x.GetAllTestResultReportAsync("?productName=TestProduct&batchName=25A", ""))
                                       .ReturnsAsync(expectedReports);

            // Act
            var result = await _coaService.GetProductTestResultReportsWithBatchRange(product, fromBatch, toBatch);

            // Assert
            Assert.AreEqual(1, result.Count);
            _testResultReportRestApiMock.Verify(x => x.GetAllTestResultReportAsync("?productName=TestProduct&batchName=25A", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange uses exact batch filter when fromBatch and toBatch are identical.
        /// Verifies that the correct API call is made with the exactBatchName parameter.
        /// </summary>
        [TestMethod]
        public async Task GetProductTestResultReportsWithBatchRange_ExactBatchName_SameBatches_UsesExactBatchFilter()
        {
            // Arrange
            var product = new Product { id = 1, name = "TestProduct" };
            var fromBatch = "25A001";
            var toBatch = "25A001";
            var expectedReports = new List<TestResultReport> { CreateTestResultReport("TestProduct", "25A001", 1) };

            _testResultReportRestApiMock.Setup(x => x.GetAllTestResultReportAsync("?productName=TestProduct&exactBatchName=25A001", ""))
                                       .ReturnsAsync(expectedReports);

            // Act
            var result = await _coaService.GetProductTestResultReportsWithBatchRange(product, fromBatch, toBatch);

            // Assert
            Assert.AreEqual(1, result.Count);
            _testResultReportRestApiMock.Verify(x => x.GetAllTestResultReportAsync("?productName=TestProduct&exactBatchName=25A001", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange uses exact batch filter when toBatch is empty or null.
        /// Verifies that the correct API call is made with the exactBatchName parameter for single batch queries.
        /// </summary>
        [TestMethod]
        public async Task GetProductTestResultReportsWithBatchRange_ExactBatchName_EmptyToBatch_UsesExactBatchFilter()
        {
            // Arrange
            var product = new Product { id = 1, name = "TestProduct" };
            var fromBatch = "25A001";
            var toBatch = "";
            var expectedReports = new List<TestResultReport> { CreateTestResultReport("TestProduct", "25A001", 1) };

            _testResultReportRestApiMock.Setup(x => x.GetAllTestResultReportAsync("?productName=TestProduct&exactBatchName=25A001", ""))
                                       .ReturnsAsync(expectedReports);

            // Act
            var result = await _coaService.GetProductTestResultReportsWithBatchRange(product, fromBatch, toBatch);

            // Assert
            Assert.AreEqual(1, result.Count);
            _testResultReportRestApiMock.Verify(x => x.GetAllTestResultReportAsync("?productName=TestProduct&exactBatchName=25A001", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange handles general exceptions gracefully and returns an empty list.
        /// Verifies that any unexpected errors are caught and logged without throwing exceptions to the caller.
        /// </summary>
        [TestMethod]
        public async Task GetProductTestResultReportsWithBatchRange_GeneralException_ReturnsEmptyList()
        {
            // Arrange
            var product = new Product { id = 1, name = "TestProduct" };
            var fromBatch = "25A";
            var toBatch = "25B";

            _testResultReportRestApiMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                                       .ThrowsAsync(new Exception("General error"));

            // Act
            var result = await _coaService.GetProductTestResultReportsWithBatchRange(product, fromBatch, toBatch);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region SortTestReportsByBatchNumber Tests

        /// <summary>
        /// Tests that SortTestReportsByBatchNumber correctly sorts test reports by their batch number extracted from batch names.
        /// Verifies that batch numbers are parsed correctly and the reports are ordered in ascending order.
        /// </summary>
        [TestMethod]
        public void SortTestReportsByBatchNumber_ValidBatchNames_SortsCorrectly()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReportWithBatch("25A003"),
                CreateTestResultReportWithBatch("25A001"),
                CreateTestResultReportWithBatch("25A002")
            };

            // Act
            var result = _coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("25A001", result[0].batchTestResult.batch.batchName);
            Assert.AreEqual("25A002", result[1].batchTestResult.batch.batchName);
            Assert.AreEqual("25A003", result[2].batchTestResult.batch.batchName);
            Assert.AreEqual(1, result[0].batchNumber);
            Assert.AreEqual(2, result[1].batchNumber);
            Assert.AreEqual(3, result[2].batchNumber);
        }

        /// <summary>
        /// Tests that SortTestReportsByBatchNumber handles invalid batch names gracefully without setting batch numbers.
        /// Verifies that reports with unparseable batch names have null batch numbers while valid ones are parsed correctly.
        /// </summary>
        [TestMethod]
        public void SortTestReportsByBatchNumber_InvalidBatchNames_DoesNotSetBatchNumber()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReportWithBatch("INVALID"),
                CreateTestResultReportWithBatch("25A001")
            };

            // Act
            var result = _coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsNull(result.First(r => r.batchTestResult.batch.batchName == "INVALID").batchNumber);
            Assert.AreEqual(1, result.First(r => r.batchTestResult.batch.batchName == "25A001").batchNumber);
        }

        /// <summary>
        /// Tests that SortTestReportsByBatchNumber handles empty input list and returns an empty list.
        /// Verifies that the method does not throw exceptions when given no data to process.
        /// </summary>
        [TestMethod]
        public void SortTestReportsByBatchNumber_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>();

            // Act
            var result = _coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// Tests that SortTestReportsByBatchNumber correctly handles a mix of valid and invalid batch names.
        /// Verifies that valid batch numbers are sorted correctly while invalid ones remain with null batch numbers.
        /// </summary>
        [TestMethod]
        public void SortTestReportsByBatchNumber_MixedValidInvalidBatchNames_SortsValidOnesCorrectly()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReportWithBatch("25A010"),
                CreateTestResultReportWithBatch("INVALID"),
                CreateTestResultReportWithBatch("25A005"),
                CreateTestResultReportWithBatch("BADNAME")
            };

            // Act
            var result = _coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual(4, result.Count);

            // Valid batch numbers should be sorted first
            var validBatches = result.Where(r => r.batchNumber.HasValue).ToList();
            Assert.AreEqual(2, validBatches.Count);
            Assert.AreEqual(5, validBatches[0].batchNumber);
            Assert.AreEqual(10, validBatches[1].batchNumber);

            // Invalid batches should have null batch numbers
            var invalidBatches = result.Where(r => !r.batchNumber.HasValue).ToList();
            Assert.AreEqual(2, invalidBatches.Count);
        }

        /// <summary>
        /// Tests that SortTestReportsByBatchNumber correctly sorts reports with large batch numbers.
        /// Verifies that numeric sorting works correctly for three-digit batch numbers.
        /// </summary>
        [TestMethod]
        public void SortTestReportsByBatchNumber_LargeBatchNumbers_SortsCorrectly()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReportWithBatch("25A999"),
                CreateTestResultReportWithBatch("25A100"),
                CreateTestResultReportWithBatch("25A050")
            };

            // Act
            var result = _coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(50, result[0].batchNumber);
            Assert.AreEqual(100, result[1].batchNumber);
            Assert.AreEqual(999, result[2].batchNumber);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test TestResultReport with the specified product name, batch name, and product ID.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="productName">The name of the product for the test report.</param>
        /// <param name="batchName">The batch name following the 25A format.</param>
        /// <param name="productId">The unique identifier for the product.</param>
        /// <returns>A properly configured TestResultReport for testing.</returns>
        private TestResultReport CreateTestResultReport(string productName, string batchName, int productId)
        {
            return new TestResultReport
            {
                id = 1,
                report = new Report
                {
                    id = 1,
                    createBy = "TestUser",
                    createDate = DateTime.Now,
                    status = true
                },
                reportId = 1,
                batchTestResult = new BatchTestResult
                {
                    id = 1,
                    batch = new Batch
                    {
                        id = 1,
                        batchName = batchName,
                        sampleBy = "TestSampler",
                        productId = productId,
                        suffix = "A",
                        batchNumber = 1
                    },
                    testResult = new MVVM.Models.TestResult
                    {
                        id = 1,
                        product = new Product
                        {
                            id = productId,
                            name = productName,
                            status = true,
                            torqueWarning = 5.0,
                            torqueFail = 10.0,
                            fusionWarning = 5.0,
                            fusionFail = 10.0
                        },
                        productId = productId,
                        testDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        operatorName = "TestOperator",
                        testNumber = 1,
                        testType = "Standard",
                        batchGroup = "25A",
                        testMethod = "ASTM",
                        status = true,
                        fileName = "test_file.txt"
                    },
                    batchId = 1,
                    testResultId = 1,
                    torqueDiff = 2.5,
                    fusionDiff = 3.0,
                    standardReference = "STD001",
                    result = true,
                    testDate = DateTime.Now
                },
                batchTestResultId = 1,
                productSpecification = new Specification
                {
                    id = 1,
                    productId = productId,
                    machineId = 1,
                    inUse = true,
                    temp = 190,
                    load = 50,
                    rpm = 60
                },
                productSpecificationId = 1,
                standardReference = "STD001",
                torqueDiff = 2.5,
                fusionDiff = 3.0,
                result = true,
                aveTestTime = 300,
                fileLocation = @"C:\Tests\test_file.txt"
            };
        }

        /// <summary>
        /// Creates a test TestResultReport with the specified batch name for sorting tests.
        /// Used for testing the batch number sorting functionality with minimal test data setup using real model classes.
        /// </summary>
        /// <param name="batchName">The batch name following the 25A format.</param>
        /// <returns>A TestResultReport configured for batch sorting tests.</returns>
        private TestResultReport CreateTestResultReportWithBatch(string batchName)
        {
            return new TestResultReport
            {
                id = 1,
                batchTestResult = new BatchTestResult
                {
                    id = 1,
                    batch = new Batch
                    {
                        id = 1,
                        batchName = batchName,
                        sampleBy = "TestSampler",
                        productId = 1,
                        suffix = "A"
                    },
                    testResult = new MVVM.Models.TestResult
                    {
                        id = 1,
                        product = new Product
                        {
                            id = 1,
                            name = "TestProduct",
                            status = true
                        },
                        productId = 1,
                        testNumber = 1,
                        status = true
                    },
                    batchId = 1,
                    testResultId = 1,
                    testDate = DateTime.Now
                }
            };
        }

        #endregion
    }
}


