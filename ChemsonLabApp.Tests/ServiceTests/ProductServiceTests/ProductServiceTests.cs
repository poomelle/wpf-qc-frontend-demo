using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.ProductServiceTests
{
    [TestClass]
    public class ProductServiceTests
    {
        private Mock<IProductRestAPI> _productRestAPIMock;
        private Mock<ISpecificationService> _specificationServiceMock;
        private ProductService _productService;

        [TestInitialize]
        public void Setup()
        {
            _productRestAPIMock = new Mock<IProductRestAPI>();
            _specificationServiceMock = new Mock<ISpecificationService>();
            _productService = new ProductService(_productRestAPIMock.Object, _specificationServiceMock.Object);
        }

        /// <summary>
        /// Tests that LoadActiveProducts returns only active products sorted by name.
        /// </summary>
        [TestMethod]
        public async Task LoadActiveProducts_ReturnsActiveProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { id = 1, name = "A", status = true },
                new Product { id = 2, name = "B", status = true }
            };
            _productRestAPIMock.Setup(x => x.GetProductsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.LoadActiveProducts();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.status));
        }

        /// <summary>
        /// Tests that LoadAllProducts returns all products with given filter and sort.
        /// </summary>
        [TestMethod]
        public async Task LoadAllProducts_WithFilterAndSort_ReturnsProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { id = 1, name = "A", status = true }
            };
            _productRestAPIMock.Setup(x => x.GetProductsAsync("filter", "sort"))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.LoadAllProducts("filter", "sort");

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        /// Tests that CreateNewProductByProductName creates a new product when name is valid and unique.
        /// </summary>
        [TestMethod]
        public async Task CreateNewProductByProductName_ValidName_CreatesProduct()
        {
            // Arrange
            var name = "NewProduct";
            _productRestAPIMock.Setup(x => x.GetProductsAsync("", ""))
                .ReturnsAsync(new List<Product>());
            _productRestAPIMock.Setup(x => x.CreateProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { id = 1, name = name, status = true });

            // Act
            var result = await _productService.CreateNewProductByProductName(name);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.name);
        }

        /// <summary>
        /// Tests that CreateNewProductByProductName returns null when name is duplicate.
        /// </summary>
        [TestMethod]
        public async Task CreateNewProductByProductName_DuplicateName_ReturnsNull()
        {
            // Arrange
            var name = "Existing";
            _productRestAPIMock.Setup(x => x.GetProductsAsync("", ""))
                .ReturnsAsync(new List<Product> { new Product { id = 1, name = name, status = true } });

            // Act
            var result = await _productService.CreateNewProductByProductName(name);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that UpdateProduct updates the product when name is valid and not duplicate.
        /// </summary>
        [TestMethod]
        public async Task UpdateProduct_ValidProduct_UpdatesProduct()
        {
            // Arrange
            var product = new Product { id = 1, name = "Updated", status = true };
            _productRestAPIMock.Setup(x => x.GetProductsAsync("", ""))
                .ReturnsAsync(new List<Product> { new Product { id = 2, name = "Other", status = true } });
            _productRestAPIMock.Setup(x => x.UpdateProductAsync(product))
                .ReturnsAsync(product);
            _specificationServiceMock.Setup(x => x.UpdateSpecificationFromProductUpdated(product))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProduct(product);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(product.name, result.name);
        }

        /// <summary>
        /// Tests that UpdateProduct returns null when name is duplicate.
        /// </summary>
        [TestMethod]
        public async Task UpdateProduct_DuplicateName_ReturnsNull()
        {
            // Arrange
            var product = new Product { id = 1, name = "Duplicate", status = true };
            _productRestAPIMock.Setup(x => x.GetProductsAsync("", ""))
                .ReturnsAsync(new List<Product>
                {
                    new Product { id = 1, name = "Duplicate", status = true },
                    new Product { id = 2, name = "Duplicate", status = true }
                });

            // Act
            var result = await _productService.UpdateProduct(product);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that DeleteProduct deletes the product when confirmation is valid.
        /// </summary>
        [TestMethod]
        public async Task DeleteProduct_ValidConfirmation_DeletesProduct()
        {
            // Arrange
            var product = new Product { id = 1, name = "ToDelete", status = true };
            var confirmation = "DELETE";
            _productRestAPIMock.Setup(x => x.DeleteProductAsync(product))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteProduct(product, confirmation);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(product.id, result.id);
        }

        /// <summary>
        /// Tests that DeleteProduct returns null when confirmation is invalid.
        /// </summary>
        [TestMethod]
        public async Task DeleteProduct_InvalidConfirmation_ReturnsNull()
        {
            // Arrange
            var product = new Product { id = 1, name = "ToDelete", status = true };
            var confirmation = "INVALID";

            // Act
            var result = await _productService.DeleteProduct(product, confirmation);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetProductFromProductName returns the first matching product.
        /// </summary>
        [TestMethod]
        public async Task GetProductFromProductName_ExistingName_ReturnsProduct()
        {
            // Arrange
            var name = "ProductA";
            var products = new List<Product>
            {
                new Product { id = 1, name = name, status = true }
            };
            _productRestAPIMock.Setup(x => x.GetProductsAsync($"?name={name}", ""))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetProductFromProductName(name);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.name);
        }

        /// <summary>
        /// Tests that GetProductFromProductName returns null when no product matches.
        /// </summary>
        [TestMethod]
        public async Task GetProductFromProductName_NoMatch_ReturnsNull()
        {
            // Arrange
            var name = "NotExist";
            _productRestAPIMock.Setup(x => x.GetProductsAsync($"?name={name}", ""))
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetProductFromProductName(name);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetAllActiveProductName returns names of all active products.
        /// </summary>
        [TestMethod]
        public async Task GetAllActiveProductName_ReturnsActiveProductNames()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { id = 1, name = "A", status = true },
                new Product { id = 2, name = "B", status = true }
            };
            _productRestAPIMock.Setup(x => x.GetProductsAsync("?status=true", "&sortBy=Name&isAscending=true"))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllActiveProductName();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("A"));
            Assert.IsTrue(result.Contains("B"));
        }
    }
}

