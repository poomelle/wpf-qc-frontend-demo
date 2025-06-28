using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChemsonLabApp.Tests.ServiceTests.DataLoaderServiceTests
{
    [TestClass]
    public class DeleteDataLoaderServiceTests
    {
        private Mock<ITestResultReportRestAPI> _testResultReportRestAPIMock;
        private Mock<IBatchTestResultRestAPI> _batchTestResultRestAPIMock;
        private Mock<IBatchRestAPI> _batchRestAPIMock;
        private Mock<IEvaluationRestAPI> _evaluationRestAPIMock;
        private Mock<IMeasurementRestAPI> _measurementRestAPIMock;
        private Mock<ITestResultRestAPI> _testResultRestAPIMock;

        private DeleteDataLoaderService _deleteDataLoaderService;

        /// <summary>
        /// Initializes test setup by creating mock objects and instantiating the DeleteDataLoaderService.
        /// Sets up all required REST API mocks for testing deletion operations.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _testResultReportRestAPIMock = new Mock<ITestResultReportRestAPI>();
            _batchTestResultRestAPIMock = new Mock<IBatchTestResultRestAPI>();
            _batchRestAPIMock = new Mock<IBatchRestAPI>();
            _evaluationRestAPIMock = new Mock<IEvaluationRestAPI>();
            _measurementRestAPIMock = new Mock<IMeasurementRestAPI>();
            _testResultRestAPIMock = new Mock<ITestResultRestAPI>();

            _deleteDataLoaderService = new DeleteDataLoaderService(
                _testResultReportRestAPIMock.Object,
                _batchTestResultRestAPIMock.Object,
                _batchRestAPIMock.Object,
                _evaluationRestAPIMock.Object,
                _measurementRestAPIMock.Object,
                _testResultRestAPIMock.Object
            );
        }

        #region DeleteBatchTestResults Tests

        /// <summary>
        /// Tests that DeleteBatchTestResults successfully deletes all provided BatchTestResult entities when valid confirmation is provided.
        /// Verifies that the method processes each BatchTestResult and calls all necessary deletion APIs.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_ValidConfirmation_DeletesAllBatchTestResults()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>
            {
                CreateSampleBatchTestResult(1),
                CreateSampleBatchTestResult(2)
            };
            var deleteConfirm = "DELETE";

            // Setup mocks to return empty lists (no related data)
            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<TestResultReport>());
            _evaluationRestAPIMock.Setup(x => x.GetAllEvaluationsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Evaluation>());
            _measurementRestAPIMock.Setup(x => x.GetAllMeasurementsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Measurement>());

            // Setup delete operations
            _batchTestResultRestAPIMock.Setup(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()))
                .ReturnsAsync(new BatchTestResult());
            _batchRestAPIMock.Setup(x => x.DeleteBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(new Batch());
            _testResultRestAPIMock.Setup(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(new MVVM.Models.TestResult());

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _batchTestResultRestAPIMock.Verify(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()), Times.Exactly(2));
            _batchRestAPIMock.Verify(x => x.DeleteBatchAsync(It.IsAny<Batch>()), Times.Exactly(2));
            _testResultRestAPIMock.Verify(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that DeleteBatchTestResults does not delete anything when invalid confirmation is provided.
        /// Verifies that no deletion APIs are called when confirmation validation fails.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_InvalidConfirmation_DoesNotDelete()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>
            {
                CreateSampleBatchTestResult(1)
            };
            var deleteConfirm = "INVALID";

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _batchTestResultRestAPIMock.Verify(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()), Times.Never);
            _batchRestAPIMock.Verify(x => x.DeleteBatchAsync(It.IsAny<Batch>()), Times.Never);
            _testResultRestAPIMock.Verify(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteBatchTestResults deletes related TestResultReports when they exist.
        /// Verifies that the method properly handles deletion of associated TestResultReport entities.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_WithTestResultReports_DeletesReports()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>
            {
                CreateSampleBatchTestResult(1)
            };
            var deleteConfirm = "DELETE";
            var testResultReports = new List<TestResultReport>
            {
                new TestResultReport { batchTestResultId = 1 },
                new TestResultReport { batchTestResultId = 1 }
            };

            // Setup mocks
            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(testResultReports);
            _testResultReportRestAPIMock.Setup(x => x.DeleteTestResultReportAsync(It.IsAny<TestResultReport>()))
                .ReturnsAsync(new TestResultReport());
            _evaluationRestAPIMock.Setup(x => x.GetAllEvaluationsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Evaluation>());
            _measurementRestAPIMock.Setup(x => x.GetAllMeasurementsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Measurement>());
            _batchTestResultRestAPIMock.Setup(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()))
                .ReturnsAsync(new BatchTestResult());
            _batchRestAPIMock.Setup(x => x.DeleteBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(new Batch());
            _testResultRestAPIMock.Setup(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(new MVVM.Models.TestResult());

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _testResultReportRestAPIMock.Verify(x => x.DeleteTestResultReportAsync(It.IsAny<TestResultReport>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that DeleteBatchTestResults deletes related Evaluations when they exist.
        /// Verifies that the method properly handles deletion of associated Evaluation entities.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_WithEvaluations_DeletesEvaluations()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>
            {
                CreateSampleBatchTestResult(1)
            };
            var deleteConfirm = "DELETE";
            var evaluations = new List<Evaluation>
            {
                new Evaluation { testResultId = 1 },
                new Evaluation { testResultId = 1 }
            };

            // Setup mocks
            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<TestResultReport>());
            _evaluationRestAPIMock.Setup(x => x.GetAllEvaluationsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(evaluations);
            _evaluationRestAPIMock.Setup(x => x.DeleteEvaluationAsync(It.IsAny<Evaluation>()))
                .ReturnsAsync(new Evaluation());
            _measurementRestAPIMock.Setup(x => x.GetAllMeasurementsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Measurement>());
            _batchTestResultRestAPIMock.Setup(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()))
                .ReturnsAsync(new BatchTestResult());
            _batchRestAPIMock.Setup(x => x.DeleteBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(new Batch());
            _testResultRestAPIMock.Setup(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(new MVVM.Models.TestResult());

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _evaluationRestAPIMock.Verify(x => x.DeleteEvaluationAsync(It.IsAny<Evaluation>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that DeleteBatchTestResults deletes related Measurements when they exist.
        /// Verifies that the method properly handles deletion of associated Measurement entities.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_WithMeasurements_DeletesMeasurements()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>
            {
                CreateSampleBatchTestResult(1)
            };
            var deleteConfirm = "DELETE";
            var measurements = new List<Measurement>
            {
                new Measurement { testResultId = 1 },
                new Measurement { testResultId = 1 }
            };

            // Setup mocks
            _testResultReportRestAPIMock.Setup(x => x.GetAllTestResultReportAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<TestResultReport>());
            _evaluationRestAPIMock.Setup(x => x.GetAllEvaluationsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(new List<Evaluation>());
            _measurementRestAPIMock.Setup(x => x.GetAllMeasurementsAsync(It.IsAny<string>(), ""))
                .ReturnsAsync(measurements);
            _measurementRestAPIMock.Setup(x => x.DeleteMeasurementAsync(It.IsAny<Measurement>()))
                .ReturnsAsync(new Measurement());
            _batchTestResultRestAPIMock.Setup(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()))
                .ReturnsAsync(new BatchTestResult());
            _batchRestAPIMock.Setup(x => x.DeleteBatchAsync(It.IsAny<Batch>()))
                .ReturnsAsync(new Batch());
            _testResultRestAPIMock.Setup(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()))
                .ReturnsAsync(new MVVM.Models.TestResult());

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _measurementRestAPIMock.Verify(x => x.DeleteMeasurementAsync(It.IsAny<Measurement>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that DeleteBatchTestResults handles empty list gracefully without calling any deletion APIs.
        /// Verifies that the method processes empty collections without errors.
        /// </summary>
        [TestMethod]
        public async Task DeleteBatchTestResults_EmptyList_NoOperations()
        {
            // Arrange
            var batchTestResults = new List<BatchTestResult>();
            var deleteConfirm = "DELETE";

            // Act
            await _deleteDataLoaderService.DeleteBatchTestResults(batchTestResults, deleteConfirm);

            // Assert
            _batchTestResultRestAPIMock.Verify(x => x.DeleteBatchTestResultAsync(It.IsAny<BatchTestResult>()), Times.Never);
            _batchRestAPIMock.Verify(x => x.DeleteBatchAsync(It.IsAny<Batch>()), Times.Never);
            _testResultRestAPIMock.Verify(x => x.DeleteTestResultAsync(It.IsAny<MVVM.Models.TestResult>()), Times.Never);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a sample BatchTestResult object for testing purposes.
        /// Provides a complete BatchTestResult with all necessary related objects.
        /// </summary>
        /// <param name="id">The ID to assign to the BatchTestResult.</param>
        /// <returns>A fully populated BatchTestResult object for testing.</returns>
        private BatchTestResult CreateSampleBatchTestResult(int id)
        {
            return new BatchTestResult
            {
                id = id,
                batchId = id,
                testResultId = id,
                batch = new Batch
                {
                    id = id,
                    batchName = $"25A{id}",
                    sampleBy = "TestUser",
                    suffix = ""
                },
                testResult = new MVVM.Models.TestResult
                {
                    id = id,
                    testDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    operatorName = "TestOperator",
                    testMethod = "Rheology",
                    testType = "BCH",
                    status = true,
                    fileName = $"test{id}.mtf"
                }
            };
        }

        #endregion
    }
}

