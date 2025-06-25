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
        /// <summary>
        /// Creates a new DailyQc entry asynchronously using the REST API.
        /// </summary>
        public async Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc)
        {
            return await _dailyQcRestAPI.CreateDailyQcAsync(dailyQc);
        }

        /// <summary>
        /// Deletes a list of DailyQc entries asynchronously using the REST API.
        /// Skips entries with id 0. Returns true if all deletions succeed, false otherwise.
        /// </summary>
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

        /// <summary>
        /// Retrieves all DailyQc entries asynchronously, with optional filter and sort parameters.
        /// </summary>
        public async Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "")
        {
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(filter, sort);
        }

        /// <summary>
        /// Retrieves a DailyQc entry by its ID asynchronously.
        /// </summary>
        public async Task<DailyQc> GetDailyQcByIdAsync(int id)
        {
            return await _dailyQcRestAPI.GetDailyQcByIdAsync(id);
        }

        /// <summary>
        /// Loads DailyQc entries for today's dashboard, including in-progress and tested-today entries,
        /// and populates additional properties such as COA, label, batch, and mixes required.
        /// </summary>
        public async Task<List<DailyQc>> LoadTodayDashboardDailyQcAsync(string productName, string year, string month, string status)
        {
            var inProgressDailyQcs = await GetDailyQcs(productName, year, month, status);
            var testedTodayDailyQcs = await GetTestedTodayDailyQcs();

            // Concatenate inProgressDailyQcs and testedTodayDailyQcs
            var dailyQcs = inProgressDailyQcs.Concat(testedTodayDailyQcs).ToList();

            var dailyQcsWithCOALabelBatchMixReqd = await PopulateCOALabelBatchMixReqd(dailyQcs);

            return dailyQcsWithCOALabelBatchMixReqd;
        }

        /// <summary>
        /// Populates the given list of DailyQc entries with last COA batch name, last label, last batch, and mixes required.
        /// Updates flags indicating if last label and last batch are loaded.
        /// </summary>
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

        /// <summary>
        /// Retrieves the last COA (Certificate of Analysis) batch name for the specified DailyQc entry.
        /// If COA is required for the product, it searches for the most recent COA batch name based on the batch year and month.
        /// Returns "Reqd" if COA is required but not found, or an empty string if not required.
        /// Handles errors related to batch name format or missing batches gracefully.
        /// </summary>
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

        /// <summary>
        /// Retrieves the last tested batch name for the specified DailyQc entry.
        /// Searches for the most recent batch test result based on the batch year and month.
        /// Returns the batch name of the last test if found, otherwise returns an empty string.
        /// </summary>
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

        /// <summary>
        /// Retrieves the last QC label batch name for the specified DailyQc entry.
        /// Constructs a filter using the product name, year, and month extracted from the batch name.
        /// Queries the QC label REST API for matching labels, orders them by batch number in descending order,
        /// and returns the batch name of the most recent label if found; otherwise, returns an empty string.
        /// </summary>
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

        /// <summary>
        /// Calculates the number of mixes required for a given DailyQc entry.
        /// Considers if a standard is required and whether the product is a double batch mix ("x2" in product comment).
        /// If double batch mix, divides total batches by 2 and rounds up, then adds standard required if applicable.
        /// Otherwise, returns total batches plus standard required.
        /// </summary>
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

        /// <summary>
        /// Calculates the total number of batches for a given DailyQc entry.
        /// Parses the batches string, handling both individual batches and batch ranges (e.g., "25A1,25A2-4").
        /// For batch ranges, computes the count as the difference between start and end plus one.
        /// Returns the total count of batches.
        /// </summary>
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

        /// <summary>
        /// Parses a batch range string (e.g., "1-3") and returns the total number of batches in the range.
        /// </summary>
        /// <param name="batch">A string representing a batch range, formatted as "start-end".</param>
        /// <returns>The total number of batches in the specified range.</returns>
        private int GetTotalBatchesFromRange(string batch)
        {
            string[] batches = batch.Split('-');
            int startBatch = int.Parse(batches[0].Trim());
            int endBatch = int.Parse(batches[1].Trim());

            // count total batches
            return endBatch - startBatch + 1;
        }

        /// <summary>
        /// Retrieves all DailyQc entries that were tested today.
        /// </summary>
        /// <returns>A list of DailyQc entries with today's tested date.</returns>
        private async Task<List<DailyQc>> GetTestedTodayDailyQcs()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string todayDailyQcFilter = $"?testedDate={today}";
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(todayDailyQcFilter);
        }

        /// <summary>
        /// Retrieves DailyQc entries filtered by product name, year, month, and status.
        /// </summary>
        /// <param name="productName">The product name to filter by.</param>
        /// <param name="year">The year to filter by.</param>
        /// <param name="month">The month to filter by.</param>
        /// <param name="status">The test status to filter by.</param>
        /// <returns>A list of DailyQc entries matching the specified filters.</returns>
        public async Task<List<DailyQc>> GetDailyQcs(string productName, string year, string month, string status)
        {
            string modifiedProductName = productName == null || productName == "All" ? "" : productName;
            string modifiedYear = year == null || year == "All" ? "" : year;
            string modifiedMonth = month == null || month == "All" ? "" : TestMonthUtility.ConvertMonth(month);
            string modifiedStatus = status == "All" ? "" : status;

            string filter = $"?testStatus={modifiedStatus}&productName={modifiedProductName}&year={modifiedYear}&month={modifiedMonth}";
            return await _dailyQcRestAPI.GetAllDailyQcsAsync(filter);
        }

        /// <summary>
        /// Updates an existing DailyQc entry asynchronously using the REST API.
        /// </summary>
        /// <param name="dailyQc">The DailyQc entry to update.</param>
        /// <returns>The updated DailyQc entry.</returns>
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

        /// <summary>
        /// Validates the format of the batches string.
        /// If the string does not contain a range ('-'), it checks if the format is valid using the utility method.
        /// If the string contains a range, it splits the string and validates the first batch format and ensures the last batch is numeric.
        /// Returns true if the format is valid, otherwise false.
        /// </summary>
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

        /// <summary>
        /// Determines if the DPC (stdReqd) value has been updated for the given DailyQc entry.
        /// Fetches the original DailyQc from the database and compares the stdReqd property.
        /// Returns true if the value has changed, otherwise false.
        /// </summary>
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

        /// <summary>
        /// Updates the DB date for the specified product in the database to the current date.
        /// If the product is not found, shows a warning notification.
        /// Handles and logs exceptions that may occur during the update process.
        /// </summary>
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

        /// <summary>
        /// Updates the "DB Date (6 mths)" column for the specified product in the "Brabender Conditions.xlsx" Excel file
        /// to the current date. If the product row is found, the date is updated and the file is saved.
        /// Handles and logs exceptions that may occur during the update process.
        /// </summary>
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

        /// <summary>
        /// Retrieves the first Product entity that matches the specified product name using the Product REST API.
        /// </summary>
        /// <param name="productName">The name of the product to search for.</param>
        /// <returns>The first matching Product, or null if no match is found.</returns>
        private async Task<Product> GetProductFromProductName(string productName)
        {
            var filter = $"?name={productName}";
            var products = await _productRestAPI.GetProductsAsync(filter);
            return products.FirstOrDefault();
        }
    }
}
