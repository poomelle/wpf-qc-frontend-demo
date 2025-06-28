using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.QcLabelServiceTests
{
    [TestClass]
    public class QcLabelServiceTests
    {
        private Mock<IQcLabelRestAPI> _qcLabelRestAPIMock;
        private QcLabelService _qcLabelService;

        [TestInitialize]
        public void Setup()
        {
            _qcLabelRestAPIMock = new Mock<IQcLabelRestAPI>();
            _qcLabelService = new QcLabelService(_qcLabelRestAPIMock.Object);
        }

        /// <summary>
        /// Tests that CreateQcLabelAsync calls the REST API and returns the created QCLabel.
        /// </summary>
        [TestMethod]
        public async Task CreateQcLabelAsync_ValidLabel_ReturnsCreatedLabel()
        {
            var label = new QCLabel { id = 1, batchName = "24001" };
            _qcLabelRestAPIMock.Setup(x => x.CreateQCLabelAsync(label)).ReturnsAsync(label);

            var result = await _qcLabelService.CreateQcLabelAsync(label);

            Assert.IsNotNull(result);
            Assert.AreEqual(label.id, result.id);
        }

        /// <summary>
        /// Tests that CreateQcLabelFromListAsync returns true when no exception occurs.
        /// </summary>
        [TestMethod]
        public async Task CreateQcLabelFromListAsync_ValidList_ReturnsTrue()
        {
            var product = new Product { id = 1, name = "P1" };
            var label = new QCLabel { batchName = "24001", product = product, productId = 1 };
            var list = new List<QCLabel> { label };

            _qcLabelRestAPIMock.Setup(x => x.GetAllQCLabelsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<QCLabel>());
            _qcLabelRestAPIMock.Setup(x => x.CreateQCLabelAsync(It.IsAny<QCLabel>()))
                .ReturnsAsync(label);

            var result = await _qcLabelService.CreateQcLabelFromListAsync(list);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that DeleteQcLabelAsync calls the REST API and returns the deleted QCLabel.
        /// </summary>
        [TestMethod]
        public async Task DeleteQcLabelAsync_ValidLabel_ReturnsDeletedLabel()
        {
            var label = new QCLabel { id = 1, batchName = "24001" };
            _qcLabelRestAPIMock.Setup(x => x.DeleteQCLabelAsync(label)).ReturnsAsync(label);

            var result = await _qcLabelService.DeleteQcLabelAsync(label);

            Assert.IsNotNull(result);
            Assert.AreEqual(label.id, result.id);
        }

        /// <summary>
        /// Tests that GetAllQcLabelsAsync calls the REST API and returns all QCLabels.
        /// </summary>
        [TestMethod]
        public async Task GetAllQcLabelsAsync_ReturnsAllLabels()
        {
            var labels = new List<QCLabel>
            {
                new QCLabel { id = 1, batchName = "24001" },
                new QCLabel { id = 2, batchName = "24002" }
            };
            _qcLabelRestAPIMock.Setup(x => x.GetAllQCLabelsAsync("", "")).ReturnsAsync(labels);

            var result = await _qcLabelService.GetAllQcLabelsAsync();

            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Tests that GetQcLabelByIdAsync calls the REST API and returns the correct QCLabel.
        /// </summary>
        [TestMethod]
        public async Task GetQcLabelByIdAsync_ValidId_ReturnsLabel()
        {
            var label = new QCLabel { id = 1, batchName = "24001" };
            _qcLabelRestAPIMock.Setup(x => x.GetQCLabelByIdAsync(1)).ReturnsAsync(label);

            var result = await _qcLabelService.GetQcLabelByIdAsync(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that UpdateQcLabelAsync calls the REST API and returns the updated QCLabel.
        /// </summary>
        [TestMethod]
        public async Task UpdateQcLabelAsync_ValidLabel_ReturnsUpdatedLabel()
        {
            var label = new QCLabel { id = 1, batchName = "24001" };
            _qcLabelRestAPIMock.Setup(x => x.UpdateQCLabelAsync(label)).ReturnsAsync(label);

            var result = await _qcLabelService.UpdateQcLabelAsync(label);

            Assert.IsNotNull(result);
            Assert.AreEqual(label.id, result.id);
        }

        /// <summary>
        /// Tests that PopulateQcLabels returns null when product is null.
        /// </summary>
        [TestMethod]
        public void PopulateQcLabels_NullProduct_ReturnsNull()
        {
            var result = _qcLabelService.PopulateQcLabels(null, "24001", "24003", "10");

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PopulateQcLabels returns null when weight is null or empty.
        /// </summary>
        [TestMethod]
        public void PopulateQcLabels_NullWeight_ReturnsNull()
        {
            var product = new Product { id = 1, name = "P1" };
            var result = _qcLabelService.PopulateQcLabels(product, "24001", "24003", null);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that PopulateQcLabels returns null when batch start is greater than batch end.
        /// </summary>
        [TestMethod]
        public void PopulateQcLabels_InvalidRange_ReturnsNull()
        {
            var product = new Product { id = 1, name = "P1" };
            var result = _qcLabelService.PopulateQcLabels(product, "24005", "24003", "10");

            Assert.IsNull(result);
        }
    }
}

