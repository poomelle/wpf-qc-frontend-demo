using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.CustomerValidationService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.CustomerServiceTests
{
    /// <summary>
    /// Unit tests for CustomerViewService public methods.
    /// Tests all seven public methods: CreateCustomerAsync, DeleteCustomerAndCustomerOrderAsync, GetAllCustomersAsync, 
    /// LoadActiveCustomers, UpdateCustomerAsync, FilterByName, and FilterByStatus.
    /// Uses real model classes and mocks external dependencies.
    /// </summary>
    [TestClass]
    public class CustomerViewServiceTests
    {
        private Mock<ICustomerRestAPI> _customerRestAPIMock;
        private Mock<ICustomerOrderRestAPI> _customerOrderRestAPIMock;
        private Mock<ICustomerValidationService> _customerValidationServiceMock;
        private CustomerViewService _customerViewService;

        /// <summary>
        /// Initializes mock objects and creates the CustomerViewService instance for testing.
        /// Sets up the test environment before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _customerRestAPIMock = new Mock<ICustomerRestAPI>();
            _customerOrderRestAPIMock = new Mock<ICustomerOrderRestAPI>();
            _customerValidationServiceMock = new Mock<ICustomerValidationService>();
            _customerViewService = new CustomerViewService(
                _customerRestAPIMock.Object,
                _customerOrderRestAPIMock.Object,
                _customerValidationServiceMock.Object
            );
        }

        #region CreateCustomerAsync Tests

        /// <summary>
        /// Tests that CreateCustomerAsync creates a new customer when validation passes.
        /// Verifies that the service validates input, creates a proper Customer object, and calls the REST API.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerAsync_ValidInput_CreatesCustomer()
        {
            // Arrange
            var customerName = "John Doe";
            var email = "john@test.com";
            var expectedCustomer = new Customer
            {
                id = 1,
                name = customerName,
                email = email,
                status = true
            };

            _customerValidationServiceMock.Setup(x => x.ValidateNewCustomerAsync(customerName, email))
                                         .ReturnsAsync(true);

            _customerRestAPIMock.Setup(x => x.CreateCustomerAsync(It.IsAny<Customer>()))
                               .ReturnsAsync(expectedCustomer);

            // Act
            var result = await _customerViewService.CreateCustomerAsync(customerName, email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
            Assert.AreEqual(customerName, result.name);
            Assert.AreEqual(email, result.email);
            Assert.IsTrue(result.status);

            _customerValidationServiceMock.Verify(x => x.ValidateNewCustomerAsync(customerName, email), Times.Once);
            _customerRestAPIMock.Verify(x => x.CreateCustomerAsync(It.Is<Customer>(c =>
                c.name == customerName &&
                c.email == email &&
                c.status == true)), Times.Once);
        }

        /// <summary>
        /// Tests that CreateCustomerAsync returns null when validation fails.
        /// Verifies that the service respects validation results and doesn't create invalid customers.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerAsync_ValidationFails_ReturnsNull()
        {
            // Arrange
            var customerName = "Invalid Name";
            var email = "invalid@email";

            _customerValidationServiceMock.Setup(x => x.ValidateNewCustomerAsync(customerName, email))
                                         .ReturnsAsync(false);

            // Act
            var result = await _customerViewService.CreateCustomerAsync(customerName, email);

            // Assert
            Assert.IsNull(result);
            _customerValidationServiceMock.Verify(x => x.ValidateNewCustomerAsync(customerName, email), Times.Once);
            _customerRestAPIMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCustomerAsync propagates exceptions from the REST API.
        /// Verifies that the service doesn't swallow exceptions from the underlying API.
        /// </summary>
        [TestMethod]
        public async Task CreateCustomerAsync_APIThrowsException_PropagatesException()
        {
            // Arrange
            var customerName = "John Doe";
            var email = "john@test.com";

            _customerValidationServiceMock.Setup(x => x.ValidateNewCustomerAsync(customerName, email))
                                         .ReturnsAsync(true);

            _customerRestAPIMock.Setup(x => x.CreateCustomerAsync(It.IsAny<Customer>()))
                               .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _customerViewService.CreateCustomerAsync(customerName, email),
                "API Error");

            _customerValidationServiceMock.Verify(x => x.ValidateNewCustomerAsync(customerName, email), Times.Once);
            _customerRestAPIMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Once);
        }

        #endregion

        #region DeleteCustomerAndCustomerOrderAsync Tests

        /// <summary>
        /// Tests that DeleteCustomerAndCustomerOrderAsync returns null when confirmation is invalid.
        /// Verifies that the service validates delete confirmation and prevents unauthorized deletions.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerAndCustomerOrderAsync_InvalidConfirmation_ReturnsNull()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe", "john@test.com");
            var invalidConfirmation = "INVALID";

            // Act
            var result = await _customerViewService.DeleteCustomerAndCustomerOrderAsync(customer, invalidConfirmation);

            // Assert
            Assert.IsNull(result);
            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync(It.IsAny<string>(), ""), Times.Never);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
            _customerRestAPIMock.Verify(x => x.DeleteCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteCustomerAndCustomerOrderAsync deletes customer when no orders exist.
        /// Verifies that the service handles customers without orders properly.
        /// </summary>
        [TestMethod]
        public async Task DeleteCustomerAndCustomerOrderAsync_NoOrders_DeletesCustomerOnly()
        {
            // Arrange
            var customer = CreateCustomer(1, "John Doe", "john@test.com");
            var deleteConfirmation = "DELETE";
            var emptyOrderList = new List<CustomerOrder>();

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync($"?customerName={customer.name}", ""))
                                    .ReturnsAsync(emptyOrderList);

            _customerRestAPIMock.Setup(x => x.DeleteCustomerAsync(customer))
                               .ReturnsAsync(customer);

            // Act
            var result = await _customerViewService.DeleteCustomerAndCustomerOrderAsync(customer, deleteConfirmation);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customer.id, result.id);

            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync($"?customerName={customer.name}", ""), Times.Once);
            _customerOrderRestAPIMock.Verify(x => x.DeleteCustomerOrderAsync(It.IsAny<CustomerOrder>()), Times.Never);
            _customerRestAPIMock.Verify(x => x.DeleteCustomerAsync(customer), Times.Once);
        }

        #endregion

        #region GetAllCustomersAsync Tests

        /// <summary>
        /// Tests that GetAllCustomersAsync returns customers when API call is successful with default parameters.
        /// Verifies that the service correctly passes empty filter and sort parameters to the REST API.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomersAsync_DefaultParameters_ReturnsCustomers()
        {
            // Arrange
            var expectedCustomers = new List<Customer>
            {
                CreateCustomer(1, "John Doe", "john@test.com"),
                CreateCustomer(2, "Jane Smith", "jane@test.com")
            };

            _customerRestAPIMock.Setup(x => x.GetAllCustomersAsync("", ""))
                               .ReturnsAsync(expectedCustomers);

            // Act
            var result = await _customerViewService.GetAllCustomersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("John Doe", result[0].name);
            Assert.AreEqual("Jane Smith", result[1].name);
            _customerRestAPIMock.Verify(x => x.GetAllCustomersAsync("", ""), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllCustomersAsync correctly passes filter and sort parameters to the REST API.
        /// Verifies that the service acts as a proper proxy for the REST API call.
        /// </summary>
        [TestMethod]
        public async Task GetAllCustomersAsync_WithFilterAndSort_PassesParametersCorrectly()
        {
            // Arrange
            var filter = "?status=true";
            var sort = "&sortBy=name";
            var expectedCustomers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer", "active@test.com")
            };

            _customerRestAPIMock.Setup(x => x.GetAllCustomersAsync(filter, sort))
                               .ReturnsAsync(expectedCustomers);

            // Act
            var result = await _customerViewService.GetAllCustomersAsync(filter, sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Active Customer", result[0].name);
            _customerRestAPIMock.Verify(x => x.GetAllCustomersAsync(filter, sort), Times.Once);
        }

        #endregion

        #region LoadActiveCustomers Tests

        /// <summary>
        /// Tests that LoadActiveCustomers returns active customers with proper filter and sort parameters.
        /// Verifies that the service applies the correct filter for active customers and sorts by name.
        /// </summary>
        [TestMethod]
        public async Task LoadActiveCustomers_CallsAPIWithCorrectParameters()
        {
            // Arrange
            var expectedCustomers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer 1", "active1@test.com"),
                CreateCustomer(2, "Active Customer 2", "active2@test.com")
            };

            var expectedFilter = "?status=true";
            var expectedSort = "&sortBy=Name&isAscending=true";

            _customerRestAPIMock.Setup(x => x.GetAllCustomersAsync(expectedFilter, expectedSort))
                               .ReturnsAsync(expectedCustomers);

            // Act
            var result = await _customerViewService.LoadActiveCustomers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Active Customer 1", result[0].name);
            Assert.AreEqual("Active Customer 2", result[1].name);
            _customerRestAPIMock.Verify(x => x.GetAllCustomersAsync(expectedFilter, expectedSort), Times.Once);
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
            var customer = CreateCustomer(1, "Updated Customer", "updated@test.com");
            var expectedUpdatedCustomer = CreateCustomer(1, "Updated Customer", "updated@test.com");

            _customerRestAPIMock.Setup(x => x.UpdateCustomerAsync(customer))
                               .ReturnsAsync(expectedUpdatedCustomer);

            // Act
            var result = await _customerViewService.UpdateCustomerAsync(customer);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.id);
            Assert.AreEqual("Updated Customer", result.name);
            Assert.AreEqual("updated@test.com", result.email);
            _customerRestAPIMock.Verify(x => x.UpdateCustomerAsync(customer), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateCustomerAsync propagates exceptions from the REST API.
        /// Verifies that the service doesn't swallow exceptions from the underlying API.
        /// </summary>
        [TestMethod]
        public async Task UpdateCustomerAsync_APIThrowsException_PropagatesException()
        {
            // Arrange
            var customer = CreateCustomer(1, "Test Customer", "test@test.com");

            _customerRestAPIMock.Setup(x => x.UpdateCustomerAsync(customer))
                               .ThrowsAsync(new Exception("Update API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _customerViewService.UpdateCustomerAsync(customer),
                "Update API Error");

            _customerRestAPIMock.Verify(x => x.UpdateCustomerAsync(customer), Times.Once);
        }

        #endregion

        #region FilterByName Tests

        /// <summary>
        /// Tests that FilterByName returns customers with matching names.
        /// Verifies that the filter correctly identifies customers by exact name match.
        /// </summary>
        [TestMethod]
        public void FilterByName_MatchingName_ReturnsFilteredCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "John Doe", "john@test.com"),
                CreateCustomer(2, "Jane Smith", "jane@test.com"),
                CreateCustomer(3, "John Doe", "john2@test.com")
            };

            var selectedCustomerName = "John Doe";

            // Act
            var result = _customerViewService.FilterByName(customers, selectedCustomerName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.name == "John Doe"));
        }

        /// <summary>
        /// Tests that FilterByName returns all customers when filter is "All".
        /// Verifies that the "All" option returns the complete customer list.
        /// </summary>
        [TestMethod]
        public void FilterByName_AllSelected_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "John Doe", "john@test.com"),
                CreateCustomer(2, "Jane Smith", "jane@test.com")
            };

            var selectedCustomerName = "All";

            // Act
            var result = _customerViewService.FilterByName(customers, selectedCustomerName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(customers.Count, result.Count);
        }

        /// <summary>
        /// Tests that FilterByName returns all customers when filter is null or empty.
        /// Verifies that null/empty filters don't exclude any customers.
        /// </summary>
        [TestMethod]
        public void FilterByName_NullOrEmptyName_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "John Doe", "john@test.com"),
                CreateCustomer(2, "Jane Smith", "jane@test.com")
            };

            // Act
            var resultNull = _customerViewService.FilterByName(customers, null);
            var resultEmpty = _customerViewService.FilterByName(customers, "");
            var resultWhitespace = _customerViewService.FilterByName(customers, "   ");

            // Assert
            Assert.AreEqual(2, resultNull.Count);
            Assert.AreEqual(2, resultEmpty.Count);
            Assert.AreEqual(2, resultWhitespace.Count);
        }

        /// <summary>
        /// Tests that FilterByName returns empty list when no customers match the filter.
        /// Verifies that non-matching filters return appropriate results.
        /// </summary>
        [TestMethod]
        public void FilterByName_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "John Doe", "john@test.com"),
                CreateCustomer(2, "Jane Smith", "jane@test.com")
            };

            var selectedCustomerName = "Non-existent Customer";

            // Act
            var result = _customerViewService.FilterByName(customers, selectedCustomerName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region FilterByStatus Tests

        /// <summary>
        /// Tests that FilterByStatus returns only active customers when "Active" is selected.
        /// Verifies that the status filter correctly identifies active customers.
        /// </summary>
        [TestMethod]
        public void FilterByStatus_ActiveSelected_ReturnsActiveCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer", "active@test.com", true),
                CreateCustomer(2, "Inactive Customer", "inactive@test.com", false),
                CreateCustomer(3, "Another Active", "active2@test.com", true)
            };

            var status = "Active";

            // Act
            var result = _customerViewService.FilterByStatus(customers, status);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.status == true));
        }

        /// <summary>
        /// Tests that FilterByStatus returns only inactive customers when "Inactive" is selected.
        /// Verifies that the status filter correctly identifies inactive customers.
        /// </summary>
        [TestMethod]
        public void FilterByStatus_InactiveSelected_ReturnsInactiveCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer", "active@test.com", true),
                CreateCustomer(2, "Inactive Customer", "inactive@test.com", false),
                CreateCustomer(3, "Another Inactive", "inactive2@test.com", false)
            };

            var status = "Inactive";

            // Act
            var result = _customerViewService.FilterByStatus(customers, status);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.status == false));
        }

        /// <summary>
        /// Tests that FilterByStatus returns all customers when "All" is selected.
        /// Verifies that the "All" option returns the complete customer list regardless of status.
        /// </summary>
        [TestMethod]
        public void FilterByStatus_AllSelected_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer", "active@test.com", true),
                CreateCustomer(2, "Inactive Customer", "inactive@test.com", false)
            };

            var status = "All";

            // Act
            var result = _customerViewService.FilterByStatus(customers, status);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(customers.Count, result.Count);
        }

        /// <summary>
        /// Tests that FilterByStatus returns all customers when status is null or empty.
        /// Verifies that null/empty status filters don't exclude any customers.
        /// </summary>
        [TestMethod]
        public void FilterByStatus_NullOrEmptyStatus_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "Active Customer", "active@test.com", true),
                CreateCustomer(2, "Inactive Customer", "inactive@test.com", false)
            };

            // Act
            var resultNull = _customerViewService.FilterByStatus(customers, null);
            var resultEmpty = _customerViewService.FilterByStatus(customers, "");
            var resultWhitespace = _customerViewService.FilterByStatus(customers, "   ");

            // Assert
            Assert.AreEqual(2, resultNull.Count);
            Assert.AreEqual(2, resultEmpty.Count);
            Assert.AreEqual(2, resultWhitespace.Count);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test Customer with the specified ID, name, email, and status.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="id">The unique identifier for the customer.</param>
        /// <param name="name">The name of the customer.</param>
        /// <param name="email">The email address of the customer.</param>
        /// <param name="status">The status of the customer (default: true).</param>
        /// <returns>A properly configured Customer for testing.</returns>
        private Customer CreateCustomer(int id, string name, string email, bool status = true)
        {
            return new Customer
            {
                id = id,
                name = name,
                email = email,
                status = status,
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
                isEditMode = false
            };
        }

        #endregion
    }
}


