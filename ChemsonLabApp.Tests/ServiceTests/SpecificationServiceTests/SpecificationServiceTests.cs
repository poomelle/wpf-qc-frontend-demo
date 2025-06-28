using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.SpecificationServiceTests
{
    [TestClass]
    public class SpecificationServiceTests
    {
        private Mock<ISpecificationRestAPI> _specRestApiMock;
        private Mock<IProductRestAPI> _productRestApiMock;
        private SpecificationService _service;

        [TestInitialize]
        public void Setup()
        {
            _specRestApiMock = new Mock<ISpecificationRestAPI>();
            _productRestApiMock = new Mock<IProductRestAPI>();
            _service = new SpecificationService(_specRestApiMock.Object, _productRestApiMock.Object);
        }

        /// <summary>
        /// Tests that CreateSpecificationAsync calls the REST API and returns the created specification.
        /// </summary>
        [TestMethod]
        public async Task CreateSpecificationAsync_ValidSpec_ReturnsCreatedSpec()
        {
            var spec = new Specification { id = 1 };
            _specRestApiMock.Setup(x => x.CreateSpecificationAsync(spec)).ReturnsAsync(spec);

            var result = await _service.CreateSpecificationAsync(spec);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that CreateSpecificationAndCreateProduct returns true when both product and specification are created.
        /// </summary>
        [TestMethod]
        public async Task CreateSpecificationAndCreateProduct_Valid_ReturnsTrue()
        {
            var product = new Product { id = 1, name = "P1" };
            var spec = new Specification { id = 2 };
            _productRestApiMock.Setup(x => x.CreateProductAsync(product)).ReturnsAsync(product);
            _specRestApiMock.Setup(x => x.CreateSpecificationAsync(It.IsAny<Specification>())).ReturnsAsync(spec);

            var result = await _service.CreateSpecificationAndCreateProduct(product, spec);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that CreateSpecificationAndUpdateProduct returns true when both update and create succeed.
        /// </summary>
        [TestMethod]
        public async Task CreateSpecificationAndUpdateProduct_Valid_ReturnsTrue()
        {
            var product = new Product { id = 1, name = "P1" };
            var spec = new Specification { id = 2 };
            _productRestApiMock.Setup(x => x.UpdateProductAsync(product)).ReturnsAsync(product);
            _specRestApiMock.Setup(x => x.CreateSpecificationAsync(It.IsAny<Specification>())).ReturnsAsync(spec);

            var result = await _service.CreateSpecificationAndUpdateProduct(product, spec);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that DeleteSpecificationAsync returns the deleted specification when confirmation is valid.
        /// </summary>
        [TestMethod]
        public async Task DeleteSpecificationAsync_ValidConfirmation_ReturnsDeletedSpec()
        {
            var spec = new Specification { id = 1 };
            _specRestApiMock.Setup(x => x.DeleteSpecificationAsync(spec)).ReturnsAsync(spec);

            var result = await _service.DeleteSpecificationAsync(spec, "DELETE");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that DeleteSpecificationAsync returns null when confirmation is invalid.
        /// </summary>
        [TestMethod]
        public async Task DeleteSpecificationAsync_InvalidConfirmation_ReturnsNull()
        {
            var spec = new Specification { id = 1 };

            var result = await _service.DeleteSpecificationAsync(spec, "INVALID");

            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetAllSpecificationsAsync returns all specifications.
        /// </summary>
        [TestMethod]
        public async Task GetAllSpecificationsAsync_ReturnsAllSpecs()
        {
            var specs = new List<Specification> { new Specification { id = 1 }, new Specification { id = 2 } };
            _specRestApiMock.Setup(x => x.GetAllSpecificationsAsync("", "")).ReturnsAsync(specs);

            var result = await _service.GetAllSpecificationsAsync();

            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Tests that GetAllActiveSpecificationsAsync returns only active specifications.
        /// </summary>
        [TestMethod]
        public async Task GetAllActiveSpecificationsAsync_ReturnsActiveSpecs()
        {
            var specs = new List<Specification> { new Specification { id = 1, inUse = true } };
            _specRestApiMock.Setup(x => x.GetAllSpecificationsAsync("?inUse=true", "&sortBy=productName&isAscending=true")).ReturnsAsync(specs);

            var result = await _service.GetAllActiveSpecificationsAsync();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].inUse);
        }

        /// <summary>
        /// Tests that GetSpecificationByIdAsync returns the correct specification.
        /// </summary>
        [TestMethod]
        public async Task GetSpecificationByIdAsync_ValidId_ReturnsSpec()
        {
            var spec = new Specification { id = 1 };
            _specRestApiMock.Setup(x => x.GetSpecificationByIdAsync(1)).ReturnsAsync(spec);

            var result = await _service.GetSpecificationByIdAsync(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that UpdateSpecificationAsync calls the REST API and returns the updated specification.
        /// </summary>
        [TestMethod]
        public async Task UpdateSpecificationAsync_ValidSpec_ReturnsUpdatedSpec()
        {
            var spec = new Specification { id = 1 };
            _specRestApiMock.Setup(x => x.UpdateSpecificationAsync(spec)).ReturnsAsync(spec);

            var result = await _service.UpdateSpecificationAsync(spec);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
        }

        /// <summary>
        /// Tests that UpdateSpecificationFromProductUpdated updates all specifications for a product.
        /// </summary>
        [TestMethod]
        public async Task UpdateSpecificationFromProductUpdated_UpdatesAllSpecs()
        {
            var product = new Product { id = 1, name = "P1", status = true };
            var machine = new Instrument { id = 2, name = "M1" };
            var specs = new List<Specification>
            {
                new Specification { id = 1, productId = 1, machine = machine }
            };
            _specRestApiMock.Setup(x => x.GetAllSpecificationsAsync($"?productName={product.name}", "")).ReturnsAsync(specs);
            _specRestApiMock.Setup(x => x.UpdateSpecificationAsync(It.IsAny<Specification>())).ReturnsAsync((Specification s) => s);

            await _service.UpdateSpecificationFromProductUpdated(product);

            _specRestApiMock.Verify(x => x.UpdateSpecificationAsync(It.IsAny<Specification>()), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateSpecificationAndUpdateProduct returns true when both update operations succeed.
        /// </summary>
        [TestMethod]
        public async Task UpdateSpecificationAndUpdateProduct_Valid_ReturnsTrue()
        {
            var product = new Product { id = 1, name = "P1", status = true };
            var machine = new Instrument { id = 2, name = "M1" };
            var spec = new Specification { id = 1, product = product, machine = machine, inUse = true };

            _productRestApiMock.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(product);
            _specRestApiMock.Setup(x => x.UpdateSpecificationAsync(It.IsAny<Specification>())).ReturnsAsync(spec);

            var result = await _service.UpdateSpecificationAndUpdateProduct(spec);

            Assert.IsTrue(result);
        }
    }
}

