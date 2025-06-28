using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.DataLoaderServiceTests
{
    [TestClass]
    public class SerchDataLoaderServiceTests
    {
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;
        private SearchDataLoaderService _searchDataLoaderService;

        [TestInitialize]
        public void Setup()
        {
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();
            _searchDataLoaderService = new SearchDataLoaderService(_batchTestResultRestAPIMock.Object);
        }

        #region LoadBCHBatchTestResult Tests

        /// <summary>
        /// Tests that LoadBCHBatchTestResult loads batch test results for a single batch when only fromBatchName is provided.
        /// Verifies that the method calls the API with the correct filter and returns filtered results by suffix.
        /// </summary>
        [TestMethod]
        public async Task LoadBCHBatchTestResult_SingleBatch_ReturnsFilteredResults()
        {
            // Arrange
            var productName = "TestProduct";
            var testNumber = "1";
            var suffix = "";
            var fromBatchName = "25A1";
            var toBatchName = "";
            var testDate = DateTime.Now;

            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    batch = new Batch { batchName = "25A1", suffix = "" },
                    testResult = new MVVM.Models.TestResult { product = new Product { name = productName } }
                },
                new BatchTestResult
                {
                    id = 2,
                    batch = new Batch { batchName = "25A1", suffix = "Cal" },
                    testResult = new MVVM.Models.TestResult { product = new Product { name = productName } }
                }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&exactBatchName={fromBatchName}&testNumber={testNumber}", ""))
                .ReturnsAsync(batchTestResults);

            // Act
            var result = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, fromBatchName, toBatchName, testDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].batch.suffix);
        }

        /// <summary>
        /// Tests that LoadBCHBatchTestResult loads batch test results for a range of batches when both fromBatchName and toBatchName are provided.
        /// Verifies that the method generates the correct batch range and calls the API for each batch.
        /// </summary>
        [TestMethod]
        public async Task LoadBCHBatchTestResult_BatchRange_ReturnsResultsForRange()
        {
            // Arrange
            var productName = "TestProduct";
            var testNumber = "1";
            var suffix = "";
            var fromBatchName = "25A1";
            var toBatchName = "25A3";
            var testDate = DateTime.Now;

            var batchTestResult1 = new List<BatchTestResult>
            {
                new BatchTestResult { id = 1, batch = new Batch { batchName = "25A1", suffix = "" } }
            };
            var batchTestResult2 = new List<BatchTestResult>
            {
                new BatchTestResult { id = 2, batch = new Batch { batchName = "25A2", suffix = "" } }
            };
            var batchTestResult3 = new List<BatchTestResult>
            {
                new BatchTestResult { id = 3, batch = new Batch { batchName = "25A3", suffix = "" } }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&exactBatchName=25A1&testNumber={testNumber}", ""))
                .ReturnsAsync(batchTestResult1);
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&exactBatchName=25A2&testNumber={testNumber}", ""))
                .ReturnsAsync(batchTestResult2);
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&exactBatchName=25A3&testNumber={testNumber}", ""))
                .ReturnsAsync(batchTestResult3);

            // Act
            var result = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, fromBatchName, toBatchName, testDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            _batchTestResultRestAPIMock.Verify(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""), Times.Exactly(3));
        }

        /// <summary>
        /// Tests that LoadBCHBatchTestResult loads batch test results by test date when no batch names are provided.
        /// Verifies that the method uses test date filter when batch name parameters are empty.
        /// </summary>
        [TestMethod]
        public async Task LoadBCHBatchTestResult_ByTestDate_ReturnsResultsByDate()
        {
            // Arrange
            var productName = "TestProduct";
            var testNumber = "1";
            var suffix = "";
            var fromBatchName = "";
            var toBatchName = "";
            var testDate = new DateTime(2025, 6, 28);

            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    batch = new Batch { batchName = "25A1", suffix = "" },
                    testResult = new MVVM.Models.TestResult { testDate = "28/06/2025" }
                }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&testDate=2025-06-28", ""))
                .ReturnsAsync(batchTestResults);

            // Act
            var result = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, fromBatchName, toBatchName, testDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _batchTestResultRestAPIMock.Verify(x => x.GetAllBatchTestResultsAsync($"?productName={productName}&testDate=2025-06-28", ""), Times.Once);
        }

        /// <summary>
        /// Tests that LoadBCHBatchTestResult filters results by suffix correctly.
        /// Verifies that only batch test results with matching suffix are returned.
        /// </summary>
        [TestMethod]
        public async Task LoadBCHBatchTestResult_WithSuffix_FiltersResultsBySuffix()
        {
            // Arrange
            var productName = "TestProduct";
            var testNumber = "1";
            var suffix = "Cal";
            var fromBatchName = "25A1";
            var toBatchName = "";
            var testDate = DateTime.Now;

            var batchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    batch = new Batch { batchName = "25A1", suffix = "" }
                },
                new BatchTestResult
                {
                    id = 2,
                    batch = new Batch { batchName = "25A1", suffix = "Cal" }
                }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(batchTestResults);

            // Act
            var result = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, fromBatchName, toBatchName, testDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Cal", result[0].batch.suffix);
        }

        /// <summary>
        /// Tests that LoadBCHBatchTestResult returns empty list when no results are found.
        /// Verifies that the method handles empty results gracefully.
        /// </summary>
        [TestMethod]
        public async Task LoadBCHBatchTestResult_NoResults_ReturnsEmptyList()
        {
            // Arrange
            var productName = "TestProduct";
            var testNumber = "1";
            var suffix = "";
            var fromBatchName = "25A1";
            var toBatchName = "";
            var testDate = DateTime.Now;

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());

            // Act
            var result = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, fromBatchName, toBatchName, testDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region LoadWarmUpAndSTDBatchTestResult Tests

        /// <summary>
        /// Tests that LoadWarmUpAndSTDBatchTestResult loads WarmUp batch test results using product name and batch group from BCH results.
        /// Verifies that the method constructs the correct filter for WarmUp test type.
        /// </summary>
        [TestMethod]
        public async Task LoadWarmUpAndSTDBatchTestResult_WarmUpTestType_ReturnsWarmUpResults()
        {
            // Arrange
            var product = new Product { name = "TestProduct" };
            var bchBatchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    testResult = new MVVM.Models.TestResult
                    {
                        product = product,
                        batchGroup = "25A"
                    }
                }
            };
            var testType = "W/U";
            var expectedResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 2,
                    batch = new Batch { batchName = "W/U" }
                }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName=TestProduct&batchGroup=25A&exactBatchName=W/U", ""))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _searchDataLoaderService.LoadWarmUpAndSTDBatchTestResult(bchBatchTestResults, testType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("W/U", result[0].batch.batchName);
        }

        /// <summary>
        /// Tests that LoadWarmUpAndSTDBatchTestResult loads STD batch test results using product name and batch group from BCH results.
        /// Verifies that the method constructs the correct filter for STD test type with partial batch name matching.
        /// </summary>
        [TestMethod]
        public async Task LoadWarmUpAndSTDBatchTestResult_STDTestType_ReturnsSTDResults()
        {
            // Arrange
            var product = new Product { name = "TestProduct" };
            var bchBatchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    testResult = new MVVM.Models.TestResult
                    {
                        product = product,
                        batchGroup = "25A"
                    }
                }
            };
            var testType = "STD";
            var expectedResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 2,
                    batch = new Batch { batchName = "STD1" }
                },
                new BatchTestResult
                {
                    id = 3,
                    batch = new Batch { batchName = "STD2" }
                }
            };

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync($"?productName=TestProduct&batchGroup=25A&batchName=STD", ""))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _searchDataLoaderService.LoadWarmUpAndSTDBatchTestResult(bchBatchTestResults, testType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.batch.batchName.StartsWith("STD")));
        }

        /// <summary>
        /// Tests that LoadWarmUpAndSTDBatchTestResult handles empty BCH batch test results gracefully.
        /// Verifies that the method can handle null/empty input collections.
        /// </summary>
        [TestMethod]
        public async Task LoadWarmUpAndSTDBatchTestResult_EmptyBCHResults_HandlesGracefully()
        {
            // Arrange
            var product = new Product { name = "TestProduct" };
            var bchBatchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult
                {
                    id = 1,
                    testResult = new MVVM.Models.TestResult
                    {
                        product = product,
                        batchGroup = "25A"
                    }
                }
            };
            var testType = "W/U";

            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());

            // Act
            var result = await _searchDataLoaderService.LoadWarmUpAndSTDBatchTestResult(bchBatchTestResults, testType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion
    }
}


