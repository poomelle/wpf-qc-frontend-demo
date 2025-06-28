using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.DataLoaderServiceTests
{
    [TestClass]
    public class EditDataLoaderServiceTests
    {
        private Mock<IBatchRestAPI> _batchRestAPIMock;
        private Mock<ITestResultRestAPI> _testResultRestAPIMock;
        private Mock<IEvaluationRestAPI> _evaluationRestAPIMock;
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;

        private EditDataLoaderService _editDataLoaderService;

        /// <summary>
        /// Initializes test setup by creating mock objects and instantiating the EditDataLoaderService.
        /// Sets up all required REST API mocks for testing edit operations.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _batchRestAPIMock = new Mock<IBatchRestAPI>();
            _testResultRestAPIMock = new Mock<ITestResultRestAPI>();
            _evaluationRestAPIMock = new Mock<IEvaluationRestAPI>();
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();

            _editDataLoaderService = new EditDataLoaderService(
                _batchRestAPIMock.Object,
                _testResultRestAPIMock.Object,
                _evaluationRestAPIMock.Object,
                _batchTestResultRestAPIMock.Object
            );
        }

        #region GetBatchInformation Tests

        /// <summary>
        /// Tests that GetBatchInformation retrieves batch data using the batch ID from BatchTestResult.
        /// Verifies that the method calls the batch REST API with the correct ID.
        /// </summary>
        [TestMethod]
        public async Task GetBatchInformation_ValidBatchTestResult_ReturnsBatch()
        {
            // Arrange
            var expectedBatch = new Batch
            {
                id = 1,
                batchName = "25A1",
                sampleBy = "TestUser",
                productId = 1
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                batch = new Batch { id = 1 }
            };

            _batchRestAPIMock.Setup(x => x.GetBatchByIdAsync(1))
                .ReturnsAsync(expectedBatch);

            // Act
            var result = await _editDataLoaderService.GetBatchInformation(batchTestResult);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBatch.id, result.id);
            Assert.AreEqual(expectedBatch.batchName, result.batchName);
            _batchRestAPIMock.Verify(x => x.GetBatchByIdAsync(1), Times.Once);
        }

        #endregion

        #region GetTestResultInformation Tests

        /// <summary>
        /// Tests that GetTestResultInformation retrieves test result data using the test result ID from BatchTestResult.
        /// Verifies that the method calls the test result REST API with the correct ID.
        /// </summary>
        [TestMethod]
        public async Task GetTestResultInformation_ValidBatchTestResult_ReturnsTestResult()
        {
            // Arrange
            var expectedTestResult = new MVVM.Models.TestResult
            {
                id = 1,
                testDate = DateTime.Now.ToString("dd/MM/yyyy"),
                operatorName = "TestOperator",
                testMethod = "Rheology"
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                testResult = new MVVM.Models.TestResult { id = 1 }
            };

            _testResultRestAPIMock.Setup(x => x.GetTestResultByIdAsync(1))
                .ReturnsAsync(expectedTestResult);

            // Act
            var result = await _editDataLoaderService.GetTestResultInformation(batchTestResult);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedTestResult.id, result.id);
            Assert.AreEqual(expectedTestResult.testMethod, result.testMethod);
            _testResultRestAPIMock.Verify(x => x.GetTestResultByIdAsync(1), Times.Once);
        }

        #endregion

        #region GetEvaluationAtPoint Tests

        /// <summary>
        /// Tests that GetEvaluationAtPoint retrieves evaluation data for a specific point using test result ID and point name.
        /// Verifies that the method calls the evaluation REST API with the correct filter and returns the last evaluation.
        /// </summary>
        [TestMethod]
        public async Task GetEvaluationAtPoint_ValidParameters_ReturnsEvaluationAtPoint()
        {
            // Arrange
            var evaluations = new List<Evaluation>
            {
                new Evaluation
                {
                    id = 1,
                    pointName = "X",
                    testResultId = 1,
                    torque = 15.5
                },
                new Evaluation
                {
                    id = 2,
                    pointName = "X",
                    testResultId = 1,
                    torque = 16.0
                }
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                testResult = new MVVM.Models.TestResult { id = 1 }
            };
            var point = "X";

            _evaluationRestAPIMock.Setup(x => x.GetAllEvaluationsAsync($"?testResultId=1&pointName=X", ""))
                .ReturnsAsync(evaluations);

            // Act
            var result = await _editDataLoaderService.GetEvaluationAtPoint(batchTestResult, point);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.id); // Should return the last evaluation
            Assert.AreEqual("X", result.pointName);
            Assert.AreEqual(16.0, result.torque);
        }

        #endregion

        #region UpdateDataLoader Tests

        /// <summary>
        /// Tests that UpdateDataLoader successfully updates test result, evaluations, and batch when no duplicates exist.
        /// Verifies that all update operations are called when uniqueness check passes.
        /// </summary>
        [TestMethod]
        public async Task UpdateDataLoader_NoDuplicates_UpdatesAllEntities()
        {
            // Arrange
            var product = new Product();
            var machine = new Instrument();
            var testResult = new MVVM.Models.TestResult
            {
                id = 1,
                product = product,
                machine = machine,
                testNumber = 1
            };
            var batch = new Batch
            {
                id = 1,
                batchName = "25A1",
                product = product
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                batch = batch,
                testResult = testResult
            };
            var evaluationX = new Evaluation
            {
                id = 1,
                pointName = "X",
                testResult = testResult
            };
            var evaluationT = new Evaluation
            {
                id = 2,
                pointName = "T",
                testResult = testResult
            };

            // Setup: No existing batch test results (uniqueness check passes)
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<BatchTestResult>());

            _testResultRestAPIMock.Setup(x => x.UpdateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(testResult);
            _evaluationRestAPIMock.Setup(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>()))
                .ReturnsAsync(new Evaluation());
            _batchRestAPIMock.Setup(x => x.UpdateBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(batch);

            // Act
            await _editDataLoaderService.UpdateDataLoader(testResult, batch, batchTestResult, evaluationX, evaluationT);

            // Assert
            _testResultRestAPIMock.Verify(x => x.UpdateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Once);
            _evaluationRestAPIMock.Verify(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>()), Times.Exactly(2));
            _batchRestAPIMock.Verify(x => x.UpdateBatchAsync(It.IsAny<Batch>()), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateDataLoader updates entities when the existing record is the same as the current one being updated.
        /// Verifies that updates proceed when the found duplicate is actually the same record.
        /// </summary>
        [TestMethod]
        public async Task UpdateDataLoader_SameRecord_UpdatesAllEntities()
        {
            // Arrange
            var product = new Product();
            var machine = new Instrument();
            var testResult = new MVVM.Models.TestResult
            {
                id = 1,
                product = product,
                machine = machine,
                testNumber = 1
            };
            var batch = new Batch
            {
                id = 1,
                batchName = "25A1",
                product = product
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                batch = batch,
                testResult = testResult
            };
            var evaluationX = new Evaluation
            {
                id = 1,
                pointName = "X",
                testResult = testResult
            };
            var evaluationT = new Evaluation
            {
                id = 2,
                pointName = "T",
                testResult = testResult
            };

            // Setup: One existing batch test result with same ID (updating same record)
            var existingBatchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult { id = 1 } // Same ID as the one being updated
            };
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(existingBatchTestResults);

            _testResultRestAPIMock.Setup(x => x.UpdateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(testResult);
            _evaluationRestAPIMock.Setup(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>()))
                .ReturnsAsync(new Evaluation());
            _batchRestAPIMock.Setup(x => x.UpdateBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(batch);

            // Act
            await _editDataLoaderService.UpdateDataLoader(testResult, batch, batchTestResult, evaluationX, evaluationT);

            // Assert
            _testResultRestAPIMock.Verify(x => x.UpdateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Once);
            _evaluationRestAPIMock.Verify(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>()), Times.Exactly(2));
            _batchRestAPIMock.Verify(x => x.UpdateBatchAsync(It.IsAny<Batch>()), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateDataLoader does not update entities when a duplicate record exists with different ID.
        /// Verifies that updates are prevented when uniqueness check fails due to existing different record.
        /// </summary>
        [TestMethod]
        public async Task UpdateDataLoader_DuplicateExists_DoesNotUpdate()
        {
            // Arrange
            var product = new Product();
            var machine = new Instrument();
            var testResult = new MVVM.Models.TestResult
            {
                id = 1,
                product = product,
                machine = machine,
                testNumber = 1
            };
            var batch = new Batch
            {
                id = 1,
                batchName = "25A1",
                product = product
            };
            var batchTestResult = new BatchTestResult
            {
                id = 1,
                batch = batch,
                testResult = testResult
            };
            var evaluationX = new Evaluation
            {
                id = 1,
                pointName = "X",
                testResult = testResult
            };
            var evaluationT = new Evaluation
            {
                id = 2,
                pointName = "T",
                testResult = testResult
            };

            // Setup: One existing batch test result with different ID (duplicate exists)
            var existingBatchTestResults = new List<BatchTestResult>
            {
                new BatchTestResult { id = 2 } // Different ID from the one being updated
            };
            _batchTestResultRestAPIMock.Setup(x => x.GetAllBatchTestResultsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(existingBatchTestResults);

            // Act
            await _editDataLoaderService.UpdateDataLoader(testResult, batch, batchTestResult, evaluationX, evaluationT);

            // Assert
            _testResultRestAPIMock.Verify(x => x.UpdateTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Never);
            _evaluationRestAPIMock.Verify(x => x.UpdateEvaluationAsync(It.IsAny<Evaluation>()), Times.Never);
            _batchRestAPIMock.Verify(x => x.UpdateBatchAsync(It.IsAny<Batch>()), Times.Never);
        }

        #endregion
    }
}
