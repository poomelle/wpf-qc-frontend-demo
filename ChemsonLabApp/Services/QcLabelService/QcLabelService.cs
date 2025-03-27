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

        public async Task<QCLabel> CreateQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.CreateQCLabelAsync(qcLabel);
        }

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

        public async Task<QCLabel> DeleteQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.DeleteQCLabelAsync(qcLabel);
        }

        public async Task<List<QCLabel>> GetAllQcLabelsAsync(string filter = "", string sort = "")
        {
            return await _qcLabelRestAPI.GetAllQCLabelsAsync(filter, sort);
        }

        public async Task<QCLabel> GetQcLabelByIdAsync(int id)
        {
            return await _qcLabelRestAPI.GetQCLabelByIdAsync(id);
        }

        public async Task<QCLabel> UpdateQcLabelAsync(QCLabel qcLabel)
        {
            return await _qcLabelRestAPI.UpdateQCLabelAsync(qcLabel);
        }

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
