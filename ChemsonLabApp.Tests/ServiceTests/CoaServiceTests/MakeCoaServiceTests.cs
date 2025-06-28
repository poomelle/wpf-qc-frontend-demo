using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.COAService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.CoaServiceTests
{
    /// <summary>
    /// Unit tests for MakeCoaService public methods.
    /// Tests both GetAllCustomerOrdersAsync and CreateCOAFromTestResultReportAsync methods.
    /// Uses real model classes and mocks external dependencies.
    /// </summary>
    [TestClass]
    public class MakeCoaServiceTests
    {
        private Mock<ICustomerOrderRestAPI> _customerOrderRestAPIMock;
        private Mock<ICoaRestAPI> _coaRestAPIMock;
        private MakeCoaService _makeCoaService;

        /// <summary>
        /// Initializes mock objects and creates the MakeCoaService instance for testing.
        /// Sets up the test environment before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _customerOrderRestAPIMock = new Mock<ICustomerOrderRestAPI>();
            _coaRestAPIMock = new Mock<ICoaRestAPI>();
            _makeCoaService = new MakeCoaService(_customerOrderRestAPIMock.Object, _coaRestAPIMock.Object);
        }

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
                CreateCustomerOrder(1, "Customer1", "Product1"),
                CreateCustomerOrder(2, "Customer2", "Product2")
            };

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync("", ""))
                                    .ReturnsAsync(expectedCustomerOrders);

            // Act
            var result = await _makeCoaService.GetAllCustomerOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Customer1", result[0].customer.name);
            Assert.AreEqual("Customer2", result[1].customer.name);
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
            var filter = "?productName=TestProduct";
            var sort = "orderBy=customerName";
            var expectedCustomerOrders = new List<CustomerOrder>
            {
                CreateCustomerOrder(1, "FilteredCustomer", "TestProduct")
            };

            _customerOrderRestAPIMock.Setup(x => x.GetAllCustomerOrdersAsync(filter, sort))
                                    .ReturnsAsync(expectedCustomerOrders);

            // Act
            var result = await _makeCoaService.GetAllCustomerOrdersAsync(filter, sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("FilteredCustomer", result[0].customer.name);
            Assert.AreEqual("TestProduct", result[0].product.name);
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
            var result = await _makeCoaService.GetAllCustomerOrdersAsync();

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
                                    .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(
                () => _makeCoaService.GetAllCustomerOrdersAsync(),
                "API Error");

            _customerOrderRestAPIMock.Verify(x => x.GetAllCustomerOrdersAsync("", ""), Times.Once);
        }

        #endregion

        #region CreateCOAFromTestResultReportAsync Tests

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync creates a new COA when none exists for the given product and batch.
        /// Verifies that the service checks for existing COAs and creates new ones when needed.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ValidInput_CreatesCOAWhenNoneExists()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("TestProduct", "25A001", 1)
            };

            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""))
                          .ReturnsAsync(new List<Coa>());

            _coaRestAPIMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.Is<Coa>(c =>
                c.productId == 1 &&
                c.batchName == "25A001")), Times.Once);
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync does not create a COA when one already exists for the product and batch.
        /// Verifies that the service avoids creating duplicate COAs.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_COAExists_DoesNotCreateDuplicate()
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

            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""))
                          .ReturnsAsync(existingCoas);

            // Act
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=TestProduct&batchName=25A001", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync handles empty input gracefully without throwing exceptions.
        /// Verifies that the service handles edge cases properly.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_EmptyList_DoesNotThrowException()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>();

            // Act & Assert - Should not throw exception
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Verify no API calls were made
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync(It.IsAny<string>(), ""), Times.Never);
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync processes multiple test result reports correctly.
        /// Verifies that the service handles batch processing and creates COAs for each unique product-batch combination.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_MultipleReports_ProcessesAllCorrectly()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("Product1", "25A001", 1),
                CreateTestResultReport("Product2", "25A002", 2),
                CreateTestResultReport("Product1", "25A003", 1)
            };

            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                          .ReturnsAsync(new List<Coa>());

            _coaRestAPIMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=Product1&batchName=25A001", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=Product2&batchName=25A002", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=Product1&batchName=25A003", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Exactly(3));
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync continues processing subsequent reports when an exception occurs with one report.
        /// Verifies that the service has proper error handling and doesn't stop processing due to individual failures.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ExceptionInOneReport_ContinuesProcessingOthers()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("Product1", "25A001", 1),
                CreateTestResultReport("Product2", "25A002", 2)
            };

            _coaRestAPIMock.SetupSequence(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                          .ThrowsAsync(new Exception("API Error for first report"))
                          .ReturnsAsync(new List<Coa>());

            _coaRestAPIMock.Setup(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ReturnsAsync(new Coa());

            // Act - Should not throw exception
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=Product1&batchName=25A001", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync("?productName=Product2&batchName=25A002", ""), Times.Once);
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Once); // Only one successful
        }

        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync handles exceptions during COA creation gracefully.
        /// Verifies that the service continues processing even when COA creation fails for individual reports.
        /// </summary>
        [TestMethod]
        public async Task CreateCOAFromTestResultReportAsync_ExceptionDuringCreation_ContinuesProcessing()
        {
            // Arrange
            var testResultReports = new List<TestResultReport>
            {
                CreateTestResultReport("Product1", "25A001", 1),
                CreateTestResultReport("Product2", "25A002", 2)
            };

            _coaRestAPIMock.Setup(x => x.GetAllCoasAsync(It.IsAny<string>(), ""))
                          .ReturnsAsync(new List<Coa>());

            _coaRestAPIMock.SetupSequence(x => x.CreateCoaAsync(It.IsAny<Coa>()))
                          .ThrowsAsync(new Exception("Creation failed for first COA"))
                          .ReturnsAsync(new Coa());

            // Act - Should not throw exception
            await _makeCoaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            _coaRestAPIMock.Verify(x => x.GetAllCoasAsync(It.IsAny<string>(), ""), Times.Exactly(2));
            _coaRestAPIMock.Verify(x => x.CreateCoaAsync(It.IsAny<Coa>()), Times.Exactly(2));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test CustomerOrder with the specified ID, customer name, and product name.
        /// Used for setting up test data with proper object relationships using real model classes.
        /// </summary>
        /// <param name="id">The unique identifier for the customer order.</param>
        /// <param name="customerName">The name of the customer.</param>
        /// <param name="productName">The name of the product.</param>
        /// <returns>A properly configured CustomerOrder for testing.</returns>
        private CustomerOrder CreateCustomerOrder(int id, string customerName, string productName)
        {
            return new CustomerOrder
            {
                id = id,
                customer = new Customer
                {
                    id = id,
                    name = customerName,
                    email = $"{customerName.ToLower()}@test.com",
                    status = true
                },
                customerId = id,
                product = new Product
                {
                    id = id,
                    name = productName,
                    status = true,
                    coa = true
                },
                productId = id,
                status = true
            };
        }

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
                        suffix = "A"
                    },
                    testResult = new MVVM.Models.TestResult
                    {
                        id = 1,
                        product = new Product
                        {
                            id = productId,
                            name = productName,
                            status = true,
                            coa = true
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

        #endregion
    }
}

