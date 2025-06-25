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
    public class MakeCoaServiceTests
    {
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
    }
}
