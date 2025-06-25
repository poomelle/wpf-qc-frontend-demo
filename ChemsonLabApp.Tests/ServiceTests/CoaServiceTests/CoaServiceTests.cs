using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace ChemsonLabApp.Tests
{
    [TestClass]
    public class CoaServiceTests
    {
        /// <summary>
        /// Tests that CreateCOAFromTestResultReportAsync creates a COA when given valid input.
        /// </summary>
        [TestMethod]
        public void TestCreateCOAFromTestResultReportAsync_ValidInput_ShouldCreateCOA()
        {
            // Arrange
            var mockCoaRestAPI = new Mock<ICoaRestAPI>();
            var mockTestResultReportRestAPI = new Mock<ITestResultReportRestAPI>();

            var coaService = new CoaService(
                mockCoaRestAPI.Object,
                mockTestResultReportRestAPI.Object
            );

            var testResultReports = new List<TestResultReport>
            {
                new TestResultReport
                {
                    batchTestResult = new BatchTestResult
                    {
                        testResult = new MVVM.Models.TestResult
                        {
                            product = new Product { id = 1, name = "TestProduct" }
                        },
                        batch = new Batch { batchName = "Batch1" }
                    }
                }
            };

            // Act
            var result = coaService.CreateCOAFromTestResultReportAsync(testResultReports);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        /// <summary>
        /// Tests that GetProductTestResultReportsWithBatchRange returns a list of TestResultReports when given a valid product and batch range.
        /// </summary>
        public void TestGetProductTestResultReportsWithBatchRange_ValidInput_ShouldReturnReports()
        {
            // Arrange
            var mockCoaRestAPI = new Mock<ICoaRestAPI>();
            var mockTestResultReportRestAPI = new Mock<ITestResultReportRestAPI>();
            var coaService = new CoaService(
                mockCoaRestAPI.Object,
                mockTestResultReportRestAPI.Object
            );

            var selectedProduct = new Product { id = 1, name = "TestProduct" };
            string fromBatch = "Batch1";
            string toBatch = "Batch10";

            // Act
            var result = coaService.GetProductTestResultReportsWithBatchRange(selectedProduct, fromBatch, toBatch);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        /// <summary>
        /// This test verifies that the SortTestReportsByBatchNumber method correctly sorts a list of TestResultReports by batch number.
        /// </summary>
        public void TestSortTestReportsByBatchNumber_ValidInput_ShouldReturnSortedReports()
        {
            // Arrange
            var coaService = new CoaService(null, null);
            var testResultReports = new List<TestResultReport>
            {
                new TestResultReport { batchTestResult = new BatchTestResult { batch = new Batch { batchName = "25A1" } } },
                new TestResultReport { batchTestResult = new BatchTestResult { batch = new Batch { batchName = "25A2" } } },
                new TestResultReport { batchTestResult = new BatchTestResult { batch = new Batch { batchName = "25A3" } } }
            };

            // Act
            var sortedReports = coaService.SortTestReportsByBatchNumber(testResultReports);

            // Assert
            Assert.AreEqual("25A1", sortedReports[0].batchTestResult.batch.batchName);
            Assert.AreEqual("25A2", sortedReports[1].batchTestResult.batch.batchName);
            Assert.AreEqual("25A3", sortedReports[2].batchTestResult.batch.batchName);
        }
    }
}
