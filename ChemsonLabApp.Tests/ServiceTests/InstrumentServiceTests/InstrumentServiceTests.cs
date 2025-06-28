using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.InstrumentServiceTests
{
    [TestClass]
    public class InstrumentServiceTests
    {
        private Mock<IInstrumentRestAPI> _instrumentRestAPIMock;
        private InstrumentService _instrumentService;

        /// <summary>
        /// Initializes test setup by creating mock objects and instantiating the InstrumentService.
        /// Sets up the instrument REST API mock for testing instrument operations.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _instrumentRestAPIMock = new Mock<IInstrumentRestAPI>();
            _instrumentService = new InstrumentService(_instrumentRestAPIMock.Object);
        }

        #region CreateInstrumentAsync Tests

        /// <summary>
        /// Tests that CreateInstrumentAsync successfully creates a new instrument when a valid unique name is provided.
        /// Verifies that the method returns true and calls the create API when validation passes.
        /// </summary>
        [TestMethod]
        public async Task CreateInstrumentAsync_ValidUniqueName_ReturnsTrue()
        {
            // Arrange
            var instrumentName = "NewInstrument";
            var existingInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "ExistingInstrument", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("", ""))
                .ReturnsAsync(existingInstruments);
            _instrumentRestAPIMock.Setup(x => x.CreateInstrumentAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Instrument { id = 2, name = instrumentName, status = true });

            // Act
            var result = await _instrumentService.CreateInstrumentAsync(instrumentName);

            // Assert
            Assert.IsTrue(result);
            _instrumentRestAPIMock.Verify(x => x.CreateInstrumentAsync(It.Is<Instrument>(i =>
                i.name == instrumentName && i.status == true)), Times.Once);
        }

        /// <summary>
        /// Tests that CreateInstrumentAsync returns false when attempting to create an instrument with a duplicate name.
        /// Verifies that the method does not call the create API when validation fails due to duplicate name.
        /// </summary>
        [TestMethod]
        public async Task CreateInstrumentAsync_DuplicateName_ReturnsFalse()
        {
            // Arrange
            var instrumentName = "ExistingInstrument";
            var existingInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "ExistingInstrument", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("", ""))
                .ReturnsAsync(existingInstruments);

            // Act
            var result = await _instrumentService.CreateInstrumentAsync(instrumentName);

            // Assert
            Assert.IsFalse(result);
            _instrumentRestAPIMock.Verify(x => x.CreateInstrumentAsync(It.IsAny<Instrument>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateInstrumentAsync returns false when attempting to create an instrument with null or empty name.
        /// Verifies that the method does not call the create API when validation fails due to invalid name.
        /// </summary>
        [TestMethod]
        public async Task CreateInstrumentAsync_NullOrEmptyName_ReturnsFalse()
        {
            // Arrange
            var instrumentName = "";
            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("", ""))
                .ReturnsAsync(new List<Instrument>());

            // Act
            var result = await _instrumentService.CreateInstrumentAsync(instrumentName);

            // Assert
            Assert.IsFalse(result);
            _instrumentRestAPIMock.Verify(x => x.CreateInstrumentAsync(It.IsAny<Instrument>()), Times.Never);
        }

        #endregion

        #region DeleteInstrumentAsync Tests

        /// <summary>
        /// Tests that DeleteInstrumentAsync successfully deletes an instrument when valid confirmation is provided.
        /// Verifies that the method calls the delete API and returns the deleted instrument.
        /// </summary>
        [TestMethod]
        public async Task DeleteInstrumentAsync_ValidConfirmation_ReturnsDeletedInstrument()
        {
            // Arrange
            var instrument = new Instrument { id = 1, name = "TestInstrument", status = true };
            var deleteConfirmation = "DELETE";

            _instrumentRestAPIMock.Setup(x => x.DeleteInstrumentAsync(instrument))
                .ReturnsAsync(instrument);

            // Act
            var result = await _instrumentService.DeleteInstrumentAsync(instrument, deleteConfirmation);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(instrument.id, result.id);
            _instrumentRestAPIMock.Verify(x => x.DeleteInstrumentAsync(instrument), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteInstrumentAsync returns null when invalid confirmation is provided.
        /// Verifies that the method does not call the delete API when confirmation validation fails.
        /// </summary>
        [TestMethod]
        public async Task DeleteInstrumentAsync_InvalidConfirmation_ReturnsNull()
        {
            // Arrange
            var instrument = new Instrument { id = 1, name = "TestInstrument", status = true };
            var deleteConfirmation = "INVALID";

            // Act
            var result = await _instrumentService.DeleteInstrumentAsync(instrument, deleteConfirmation);

            // Assert
            Assert.IsNull(result);
            _instrumentRestAPIMock.Verify(x => x.DeleteInstrumentAsync(It.IsAny<Instrument>()), Times.Never);
        }

        #endregion

        #region GetAllActiveInstrument Tests

        /// <summary>
        /// Tests that GetAllActiveInstrument retrieves all active instruments with correct filter and sort parameters.
        /// Verifies that the method calls the API with status=true filter and name sorting.
        /// </summary>
        [TestMethod]
        public async Task GetAllActiveInstrument_ReturnsActiveInstruments()
        {
            // Arrange
            var activeInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "Instrument1", status = true },
                new Instrument { id = 2, name = "Instrument2", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(activeInstruments);

            // Act
            var result = await _instrumentService.GetAllActiveInstrument();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(i => i.status == true));
        }

        #endregion

        #region GetAllInstrumentsAsync Tests

        /// <summary>
        /// Tests that GetAllInstrumentsAsync retrieves all instruments with default parameters.
        /// Verifies that the method calls the API with empty filter and sort when no parameters are provided.
        /// </summary>
        [TestMethod]
        public async Task GetAllInstrumentsAsync_DefaultParameters_ReturnsAllInstruments()
        {
            // Arrange
            var allInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "Instrument1", status = true },
                new Instrument { id = 2, name = "Instrument2", status = false }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("", ""))
                .ReturnsAsync(allInstruments);

            // Act
            var result = await _instrumentService.GetAllInstrumentsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _instrumentRestAPIMock.Verify(x => x.GetInstrumentsAsync("", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllInstrumentsAsync retrieves instruments with custom filter and sort parameters.
        /// Verifies that the method passes through the provided filter and sort parameters to the API.
        /// </summary>
        [TestMethod]
        public async Task GetAllInstrumentsAsync_CustomParameters_ReturnsFilteredInstruments()
        {
            // Arrange
            var filter = "?status=true";
            var sort = "&sortBy=Name";
            var filteredInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "Instrument1", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync(filter, sort))
                .ReturnsAsync(filteredInstruments);

            // Act
            var result = await _instrumentService.GetAllInstrumentsAsync(filter, sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _instrumentRestAPIMock.Verify(x => x.GetInstrumentsAsync(filter, sort), Times.Once);
        }

        #endregion

        #region UpdateInstrumentAsync Tests

        /// <summary>
        /// Tests that UpdateInstrumentAsync successfully updates an instrument when validation passes.
        /// Verifies that the method returns true and calls the update API when name validation succeeds.
        /// </summary>
        [TestMethod]
        public async Task UpdateInstrumentAsync_ValidUpdate_ReturnsTrue()
        {
            // Arrange
            var instrumentToUpdate = new Instrument { id = 1, name = "UpdatedName", status = true };
            var activeInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "OriginalName", status = true },
                new Instrument { id = 2, name = "OtherInstrument", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(activeInstruments);
            _instrumentRestAPIMock.Setup(x => x.UpdateInstrumentAsync(instrumentToUpdate))
                .ReturnsAsync(instrumentToUpdate);

            // Act
            var result = await _instrumentService.UpdateInstrumentAsync(instrumentToUpdate);

            // Assert
            Assert.IsTrue(result);
            _instrumentRestAPIMock.Verify(x => x.UpdateInstrumentAsync(instrumentToUpdate), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateInstrumentAsync returns false when attempting to update with a duplicate name.
        /// Verifies that the method does not call the update API when validation fails due to duplicate name.
        /// </summary>
        [TestMethod]
        public async Task UpdateInstrumentAsync_DuplicateName_ReturnsFalse()
        {
            // Arrange
            var instrumentToUpdate = new Instrument { id = 1, name = "ExistingName", status = true };
            var activeInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "OriginalName", status = true },
                new Instrument { id = 2, name = "ExistingName", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(activeInstruments);

            // Act
            var result = await _instrumentService.UpdateInstrumentAsync(instrumentToUpdate);

            // Assert
            Assert.IsFalse(result);
            _instrumentRestAPIMock.Verify(x => x.UpdateInstrumentAsync(It.IsAny<Instrument>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateInstrumentAsync returns false when attempting to update with null or empty name.
        /// Verifies that the method does not call the update API when validation fails due to invalid name.
        /// </summary>
        [TestMethod]
        public async Task UpdateInstrumentAsync_NullOrEmptyName_ReturnsFalse()
        {
            // Arrange
            var instrumentToUpdate = new Instrument { id = 1, name = "", status = true };
            var activeInstruments = new List<Instrument>
            {
                new Instrument { id = 2, name = "OtherInstrument", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(activeInstruments);

            // Act
            var result = await _instrumentService.UpdateInstrumentAsync(instrumentToUpdate);

            // Assert
            Assert.IsFalse(result);
            _instrumentRestAPIMock.Verify(x => x.UpdateInstrumentAsync(It.IsAny<Instrument>()), Times.Never);
        }

        #endregion

        #region GetAllActiveInstrumentName Tests

        /// <summary>
        /// Tests that GetAllActiveInstrumentName retrieves names of all active instruments.
        /// Verifies that the method returns a list of instrument names extracted from active instruments.
        /// </summary>
        [TestMethod]
        public async Task GetAllActiveInstrumentName_ReturnsActiveInstrumentNames()
        {
            // Arrange
            var activeInstruments = new List<Instrument>
            {
                new Instrument { id = 1, name = "Instrument1", status = true },
                new Instrument { id = 2, name = "Instrument2", status = true },
                new Instrument { id = 3, name = "Instrument3", status = true }
            };

            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(activeInstruments);

            // Act
            var result = await _instrumentService.GetAllActiveInstrumentName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains("Instrument1"));
            Assert.IsTrue(result.Contains("Instrument2"));
            Assert.IsTrue(result.Contains("Instrument3"));
        }

        /// <summary>
        /// Tests that GetAllActiveInstrumentName returns empty list when no active instruments exist.
        /// Verifies that the method handles empty collections gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetAllActiveInstrumentName_NoActiveInstruments_ReturnsEmptyList()
        {
            // Arrange
            _instrumentRestAPIMock.Setup(x => x.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(new List<Instrument>());

            // Act
            var result = await _instrumentService.GetAllActiveInstrumentName();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion
    }
}

