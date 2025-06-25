using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class QcLabelService : IQcLabelService
    {
        private readonly IQcLabelRestAPI _qcLabelRestAPI;

        public QcLabelService(IQcLabelRestAPI qcLabelRestAPI)
        {
            this._qcLabelRestAPI = qcLabelRestAPI;
        }

        /// <summary>
        /// Creates a new QCLabel record asynchronously by calling the underlying REST API.
        /// </summary>
        /// <param name="qcLabel">The QCLabel object to create.</param>
        /// <returns>The created QCLabel object.</returns>
        public async Task<QCLabel> CreateQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.CreateQCLabelAsync(qcLabel);
        }

        /// <summary>
        /// Creates QCLabel records from a list of QCLabel objects asynchronously.
        /// For each QCLabel in the list, checks if a label with the same batch name and product name already exists.
        /// If not, creates a new QCLabel record marked as printed.
        /// Returns true if the operation completes without exceptions, otherwise returns false.
        /// </summary>
        /// <param name="qcLabels">The list of QCLabel objects to process.</param>
        /// <returns>True if all operations succeed; otherwise, false.</returns>
        public async Task<bool> CreateQcLabelFromListAsync(List<QCLabel> qcLabels)
        {
            try
            {
                foreach (var qcLabel in qcLabels)
                {
                    // get batch name of print qc label
                    string batchName = GetBatchName(qcLabel.batchName);
                    // get product name of print qc label
                    string productName = qcLabel.product.name;
                    int productId = qcLabel.product.id;

                    // check if the qc label is already printed
                    string qcLabelFilter = $"?batchName={batchName}&productName={productName}";
                    var existingQcLabels = await GetAllQcLabelsAsync(filter: qcLabelFilter);

                    // if not, create record of the printed qc label
                    if (qcLabels.Count == 0 || qcLabels == null)
                    {
                        var newQCLabel = new QCLabel
                        {
                            batchName = batchName,
                            productId = productId,
                            printed = true,
                        };
                        await CreateQcLabelAsync(newQCLabel);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in CreateQcLabelFromListAsync");
                LoggerUtility.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Extracts and formats the batch name from the provided string.
        /// If the batch name contains a hyphen, it combines the first three characters (year and month)
        /// with the batch number after the hyphen. Otherwise, returns the original batch name.
        /// </summary>
        /// <param name="batchName">The original batch name string.</param>
        /// <returns>The formatted batch name.</returns>
        private string GetBatchName(string batchName)
        {
            if (batchName.Contains("-"))
            {
                string yearMonth = batchName.Substring(0, 3);
                string batchNumber = batchName.Split('-')[1].Trim();
                return $"{yearMonth}{batchNumber}";
            }
            return batchName;
        }

        /// <summary>
        /// Deletes the specified QCLabel asynchronously by calling the underlying REST API.
        /// </summary>
        /// <param name="qcLabel">The QCLabel object to delete.</param>
        /// <returns>The deleted QCLabel object.</returns>
        public async Task<QCLabel> DeleteQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.DeleteQCLabelAsync(qcLabel);
        }

        /// <summary>
        /// Retrieves all QCLabel records asynchronously, with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">Optional filter string for querying QCLabels.</param>
        /// <param name="sort">Optional sort string for ordering QCLabels.</param>
        /// <returns>A list of QCLabel objects.</returns>
        public async Task<List<QCLabel>> GetAllQcLabelsAsync(string filter = "", string sort = "")
        {
            return await _qcLabelRestAPI.GetAllQCLabelsAsync(filter, sort);
        }

        /// <summary>
        /// Retrieves a QCLabel by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the QCLabel.</param>
        /// <returns>The QCLabel object with the specified ID.</returns>
        public async Task<QCLabel> GetQcLabelByIdAsync(int id)
        {
            return await _qcLabelRestAPI.GetQCLabelByIdAsync(id);
        }

        /// <summary>
        /// Updates the specified QCLabel asynchronously by calling the underlying REST API.
        /// </summary>
        /// <param name="qcLabel">The QCLabel object to update.</param>
        /// <returns>The updated QCLabel object.</returns>
        public async Task<QCLabel> UpdateQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.UpdateQCLabelAsync(qcLabel);
        }

        /// <summary>
        /// Populates a list of QCLabel objects for a given product and batch range.
        /// Validates the input parameters, then generates QCLabel instances for each batch number
        /// in the specified range, assigning the provided product and weight to each label.
        /// Returns the list of generated QCLabel objects, or null if validation fails or an error occurs.
        /// </summary>
        /// <param name="product">The product associated with the QC labels.</param>
        /// <param name="batchStart">The starting batch name (e.g., "24001").</param>
        /// <param name="batchEnd">The ending batch name (e.g., "24010").</param>
        /// <param name="weight">The weight to assign to each QC label.</param>
        /// <returns>A list of populated QCLabel objects, or null if validation fails or an error occurs.</returns>
        public List<QCLabel> PopulateQcLabels(Product product, string batchStart, string batchEnd, string weight)
        {
            if (!ValidateQcLabelInputs(product, batchStart, batchEnd, weight)) return null;

            try
            {
                var qcLabels = new List<QCLabel>();

                var batchNamePrefix = batchStart.Substring(0, 3);
                var batchNameStart = int.Parse(batchStart.Substring(3));
                var batchNameEnd = int.Parse(batchEnd.Substring(3));

                if (batchNameStart > batchNameEnd)
                {
                    // swap batchNameStart and batchNameEnd
                    var temp = batchNameStart;
                    batchNameStart = batchNameEnd;
                    batchNameEnd = temp;
                }

                for (int i = batchNameStart; i <= batchNameEnd; i++)
                {
                    var batchName = $"{batchNamePrefix}{i}";
                    var qcLabel = new QCLabel
                    {
                        product = product,
                        batchName = batchName.ToUpper(),
                        weight = weight
                    };

                    qcLabels.Add(qcLabel);
                }

                return qcLabels;
            }
            catch (Exception e)
            {
                NotificationUtility.ShowError("Error in PopulateQcLabels");
                LoggerUtility.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// Validates the input parameters required for creating or populating QCLabel objects.
        /// Checks that the product, batch start, batch end, and weight are not null or empty,
        /// and that the batch numbers are in the correct format.
        /// Returns true if all validations pass; otherwise, false.
        /// </summary>
        /// <param name="product">The product associated with the QC labels.</param>
        /// <param name="batchStart">The starting batch name.</param>
        /// <param name="batchEnd">The ending batch name.</param>
        /// <param name="weight">The weight to assign to each QC label.</param>
        /// <returns>True if all inputs are valid; otherwise, false.</returns>
        private bool ValidateQcLabelInputs(Product product, string batchStart, string batchEnd, string weight)
        {
            var isSelectProductNotNull = InputValidationUtility.ValidateNotNullObject(product, "Product");

            var isBatchNumberStartValid = InputValidationUtility.ValidateNotNullInput(batchStart, "Batch Number Start");
            var isBatchStartFormatValid = InputValidationUtility.ValidateBatchNumberFormat(batchStart);

            var isBatchNumberEndValid = InputValidationUtility.ValidateNotNullInput(batchEnd, "Batch Number End");
            var isBatchEndFormatValid = InputValidationUtility.ValidateBatchNumberFormat(batchEnd);

            var isInputWeightValid = InputValidationUtility.ValidateNotNullInput(weight, "Weight");

            return isSelectProductNotNull && isBatchNumberStartValid && isBatchNumberEndValid && isInputWeightValid && isBatchStartFormatValid && isBatchEndFormatValid;
        }
    }
}
