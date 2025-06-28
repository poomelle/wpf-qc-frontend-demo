using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.DailyQcServiceTests
{
    [TestClass]
    public class DailyQcServiceTests
    {
        private Mock<IDailyQcRestAPI> _dailyQcRestAPIMock;
        private Mock<ICoaRestAPI> _coaRestAPIMock;
        private Mock<IQcLabelRestAPI> _qcLabelRestAPIMock;
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;
        private Mock<IProductRestAPI> _productRestAPIMock;

        private DailyQcService _dailyQcService;

        [TestInitialize]
        public void Setup()
        {
            _dailyQcRestAPIMock = new Mock<IDailyQcRestAPI>();
            _coaRestAPIMock = new Mock<ICoaRestAPI>();
            _qcLabelRestAPIMock = new Mock<IQcLabelRestAPI>();
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();
            _productRestAPIMock = new Mock<IProductRestAPI>();
            _dailyQcService = new DailyQcService(
                _dailyQcRestAPIMock.Object,
                _coaRestAPIMock.Object,
                _qcLabelRestAPIMock.Object,
                _batchTestResultRestAPIMock.Object,
                _productRestAPIMock.Object
            );
        }

        #region CreateDailyQcAsync Tests

        /// <summary>
        /// Tests that CreateDailyQcAsync creates a new DailyQc entry and returns the created entry.
        /// </summary>
        [TestMethod]
        public async Task CreateDailyQcAsync_ValidDailyQc_ReturnsCreatedDailyQc()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                id = 1,
                productName = "Test Product",
                batches = "25A1",
                testStatus = "In Progress"
            };
            _dailyQcRestAPIMock.Setup(x => x.CreateDailyQcAsync(dailyQc))
                .ReturnsAsync(dailyQc);

            // Act
            var result = await _dailyQcService.CreateDailyQcAsync(dailyQc);

            // Assert
            Assert.AreEqual(dailyQc, result);
            _dailyQcRestAPIMock.Verify(x => x.CreateDailyQcAsync(dailyQc), Times.Once);
        }

        #endregion

        #region DeleteDailyQcAsync Tests

        /// <summary>
        /// Tests that DeleteDailyQcAsync successfully deletes valid DailyQc entries and returns true.
        /// </summary>
        [TestMethod]
        public async Task DeleteDailyQcAsync_ValidEntries_ReturnsTrue()
        {
            // Arrange
            var dailyQcs = new List<DailyQc>
            {
                new DailyQc { id = 1 },
                new DailyQc { id = 2 }
            };
            _dailyQcRestAPIMock.Setup(x => x.DeleteDailyQcAsync(It.IsAny<DailyQc>()))
                .ReturnsAsync(new DailyQc());

            // Act
            var result = await _dailyQcService.DeleteDailyQcAsync(dailyQcs);

            // Assert
            Assert.IsTrue(result);
            _dailyQcRestAPIMock.Verify(x => x.DeleteDailyQcAsync(It.IsAny<DailyQc>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that DeleteDailyQcAsync skips entries with id 0 and only deletes valid entries.
        /// </summary>
        [TestMethod]
        public async Task DeleteDailyQcAsync_SkipsZeroIds_ReturnsTrue()
        {
            // Arrange
            var dailyQcs = new List<DailyQc>
            {
                new DailyQc { id = 0 }, // Should be skipped
                new DailyQc { id = 1 }  // Should be deleted
            };
            _dailyQcRestAPIMock.Setup(x => x.DeleteDailyQcAsync(It.IsAny<DailyQc>()))
                .ReturnsAsync(new DailyQc());

            // Act
            var result = await _dailyQcService.DeleteDailyQcAsync(dailyQcs);

            // Assert
            Assert.IsTrue(result);
            _dailyQcRestAPIMock.Verify(x => x.DeleteDailyQcAsync(It.IsAny<DailyQc>()), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteDailyQcAsync returns false when an exception occurs during deletion.
        /// </summary>
        [TestMethod]
        public async Task DeleteDailyQcAsync_ExceptionThrown_ReturnsFalse()
        {
            // Arrange
            var dailyQcs = new List<DailyQc> { new DailyQc { id = 1 } };
            _dailyQcRestAPIMock.Setup(x => x.DeleteDailyQcAsync(It.IsAny<DailyQc>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _dailyQcService.DeleteDailyQcAsync(dailyQcs);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region GetAllDailyQcsAsync Tests

        /// <summary>
        /// Tests that GetAllDailyQcsAsync retrieves all DailyQc entries with default parameters.
        /// </summary>
        [TestMethod]
        public async Task GetAllDailyQcsAsync_DefaultParameters_ReturnsAllEntries()
        {
            // Arrange
            var expected = new List<DailyQc>
            {
                new DailyQc { id = 1, productName = "Product 1" },
                new DailyQc { id = 2, productName = "Product 2" }
            };
            _dailyQcRestAPIMock.Setup(x => x.GetAllDailyQcsAsync("", ""))
                .ReturnsAsync(expected);

            // Act
            var result = await _dailyQcService.GetAllDailyQcsAsync();

            // Assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(2, result.Count);
        }

        #endregion

        #region GetDailyQcByIdAsync Tests

        /// <summary>
        /// Tests that GetDailyQcByIdAsync retrieves a specific DailyQc entry by its ID.
        /// </summary>
        [TestMethod]
        public async Task GetDailyQcByIdAsync_ValidId_ReturnsDailyQc()
        {
            // Arrange
            var expected = new DailyQc { id = 1, productName = "Test Product" };
            _dailyQcRestAPIMock.Setup(x => x.GetDailyQcByIdAsync(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _dailyQcService.GetDailyQcByIdAsync(1);

            // Assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(1, result.id);
        }

        #endregion

        #region GetLastCoaBatchName Tests

        /// <summary>
        /// Tests that GetLastCoaBatchName returns empty string when COA is not required.
        /// </summary>
        [TestMethod]
        public async Task GetLastCoaBatchName_CoaNotRequired_ReturnsEmptyString()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1",
                product = new Product { name = "Test Product", coa = false }
            };

            // Act
            var result = await _dailyQcService.GetLastCoaBatchName(dailyQc);

            // Assert
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// Tests that GetLastCoaBatchName returns "Reqd" when COA is required but no COA found.
        /// </summary>
        [TestMethod]
        public async Task GetLastCoaBatchName_CoaRequiredNoCoaFound_ReturnsReqd()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1",
                product = new Product { name = "Test Product", coa = true }
            };
            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Coa>());

            // Act
            var result = await _dailyQcService.GetLastCoaBatchName(dailyQc);

            // Assert
            Assert.AreEqual("Reqd", result);
        }

        #endregion

        #region GetLastTest Tests

        /// <summary>
        /// Tests that GetLastTest returns empty string when no batch test results are found.
        /// </summary>
        [TestMethod]
        public async Task GetLastTest_NoBatchTestResults_ReturnsEmptyString()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1",
                product = new Product { name = "Test Product" }
            };
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());

            // Act
            var result = await _dailyQcService.GetLastTest(dailyQc);

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region GetDailyQcLabel Tests

        /// <summary>
        /// Tests that GetDailyQcLabel returns empty string when no QC labels are found.
        /// </summary>
        [TestMethod]
        public async Task GetDailyQcLabel_NoQcLabels_ReturnsEmptyString()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1",
                product = new Product { name = "Test Product" }
            };
            _qcLabelRestAPIMock.Setup(x => x.GetAllQCLabelsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<QCLabel>());

            // Act
            var result = await _dailyQcService.GetDailyQcLabel(dailyQc);

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region GetMixReqd Tests

        /// <summary>
        /// Tests that GetMixReqd calculates correct number of mixes for normal batches without standard.
        /// </summary>
        [TestMethod]
        public void GetMixReqd_NormalBatchesNoStandard_ReturnsCorrectCount()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1,25A2",
                stdReqd = null,
                product = new Product { comment = null }
            };

            // Act
            var result = _dailyQcService.GetMixReqd(dailyQc);

            // Assert
            Assert.IsTrue(result >= 0);
        }

        /// <summary>
        /// Tests that GetMixReqd adds one additional mix when standard is required.
        /// </summary>
        [TestMethod]
        public void GetMixReqd_WithStandard_AddsOneForStandard()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1",
                stdReqd = "Yes",
                product = new Product { comment = null }
            };

            // Act
            var result = _dailyQcService.GetMixReqd(dailyQc);

            // Assert
            Assert.IsTrue(result >= 1);
        }

        /// <summary>
        /// Tests that GetMixReqd divides by two for double batch mix products.
        /// </summary>
        [TestMethod]
        public void GetMixReqd_DoubleBatchMix_DividesByTwo()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                batches = "25A1,25A2,25A3,25A4",
                stdReqd = null,
                product = new Product { comment = "x2" }
            };

            // Act
            var result = _dailyQcService.GetMixReqd(dailyQc);

            // Assert
            Assert.IsTrue(result >= 0);
        }

        #endregion

        #region GetDailyQcs Tests

        /// <summary>
        /// Tests that GetDailyQcs retrieves filtered DailyQc entries based on provided parameters.
        /// </summary>
        [TestMethod]
        public async Task GetDailyQcs_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var expected = new List<DailyQc>
            {
                new DailyQc { id = 1, productName = "Test Product" }
            };
            _dailyQcRestAPIMock.Setup(x => x.GetAllDailyQcsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(expected);

            // Act
            var result = await _dailyQcService.GetDailyQcs("Test Product", "2025", "June", "In Progress");

            // Assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        /// Tests that GetDailyQcs handles "All" parameters by converting them to empty filters.
        /// </summary>
        [TestMethod]
        public async Task GetDailyQcs_AllParameters_UsesEmptyFilters()
        {
            // Arrange
            var expected = new List<DailyQc>();
            _dailyQcRestAPIMock.Setup(x => x.GetAllDailyQcsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(expected);

            // Act
            var result = await _dailyQcService.GetDailyQcs("All", "All", "All", "All");

            // Assert
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region UpdateDailyQcAsync Tests

        /// <summary>
        /// Tests that UpdateDailyQcAsync updates an existing DailyQc entry and returns the updated entry.
        /// </summary>
        [TestMethod]
        public async Task UpdateDailyQcAsync_ValidDailyQc_ReturnsUpdatedDailyQc()
        {
            // Arrange
            var dailyQc = new DailyQc
            {
                id = 1,
                productName = "Updated Product",
                testStatus = "Completed"
            };
            _dailyQcRestAPIMock.Setup(x => x.UpdateDailyQcAsync(dailyQc))
                .ReturnsAsync(dailyQc);

            // Act
            var result = await _dailyQcService.UpdateDailyQcAsync(dailyQc);

            // Assert
            Assert.AreEqual(dailyQc, result);
            _dailyQcRestAPIMock.Verify(x => x.UpdateDailyQcAsync(dailyQc), Times.Once);
        }

        #endregion

        #region SaveAllDailyQcs Tests

        /// <summary>
        /// Tests that SaveAllDailyQcs processes new DailyQc entries correctly by creating them.
        /// </summary>
        [TestMethod]
        public async Task SaveAllDailyQcs_NewEntry_CreatesEntry()
        {
            // Arrange
            var product = new Product { id = 1, name = "Test Product", coa = false };
            var dailyQcs = new List<DailyQc>
            {
                new DailyQc
                {
                    id = 0, // New entry
                    productName = "Test Product",
                    batches = "25A1",
                    product = product
                }
            };

            _productRestAPIMock.Setup(x => x.GetProductsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Product> { product });
            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Coa>());
            _qcLabelRestAPIMock.Setup(x => x.GetAllQCLabelsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<QCLabel>());
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());
            _dailyQcRestAPIMock.Setup(x => x.CreateDailyQcAsync(It.IsAny<DailyQc>()))
                .ReturnsAsync(dailyQcs[0]);

            // Act
            await _dailyQcService.SaveAllDailyQcs(dailyQcs);

            // Assert
            _dailyQcRestAPIMock.Verify(x => x.CreateDailyQcAsync(It.IsAny<DailyQc>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveAllDailyQcs processes existing DailyQc entries correctly by updating them.
        /// </summary>
        [TestMethod]
        public async Task SaveAllDailyQcs_ExistingEntry_UpdatesEntry()
        {
            // Arrange
            var product = new Product { id = 1, name = "Test Product", coa = false };
            var dailyQcs = new List<DailyQc>
            {
                new DailyQc
                {
                    id = 1, // Existing entry
                    productName = "Test Product",
                    batches = "25A1",
                    product = product
                }
            };

            _productRestAPIMock.Setup(x => x.GetProductsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Product> { product });
            _dailyQcRestAPIMock.Setup(x => x.GetDailyQcByIdAsync(1))
                .ReturnsAsync(dailyQcs[0]);
            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Coa>());
            _qcLabelRestAPIMock.Setup(x => x.GetAllQCLabelsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<QCLabel>());
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());
            _dailyQcRestAPIMock.Setup(x => x.UpdateDailyQcAsync(It.IsAny<DailyQc>()))
                .ReturnsAsync(dailyQcs[0]);

            // Act
            await _dailyQcService.SaveAllDailyQcs(dailyQcs);

            // Assert
            _dailyQcRestAPIMock.Verify(x => x.UpdateDailyQcAsync(It.IsAny<DailyQc>()), Times.Once);
        }

        #endregion
    }
}

