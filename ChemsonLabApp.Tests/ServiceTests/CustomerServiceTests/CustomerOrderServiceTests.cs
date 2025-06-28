using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.CoaServiceTests
{
    [TestClass]
    public class CustomerOrderServiceTests
    {
        private Mock<ICustomerOrderRestAPI> _customerOrderRestAPIMock;
        private Mock<ICustomerRestAPI> _customerRestAPIMock;
        private CustomerOrderService _customerOrderService;

        [TestInitialize]
        public void Setup()
        {
            _customerOrderRestAPIMock = new Mock<ICustomerOrderRestAPI>();
            _customerRestAPIMock = new Mock<ICustomerRestAPI>();
            _customerOrderService = new CustomerOrderService(_customerOrderRestAPIMock.Object, _customerRestAPIMock.Object);
        }

        #region CreateCustomerOrderAsync Tests

        /// <summary>
        /// Tests that CreateCustomerOrderAsync creates a new customer order when valid customer and product are provided.
        /// Verifies that the service validates inputs, creates a proper CustomerOrder object, and calls the REST API.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerOrderAsync_ValidInputs_CreatesCustomerOrder()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe", "john@test.com");
            var product = CreateProduct(1, "Test Product");
            var expectedCustomerOrder = new CustomerOrder
            {
                id = 1,
                customerId = 1,
                productId = 1,
                status = true
            };

            _customerOrderRestAPIMock.Setup(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()))
                                    .ReturnsAsync(expectedCustomerOrder);

            // Act
            var result = await _customerOrderService.CreateCustomerOrderAsync(customer, product);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.customerId);
            Assert.AreEqual(1, result.productId);
            Assert.IsTrue(result.status);
            _customerOrderRestAPIMock.Verify(x => x.CreateCustomerOrderAsync(It.Is<CustomerOrder>(co =>
                co.customerId == 1 &&
                co.productId == 1 &&
                co.status == true)), Times.Once);
        }

        /// <summary>
        /// Tests that CreateCustomerOrderAsync returns null when customer is null.
        /// Verifies that the service validates customer input and handles null values gracefully.
        /// Pop up a message to the user indicating that customer input is required.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerOrderAsync_NullCustomer_ReturnsNull()
        {
            // Arrange
            Customer customer = null;
            var product = CreateProduct(1, "Test Product");

            // Act
            var result = await _customerOrderService.CreateCustomerOrderAsync(customer, product);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCustomerOrderAsync returns null when product is null.
        /// Verifies that the service validates product input and handles null values gracefully.
        /// Pop up a message to the user indicating that product input is required.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerOrderAsync_NullProduct_ReturnsNull()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe", "john@test.com");
            Product product = null;

            // Act
            var result = await _customerOrderService.CreateCustomerOrderAsync(customer, product);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCustomerOrderAsync returns null when both customer and product are null.
        /// Verifies that the service handles complete null input scenarios properly.
        /// Pop up a message to the user indicating that both inputs are required.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerOrderAsync_BothInputsNull_ReturnsNull()
        {
            // Arrange
            Customer customer = null;
            Product product = null;

            // Act
            var result = await _customerOrderService.CreateCustomerOrderAsync(customer, product);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCustomerOrderAsync propagates exceptions from the REST API.
        /// Verifies that the service doesn't swallow exceptions from the underlying API.
        /// Pop up a message to the user indicating that an error occurred while creating the order.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerOrderAsync_APIThrowsException_PropagatesException()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe", "john@test.com");
            var product = CreateProduct(1, "Test Product");

            _customerOrderRestAPIMock.Setup(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()))
                                    .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _customerOrderService.CreateCustomerOrderAsync(customer, product),
                "API Error");

            _customerOrderRestAPIMock.Verify(x => x.CreateCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Once);
        }

        #endregion

        #region DeleteCustomerOrderAsync Tests

        /// <summary>
        /// Tests that DeleteCustomerOrderAsync deletes a customer order when valid confirmation is provided.
        /// Verifies that the service validates the delete confirmation and calls the REST API.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerOrderAsync_ValidConfirmation_DeletesCustomerOrder()
        {
            // Arrange
            var customerOrder = CreateCustomerOrder(1, 1, 1);
            var deleteConfirmation = "DELETE"; // Assuming this is the valid confirmation string
            var expectedDeletedOrder = CreateCustomerOrder(1, 1, 1);

            _customerOrderRestAPIMock.Setup(x => x.DeleteCustomerOrderAsync(customerOrder))
                                    .ReturnsAsync(expectedDeletedOrder);

            // Act
            var result = await _customerOrderService.DeleteCustomerOrderAsync(customerOrder, deleteConfirmation);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(customerOrder), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteCustomerOrderAsync returns null when invalid confirmation is provided.
        /// Verifies that the service validates delete confirmation and prevents unauthorized deletions.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerOrderAsync_InvalidConfirmation_ReturnsNull()
        {
            // Arrange
            var customerOrder = CreateCustomerOrder(1, 1, 1);
            var invalidConfirmation = "INVALID";

            // Act
            var result = await _customerOrderService.DeleteCustomerOrderAsync(customerOrder, invalidConfirmation);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteCustomerOrderAsync returns null when empty confirmation is provided.
        /// Verifies that the service requires proper confirmation for delete operations.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerOrderAsync_EmptyConfirmation_ReturnsNull()
        {
            // Arrange
            var customerOrder = CreateCustomerOrder(1, 1, 1);
            var emptyConfirmation = "";

            // Act
            var result = await _customerOrderService.DeleteCustomerOrderAsync(customerOrder, emptyConfirmation);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteCustomerOrderAsync returns null when null confirmation is provided.
        /// Verifies that the service handles null confirmation strings properly.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerOrderAsync_NullConfirmation_ReturnsNull()
        {
            // Arrange
            var customerOrder = CreateCustomerOrder(1, 1, 1);
            string nullConfirmation = null;

            // Act
            var result = await _customerOrderService.DeleteCustomerOrderAsync(customerOrder, nullConfirmation);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
        }

        #endregion

        #region GetAllCustomerOrdersAsync Tests

        /// <summary>
        /// Tests that GetAllCustomerOrdersAsync returns customer orders when API call is successful with default parameters.
        /// Verifies that the service correctly passes empty filter and sort parameters to the REST API.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomerOrdersAsync_DefaultParameters_ReturnsCustomerOrders()
        {
            // Arrange
            var expectedCustomerOrders = new List<CustomerOrder>
            {
                CreateCustomerOrder(1, 1, 1),
                CreateCustomerOrder(2, 2, 2)
            };

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync("", ""))
                                    .ReturnsAsync(expectedCustomerOrders);

            // Act
            var result = await _customerOrderService.GetAllCustomerOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].id);
            Assert.AreEqual(2, result[1].id);
            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync("", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllCustomerOrdersAsync correctly passes filter and sort parameters to the REST API.
        /// Verifies that the service acts as a proper proxy for the REST API call.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomerOrdersAsync_WithFilterAndSort_PassesParametersCorrectly()
        {
            // Arrange
            var filter = "?customerId=1";
            var sort = "orderBy=id";
            var expectedCustomerOrders = new List<CustomerOrder>
            {
                CreateCustomerOrder(1, 1, 1)
            };

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync(filter, sort))
                                    .ReturnsAsync(expectedCustomerOrders);

            // Act
            var result = await _customerOrderService.GetAllCustomerOrdersAsync(filter, sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].customerId);
            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync(filter, sort), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllCustomerOrdersAsync returns an empty list when no customer orders are found.
        /// Verifies that the service handles empty results gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomerOrdersAsync_NoOrdersFound_ReturnsEmptyList()
        {
            // Arrange
            var emptyCustomerOrders = new List<CustomerOrder>();

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync(It.IsAny<string>(), It.IsAny<string>()))
                                    .ReturnsAsync(emptyCustomerOrders);

            // Act
            var result = await _customerOrderService.GetAllCustomerOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync("", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllCustomerOrdersAsync propagates exceptions from the REST API.
        /// Verifies that the service doesn't swallow exceptions from the underlying API.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomerOrdersAsync_APIThrowsException_PropagatesException()
        {
            // Arrange
            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync(It.IsAny<string>(), It.IsAny<string>()))
                                    .ThrowsAsync(new Exception("Get API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _customerOrderService.GetAllCustomerOrdersAsync(),
                "Get API Error");

            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync("", ""), Times.Once);
        }

        #endregion

        #region UpdateCustomerAsync Tests

        /// <summary>
        /// Tests that UpdateCustomerAsync updates a customer when valid customer is provided.
        /// Verifies that the service correctly calls the REST API with the customer object.
        /// </summary>
        [TestMethod]
        public async Task UpdateCustomerAsync_ValidCustomer_UpdatesCustomer()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe Updated", "john.updated@test.com");
            var expectedUpdatedCustomer = CreateCustomer(1, "John Doe Updated", "john.updated@test.com");

            _customerRestAPIMock.Setup(x => x.UpdateCustomerAsync(customer))
                               .ReturnsAsync(expectedUpdatedCustomer);

            // Act
            var result = await _customerOrderService.UpdateCustomerAsync(customer);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
            Assert.AreEqual("John Doe Updated", result.name);
            Assert.AreEqual("john.updated@test.com", result.email);
            _customerRestAPIMock.Verify(x => x.UpdateCustomerAsync(customer), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateCustomerAsync handles null customer input gracefully.
        /// Verifies that the service passes null to the REST API (assuming it handles validation).
        /// </summary>
        [TestMethod]
        public async Task UpdateCustomerAsync_NullCustomer_CallsAPIWithNull()
        {
            // Arrange
            Customer customer = null;

            _customerRestAPIMock.Setup(x => x.UpdateCustomerAsync(null))
                               .ReturnsAsync((Customer)null);

            // Act
            var result = await _customerOrderService.UpdateCustomerAsync(customer);

            // Assert
            Assert.IsNull(result);
            _customerRestAPIMock.Verify(x => x.UpdateCustomerAsync(null), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateCustomerAsync handles updates with different customer properties.
        /// Verifies that the service preserves all customer properties during updates.
        /// </summary>
        [TestMethod]
        public async Task UpdateCustomerAsync_CustomerWithDifferentProperties_PreservesAllProperties()
        {
            // Arrange
            var customer = new Customer
            {
                id = 5,
                name = "Jane Smith",
                email = "jane@example.com",
                status = false,
                show = true,
                isViewMode = false,
                isEditMode = true
            };

            _customerRestAPIMock.Setup(x => x.UpdateCustomerAsync(customer))
                               .ReturnsAsync(customer);

            // Act
            var result = await _customerOrderService.UpdateCustomerAsync(customer);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.id);
            Assert.AreEqual("Jane Smith", result.name);
            Assert.AreEqual("jane@example.com", result.email);
            Assert.IsFalse(result.status);
            Assert.IsTrue(result.show);
            Assert.IsFalse(result.isViewMode);
            Assert.IsTrue(result.isEditMode);
            _customerRestAPIMock.Verify(x => x.UpdateCustomerAsync(customer), Times.Once);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test Customer with the specified ID, name, and email.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="id">The unique identifier for the customer.</param>
        /// <param name="name">The name of the customer.</param>
        /// <param name="email">The email address of the customer.</param>
        /// <returns>A properly configured Customer for testing.</returns>
        private Customer CreateCustomer(int id, string name, string email)
        {
            return new Customer
            {
                id = id,
                name = name,
                email = email,
                status = true,
                show = true,
                isViewMode = true,
                isEditMode = false
            };
        }

        /// <summary>
        /// Creates a test Product with the specified ID and name.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="id">The unique identifier for the product.</param>
        /// <param name="name">The name of the product.</param>
        /// <returns>A properly configured Product for testing.</returns>
        private Product CreateProduct(int id, string name)
        {
            return new Product
            {
                id = id,
                name = name,
                status = true,
                coa = true,
                sampleAmount = 10.0,
                torqueWarning = 5.0,
                torqueFail = 10.0,
                fusionWarning = 5.0,
                fusionFail = 10.0,
                show = true,
                isViewMode = true,
                isEditMode = false
            };
        }

        /// <summary>
        /// Creates a test CustomerOrder with the specified ID, customer ID, and product ID.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="id">The unique identifier for the customer order.</param>
        /// <param name="customerId">The ID of the customer placing the order.</param>
        /// <param name="productId">The ID of the product being ordered.</param>
        /// <returns>A properly configured CustomerOrder for testing.</returns>
        private CustomerOrder CreateCustomerOrder(int id, int customerId, int productId)
        {
            return new CustomerOrder
            {
                id = id,
                customerId = customerId,
                productId = productId,
                status = true,
                show = true,
                isViewMode = true,
                isEditMode = false,
                customer = CreateCustomer(customerId, $"Customer{customerId}", $"customer{customerId}@test.com"),
                product = CreateProduct(productId, $"Product{productId}")
            };
        }

        #endregion
    }
}


