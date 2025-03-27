using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ChemsonLabApp.Services
{
    public class DailyQcService : IDailyQcService
    {
        private readonly IDailyQcRestAPI _dailyQcRestAPI;
        private readonly ICoaRestAPI _coaRestAPI;
        private readonly IQcLabelRestAPI _qcLabelRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;
        private readonly IProductRestAPI _productRestAPI;

        public DailyQcService(
            IDailyQcRestAPI dailyQcRestAPI, 
            ICoaRestAPI coaRestAPI, 
            IQcLabelRestAPI qcLabelRestAPI, 
            IBatchTestResultRestAPI batchTestResultRestAPI,
            IProductRestAPI productRestAPI)
        {
            this._dailyQcRestAPI = dailyQcRestAPI;
            this._coaRestAPI = coaRestAPI;
            this._qcLabelRestAPI = qcLabelRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
            this._productRestAPI = productRestAPI;
        }
        public async Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc)
        {
            return await _dailyQcRestAPI.CreateDailyQcAsync(dailyQc);
        }

        public async Task<bool> DeleteDailyQcAsync(List<DailyQc> dailyQcs)
        {
            try
            {
                foreach (var dailyQc in dailyQcs)
                {
                    if (dailyQc.id == 0) continue;
                    await _dailyQcRestAPI.DeleteDailyQcAsync(dailyQc);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "")
        {
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(filter, sort);
        }

        public async Task<DailyQc> GetDailyQcByIdAsync(int id)
        {
            return await _dailyQcRestAPI.GetDailyQcByIdAsync(id);
        }

        public async Task<List<DailyQc>> LoadTodayDashboardDailyQcAsync(string productName, string year, string month, string status)
        {
            var inProgressDailyQcs = await GetDailyQcs(productName, year, month, status);
            var testedTodayDailyQcs = await GetTestedTodayDailyQcs();

            // Concatenate inProgressDailyQcs and testedTodayDailyQcs
            var dailyQcs = inProgressDailyQcs.Concat(testedTodayDailyQcs).ToList();

            var dailyQcsWithCOALabelBatchMixReqd = await PopulateCOALabelBatchMixReqd(dailyQcs);

            return dailyQcsWithCOALabelBatchMixReqd;
        }

        public async Task<List<DailyQc>> PopulateCOALabelBatchMixReqd(List<DailyQc> dailyQcs)
        {
            CursorUtility.DisplayCursor(true);
            foreach (var dailyQc in dailyQcs)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dailyQc.batches) || dailyQc.product == null) continue;

                    dailyQc.lastCoaBatchName = await GetLastCoaBatchName(dailyQc);
                    dailyQc.lastLabel = await GetDailyQcLabel(dailyQc);
                    dailyQc.lastBatch = await GetLastTest(dailyQc);
                    dailyQc.mixesReqd = GetMixReqd(dailyQc);

                    if (!string.IsNullOrWhiteSpace(dailyQc.lastLabel)) dailyQc.isLastLabelLoaded = true;
                    if (!string.IsNullOrWhiteSpace(dailyQc.lastBatch)) dailyQc.isLastBatchLoaded = true;
                } 
                catch
                {
                    continue;
                }
            }
            CursorUtility.DisplayCursor(false);

            return dailyQcs;
        }

        public async Task<string> GetLastCoaBatchName(DailyQc dailyQc)
        {
            var coaRequired = (bool)dailyQc.product.coa;
            string lastCoaBatchName = coaRequired ? "Reqd" : "";

            if (!coaRequired) return lastCoaBatchName;

            string productName = dailyQc.product.name;
            string batchYear = dailyQc.batches.Trim().Substring(0, 2);
            string latestCoaFilter = $"?productName={productName}&batchName={batchYear}";

            var coas = await _coaRestAPI.GetAllCoasAsync(latestCoaFilter);

            if (coas.Count > 0)
            {
                try
                {
                    var monthAlphabets = Constants.Constants.MonthsAlphabets;

                    for (int i = monthAlphabets.Count - 1; i >= 0; i--)
                    {
                        var month = monthAlphabets[i];
                        var yearMonth = $"{batchYear}{month}".ToUpper();

                        var coasByMonth = coas.Where(coa => coa.batchName.Contains(yearMonth)).ToList();

                        if (coasByMonth.Count > 0 && coasByMonth != null)
                        {
                            var lastCoa = coasByMonth.OrderByDescending(coa =>
                            int.Parse(BatchUtility.RemoveTrailingAlphabetBatchName(coa.batchName.Substring(3)))).FirstOrDefault();

                            lastCoaBatchName = lastCoa.batchName;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // show error message and log error
                    // the error might be due to the format of the batches name or no batches
                    NotificationUtility.ShowError("An error occurred. Please try again or the batch has not input");
                    LoggerUtility.LogError(ex);
                }
            }

            return lastCoaBatchName;
        }

        public async Task<string> GetLastTest(DailyQc dailyQc)
        {
            string lastTestBatchName = "";

            string productName = dailyQc.product.name;
            string batchYear = dailyQc.batches == null || dailyQc.batches == "" ? DateTime.Now.Year.ToString().Substring(2, 2) : dailyQc.batches.Trim().Substring(0, 2);
            string latestBatchTestResultFilter = $"?productName={productName}&batchName={batchYear}";

            var batchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(latestBatchTestResultFilter);

            if (batchTestResults.Count > 0 && batchTestResults != null)
            {
                var monthAlphabets = Constants.Constants.MonthsAlphabets;

                for (int i = monthAlphabets.Count - 1; i >= 0; i--)
                {
                    var month = monthAlphabets[i];
                    var yearMonth = $"{batchYear}{month}".ToUpper();

                    var batchTestResultsByMonth = batchTestResults.Where(batchTestResult => batchTestResult.batch.batchName.Contains(yearMonth)).ToList();

                    if (batchTestResultsByMonth.Count > 0 && batchTestResultsByMonth != null)
                    {
                        var lastBatchTestResult = batchTestResultsByMonth.OrderByDescending(batchTestResult => 
                        int.Parse(BatchUtility.RemoveTrailingAlphabetBatchName(batchTestResult.batch.batchName.Substring(3)))).FirstOrDefault();

                        lastTestBatchName = lastBatchTestResult.batch.batchName;
                        break;
                    }
                }
            }

            return lastTestBatchName;
        }

        public async Task<string> GetDailyQcLabel(DailyQc dailyQc)
        {
            string lastQcLabelBatchName = "";

            string productName = dailyQc.product.name;
            string fullYear = $"20{dailyQc.batches.Trim().Substring(0, 2)}";
            string monthAlphabet = dailyQc.batches.Trim().Substring(2, 1).ToUpper();

            string latestQcLabelFilter = $"?productName={productName}&year={fullYear}&month={monthAlphabet}";
            var qcLabels = await _qcLabelRestAPI.GetAllQCLabelsAsync(latestQcLabelFilter);

            if (qcLabels.Count > 0 && qcLabels != null)
            {
                var lastQcLabel = qcLabels.OrderByDescending(qcLabel => 
                int.Parse(BatchUtility.RemoveTrailingAlphabetBatchName(qcLabel.batchName.Substring(3)))).FirstOrDefault();

                lastQcLabelBatchName = BatchUtility.RemoveTrailingAlphabetBatchName(lastQcLabel.batchName);
            }


            return lastQcLabelBatchName;
        }

        public int GetMixReqd(DailyQc dailyQc)
        {
            int stdReqd = !string.IsNullOrWhiteSpace(dailyQc.stdReqd) ? 1 : 0;

            bool isDoubleBatchMix = dailyQc.product.comment == null ? false : dailyQc.product.comment.Equals("x2", StringComparison.CurrentCultureIgnoreCase);

            int totalBatches = GetTotalBatches(dailyQc);

            if (isDoubleBatchMix)
            {
                return (int)Math.Ceiling(totalBatches / 2.0) + stdReqd;
            }
            else
            {
                return totalBatches + stdReqd;
            }
        }

        private int GetTotalBatches(DailyQc dailyQc)
        {
            int totalBatches = 0;
            string batchesString = BatchUtility.RemoveTrailingAlphabetBatchName(dailyQc.batches).Trim().Substring(3);

            List<string> batchesNumber = batchesString.Split(',').ToList();

            foreach (var batch in batchesNumber)
            {
                if (batch.Contains('-'))
                {
                    totalBatches += GetTotalBatchesFromRange(batch);
                }
                else
                {
                    totalBatches++;
                }
            }

            return totalBatches;
        }

        private int GetTotalBatchesFromRange(string batch)
        {
            string[] batches = batch.Split('-');
            int startBatch = int.Parse(batches[0].Trim());
            int endBatch = int.Parse(batches[1].Trim());

            // count total batches
            return endBatch - startBatch + 1;
        }

        private async Task<List<DailyQc>> GetTestedTodayDailyQcs()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string todayDailyQcFilter = $"?testedDate={today}";
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(todayDailyQcFilter);
        }

        public async Task<List<DailyQc>> GetDailyQcs(string productName, string year, string month, string status)
        {
            string modifiedProductName = productName == null || productName == "All" ? "" : productName;
            string modifiedYear = year == null || year == "All" ? "" : year;
            string modifiedMonth = month == null || month == "All" ? "" : TestMonthUtility.ConvertMonth(month);
            string modifiedStatus = status == "All" ? "" : status;

            string filter = $"?testStatus={modifiedStatus}&productName={modifiedProductName}&year={modifiedYear}&month={modifiedMonth}";
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(filter);
        }

        public async Task<DailyQc> UpdateDailyQcAsync(DailyQc dailyQc)
        {
            return await _dailyQcRestAPI.UpdateDailyQcAsync(dailyQc);
        }

        public async Task SaveAllDailyQcs(List<DailyQc> dailyQcs)
        {
            foreach(var dailyQc in dailyQcs)
            {
                // check if productName, batches is not null
                if (!InputValidationUtility.ValidateNotNullInput(dailyQc.productName, "Product Name") || 
                    !InputValidationUtility.ValidateNotNullInput(dailyQc.batches, "Batches")) return;

                // check if the batches format is valid
                if (!IsBatchesFormatValid(dailyQc.batches))
                {
                    NotificationUtility.ShowError("Invalid batch number format. ex: 25A1 or 25A1-2");
                    return;
                } 

                // Assign product to dailyQc in case changing the product name
                if (dailyQc.product == null || dailyQc.product.name != dailyQc.productName)
                {
                    dailyQc.product = await GetProductFromProductName(dailyQc.productName);
                }
                // fetch last label, last batch test, last coa batch name and mixes required
                dailyQc.lastCoaBatchName = await GetLastCoaBatchName(dailyQc);
                dailyQc.lastLabel = await GetDailyQcLabel(dailyQc);
                dailyQc.lastBatch = await GetLastTest(dailyQc);
                dailyQc.mixesReqd = GetMixReqd(dailyQc);

                if (dailyQc.testStatus == "Yes" && dailyQc.testedDate == null)
                {
                    dailyQc.testedDate = DateTime.Now.Date;
                }

                if (dailyQc.testStatus == null)
                {
                    dailyQc.testStatus = "In Progress";
                }

                dailyQc.batches = dailyQc.batches.ToUpper();

                if (dailyQc.id == 0)
                {

                    if (dailyQc.stdReqd == "DPC")
                    {
                        // Update product to database
                        await UpdateDBDateInProduct(dailyQc);
                        await UpdateDBDateInExcelFile(dailyQc);
                    }

                    // Add new dailyQc to database
                    await CreateDailyQcAsync(dailyQc);
                }
                else
                {
                    if (await IsDPCUpdated(dailyQc))
                    {
                        await UpdateDBDateInProduct(dailyQc);
                        await UpdateDBDateInExcelFile(dailyQc);
                    }

                    // Update dailyQc in database
                    await UpdateDailyQcAsync(dailyQc);
                }
            }
        }

        private bool IsBatchesFormatValid(string batches)
        {
            // if batches is not range, check if the format is valid
            if (!batches.Contains('-')) return InputValidationUtility.ValidateBatchNumberFormat(batches);

            // if bathes in range, split the batches
            string[] batchesArray = batches.Split('-');
            string firstBatch = batchesArray[0].Trim();
            string lastBatch = batchesArray[1].Trim();

            // check if the first batch is valid format
            bool isFirstBatchValid = InputValidationUtility.ValidateBatchNumberFormat(firstBatch);
            // check if the last batch is only number
            bool isLastBatchValid = lastBatch.All(char.IsDigit);

            return isFirstBatchValid && isLastBatchValid;
        }

        private async Task<bool> IsDPCUpdated(DailyQc dailyQc)
        {
            var originalDailyQc = await GetDailyQcByIdAsync(dailyQc.id);

            if (originalDailyQc == null)
            {
                NotificationUtility.ShowWarning("Daily QC not found");
                return false;
            }

            return originalDailyQc.stdReqd != dailyQc.stdReqd;
        }

        private async Task UpdateDBDateInProduct(DailyQc dailyQc)
        {
            // Get product from database by product id
            int? productId = dailyQc.productId;

            if (productId.HasValue)
            {
                try
                {
                    //Product product = await GetProductByIdAsync(productId.Value);

                    var product = await _productRestAPI.GetProductByIdAsync(productId.Value);

                    if (product == null)
                    {
                        NotificationUtility.ShowWarning("Product not found");
                        return;
                    }

                    // Update product date in database
                    product.dbDate = DateTime.Now.Date;
                    await _productRestAPI.UpdateProductAsync(product);
                }
                catch (Exception ex)
                {
                    NotificationUtility.ShowError("An error occurred. Please try again.");
                    LoggerUtility.LogError(ex);
                }
            }
        }

        private async Task UpdateDBDateInExcelFile(DailyQc dailyQc)
        {
            try
            {
                string folderPath = "S:\\Lab";
                string fileName = "Brabender Conditions.xlsx";
                string fullPath = Path.Combine(folderPath, fileName);

                using (var workbook = new XLWorkbook(fullPath))
                {
                    var worksheet = workbook.Worksheet(1);

                    var rows = worksheet.RowsUsed();
                    var columnNames = rows.First().Cells().Select(cell => cell.Value.ToString()).ToList();

                    // find row number of the product
                    int rowNumber = rows.Skip(1).FirstOrDefault(rows => rows.Cell(columnNames.IndexOf("Product") + 1).Value.ToString() == dailyQc.productName)?.RowNumber() ?? 0;
                    int colNumber = columnNames.IndexOf("DB Date\n(6 mths)") + 1;

                    if (rowNumber != 0)
                    {
                        // Update the DB Date in the excel file
                        worksheet.Cell(rowNumber, colNumber).Value = DateTime.Now.Date.ToString("dd/MM/yyyy");

                        // Save change the excel file
                        workbook.SaveAs(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred. Please try again.");
                LoggerUtility.LogError(ex);
            }
        }

        private async Task<Product> GetProductFromProductName(string productName)
        {
            var filter = $"?name={productName}";
            var products = await _productRestAPI.GetProductsAsync(filter);
            return products.FirstOrDefault();
        }
    }
}
