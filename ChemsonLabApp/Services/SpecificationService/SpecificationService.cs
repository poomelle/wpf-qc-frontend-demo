using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class SpecificationService : ISpecificationService
    {
        private readonly ISpecificationRestAPI _specificationRestAPI;
        private readonly IProductRestAPI _productRestAPI;

        public SpecificationService(ISpecificationRestAPI specificationRestAPI, IProductRestAPI productRestAPI)
        {
            this._specificationRestAPI = specificationRestAPI;
            this._productRestAPI = productRestAPI;
        }

        /// <summary>
        /// Creates a new specification asynchronously by calling the specification REST API.
        /// </summary>
        /// <param name="specification">The specification to create.</param>
        /// <returns>The created <see cref="Specification"/> object.</returns>
        public async Task<Specification> CreateSpecificationAsync(Specification specification)
        {
            return await _specificationRestAPI.CreateSpecificationAsync(specification);
        }

        /// <summary>
        /// Creates a new product and then creates a specification for that product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <param name="specification">The specification to create for the product.</param>
        /// <returns>True if both product and specification are created successfully; otherwise, false.</returns>
        public async Task<bool> CreateSpecificationAndCreateProduct(Product product, Specification specification)
        {
            var createdProduct = await _productRestAPI.CreateProductAsync(product);

            if (createdProduct != null)
            {
                specification.productId = createdProduct.id;
                var createdSpecification = await _specificationRestAPI.CreateSpecificationAsync(specification);

                if (createdSpecification != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates an existing product and then creates a specification for it.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <param name="specification">The specification to create for the updated product.</param>
        /// <returns>True if both product update and specification creation are successful; otherwise, false.</returns>
        public async Task<bool> CreateSpecificationAndUpdateProduct(Product product, Specification specification)
        {
            specification.productId = product.id;
            var updatedProduct = await _productRestAPI.UpdateProductAsync(product);
            var createdProduct = await _specificationRestAPI.CreateSpecificationAsync(specification);

            if (updatedProduct != null && createdProduct != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes a specification after confirming the delete action.
        /// </summary>
        /// <param name="specification">The specification to delete.</param>
        /// <param name="deleteConfirm">The confirmation string for deletion.</param>
        /// <returns>The deleted <see cref="Specification"/> object, or null if not confirmed.</returns>
        public async Task<Specification> DeleteSpecificationAsync(Specification specification, string deleteConfirm)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirm)) return null;
            return await _specificationRestAPI.DeleteSpecificationAsync(specification);
        }

        /// <summary>
        /// Retrieves all specifications with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">The filter string for querying specifications.</param>
        /// <param name="sort">The sort string for ordering specifications.</param>
        /// <returns>A list of <see cref="Specification"/> objects.</returns>
        public async Task<List<Specification>> GetAllSpecificationsAsync(string filter = "", string sort = "")
        {
            return await _specificationRestAPI.GetAllSpecificationsAsync(filter, sort);
        }

        /// <summary>
        /// Retrieves all active specifications, sorted by product name in ascending order.
        /// </summary>
        /// <returns>A list of active <see cref="Specification"/> objects.</returns>
        public async Task<List<Specification>> GetAllActiveSpecificationsAsync()
        {
            string filter = "?inUse=true";
            string sort = "&sortBy=productName&isAscending=true";

            return await GetAllSpecificationsAsync(filter, sort);
        }

        /// <summary>
        /// Retrieves a specification by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the specification.</param>
        /// <returns>The <see cref="Specification"/> object with the specified ID.</returns>
        public async Task<Specification> GetSpecificationByIdAsync(int id)
        {
            return await _specificationRestAPI.GetSpecificationByIdAsync(id);
        }

        /// <summary>
        /// Updates an existing specification asynchronously.
        /// </summary>
        /// <param name="specification">The specification to update.</param>
        /// <returns>The updated <see cref="Specification"/> object.</returns>
        public async Task<Specification> UpdateSpecificationAsync(Specification specification)
        {
            return await _specificationRestAPI.UpdateSpecificationAsync(specification);
        }

        /// <summary>
        /// Updates all specifications associated with the given product.
        /// Sets the specification's inUse, productId, and machineId fields to match the updated product.
        /// </summary>
        /// <param name="updateProduct">The product with updated information.</param>
        public async Task UpdateSpecificationFromProductUpdated(Product updateProduct)
        {
            string filter = $"?productName={updateProduct.name}";

            var specifications = await GetAllSpecificationsAsync(filter);

            if (specifications != null && specifications.Count > 0)
            {
                foreach (var specification in specifications)
                {
                    specification.inUse = updateProduct.status;
                    specification.productId = updateProduct.id;
                    specification.machineId = specification.machine.id;

                    await UpdateSpecificationAsync(specification);
                }
            }
        }

        /// <summary>
        /// Updates both the product and its associated specification in the database.
        /// </summary>
        /// <param name="specification">The specification containing updated product and specification data.</param>
        /// <returns>True if both product and specification are updated successfully; otherwise, false.</returns>
        public async Task<bool> UpdateSpecificationAndUpdateProduct(Specification specification)
        {
            Product updateProduct = new Product()
            {
                id = specification.product.id,
                name = specification.product.name,
                dbDate = specification.product.dbDate,
                comment = specification.product.comment,
                coa = specification.product.coa,
                status = specification.inUse,
                colour = specification.product.colour,
                sampleAmount = specification.product.sampleAmount,
                torqueWarning = specification.product.torqueWarning,
                torqueFail = specification.product.torqueFail,
                fusionFail = specification.product.fusionFail,
                fusionWarning = specification.product.fusionWarning,
                updateDate = specification.product.updateDate,
            };

            var updatedProduct = await _productRestAPI.UpdateProductAsync(updateProduct);

            Specification updateSpecification = new Specification
            {
                id = specification.id,
                productId = specification.product.id,
                machineId = specification.machine.id,
                inUse = specification.inUse,
                temp = specification.temp,
                load = specification.load,
                rpm = specification.rpm,
            };

            var updatedSpecification = await UpdateSpecificationAsync(updateSpecification);

            if (updatedProduct != null && updatedSpecification != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates or updates a product and its associated specifications (B3, Hapro, Hap2) from an Excel row.
        /// For each machine type, creates a specification object and saves it to the database if data is present.
        /// Logs any exceptions encountered during the process.
        /// </summary>
        /// <param name="row">The Excel row containing product and specification data.</param>
        /// <param name="columnNames">The list of column names for mapping cell values.</param>
        /// <param name="instruments">The list of available instruments (machines).</param>
        public async Task CreateOrUpdateSpecificationFromExcel(IXLRow row, List<string> columnNames, List<MVVM.Models.Instrument> instruments)
        {
            try
            {
                // Create a new Product object
                Product importProduct = CreateProductObject(row, columnNames);

                // Save the product to the database
                var updatedProduct = await CreateOrUpdateProductToDatabase(importProduct);

                // Create a new B3 Specification object if any
                Specification b3Specification = CreateSpecificationObject(row, updatedProduct, "B3", columnNames, instruments);

                // Save the B3 Specification to the database
                await SaveSpecificationToDatabase(b3Specification, updatedProduct, instruments);

                // Create a new Hapro Specification object if any
                Specification haproSpecification = CreateSpecificationObject(row, updatedProduct, "Hapro", columnNames, instruments);

                // Save the Hapro Specification to the database
                await SaveSpecificationToDatabase(haproSpecification, updatedProduct, instruments);

                // Create a new Hap2 Specification object if any
                Specification hap2Specification = CreateSpecificationObject(row, updatedProduct, "Hap2", columnNames, instruments);

                // Save the Hap2 Specification to the database
                await SaveSpecificationToDatabase(hap2Specification, updatedProduct, instruments);
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError(ex);
                return;
            }
        }

        /// <summary>
        /// Creates a Product object from an Excel row using the provided column names.
        /// Maps each relevant cell value to the corresponding Product property, performing type conversions as needed.
        /// </summary>
        /// <param name="row">The Excel row containing product data.</param>
        /// <param name="columnNames">A list of column names to map cell values.</param>
        /// <returns>A populated Product object.</returns>
        private Product CreateProductObject(IXLRow row, List<string> columnNames)
        {
            var product = new Product();
            product.name = row.Cell(columnNames.IndexOf("Product") + 1).Value.ToString();
            product.status = row.Cell(columnNames.IndexOf("InUse") + 1).Value.ToString().ToLower() == "yes" ? true : false;
            product.sampleAmount = double.TryParse(row.Cell(columnNames.IndexOf("Smple") + 1).Value.ToString(), out double sample) ? (double?)sample : null;
            product.comment = row.Cell(columnNames.IndexOf("Comment") + 1).Value.ToString();
            product.dbDate = DateTime.TryParse(row.Cell(columnNames.IndexOf("DB Date\n(6 mths)") + 1).Value.ToString(), out DateTime dbDate) ? (DateTime?)dbDate : null;
            product.coa = !string.IsNullOrWhiteSpace(row.Cell(columnNames.IndexOf("CoA Customer") + 1).Value.ToString().ToLower()) ? true : false;
            product.colour = row.Cell(columnNames.IndexOf("Colour") + 1).Value.ToString();
            product.torqueWarning = double.TryParse(row.Cell(columnNames.IndexOf("Torque warning") + 1).Value.ToString().TrimEnd('%'), out double torqueWarning) ? Math.Round(torqueWarning * 100, 0) : 0;
            product.torqueFail = double.TryParse(row.Cell(columnNames.IndexOf("Torque fail") + 1).Value.ToString().TrimEnd('%'), out double torqueFail) ? Math.Round(torqueFail * 100, 0) : 0;
            product.fusionWarning = double.TryParse(row.Cell(columnNames.IndexOf("Fusion warning") + 1).Value.ToString().TrimEnd('%'), out double fusionWarning) ? Math.Round(fusionWarning * 100, 0) : 0;
            product.fusionFail = double.TryParse(row.Cell(columnNames.IndexOf("Fusion fail") + 1).Value.ToString().TrimEnd('%'), out double fusionFail) ? Math.Round(fusionFail * 100, 0) : 0;
            product.updateDate = DateTime.TryParse(row.Cell(columnNames.IndexOf("Updated") + 1).Value.ToString(), out DateTime updateDate) ? (DateTime?)updateDate : null;

            return product;
        }

        /// <summary>
        /// Creates or updates a product in the database based on its existence.
        /// If a product with the same name exists, updates its properties; otherwise, creates a new product.
        /// </summary>
        /// <param name="importProduct">The Product object to create or update.</param>
        /// <returns>The created or updated Product object from the database.</returns>
        private async Task<Product> CreateOrUpdateProductToDatabase(Product importProduct)
        {
            string productName = importProduct.name;
            string filter = $"?name={productName}";

            // Get the product from the database
            var products = await _productRestAPI.GetProductsAsync(filter);

            // check if the product already exists in the database
            if (products.Count > 0)
            {
                // if yes, Update the existing product's properties and save it in the database
                var existingProduct = products.FirstOrDefault();

                existingProduct.status = importProduct.status;
                existingProduct.sampleAmount = importProduct.sampleAmount;
                existingProduct.comment = importProduct.comment;
                // existingProduct.dbDate = importProduct.dbDate; => use the existing data from database
                existingProduct.coa = importProduct.coa;
                existingProduct.colour = importProduct.colour;
                existingProduct.torqueWarning = importProduct.torqueWarning;
                existingProduct.torqueFail = importProduct.torqueFail;
                existingProduct.fusionWarning = importProduct.fusionWarning;
                existingProduct.fusionFail = importProduct.fusionFail;
                existingProduct.updateDate = importProduct.updateDate;
                existingProduct.dbDate = importProduct.dbDate;

                return await _productRestAPI.UpdateProductAsync(existingProduct);
            }
            else
            {
                // if not, save a new product in the database
                return await _productRestAPI.CreateProductAsync(importProduct);
            }
        }

        /// <summary>
        /// Creates a Specification object from an Excel row for a given machine and product.
        /// Maps temperature, load, and RPM values from the row using the provided column names and instruments list.
        /// </summary>
        /// <param name="row">The Excel row containing specification data.</param>
        /// <param name="product">The associated Product object.</param>
        /// <param name="machineName">The name of the machine (instrument) for which the specification is created.</param>
        /// <param name="columnNames">A list of column names to map cell values.</param>
        /// <param name="instruments">A list of available instruments (machines).</param>
        /// <returns>A populated Specification object.</returns>
        private Specification CreateSpecificationObject(IXLRow row, Product product, string machineName, List<string> columnNames, List<MVVM.Models.Instrument> instruments)
        {
            // search for the machine in the Instruments list

            var specification = new Specification();
            specification.productId = product.id;
            specification.machineId = instruments.FirstOrDefault(i => i.name == machineName).id;
            specification.inUse = product.status;
            specification.temp = int.TryParse(row.Cell(columnNames.IndexOf($"{machineName} Temp C") + 1).Value.ToString(), out int temp) ? temp : 0;
            specification.load = int.TryParse(row.Cell(columnNames.IndexOf($"{machineName} Load (g)") + 1).Value.ToString(), out int load) ? load : 0;
            specification.rpm = int.TryParse(row.Cell(columnNames.IndexOf($"{machineName} RPM") + 1).Value.ToString(), out int rpm) ? rpm : 0;

            return specification;
        }

        /// <summary>
        /// Saves a Specification object to the database.
        /// If a specification for the given product and machine exists, updates it; otherwise, creates a new specification if data is present.
        /// </summary>
        /// <param name="specification">The Specification object to save.</param>
        /// <param name="updatedProduct">The associated Product object.</param>
        /// <param name="instruments">A list of available instruments (machines).</param>
        private async Task SaveSpecificationToDatabase(Specification specification, Product updatedProduct, List<MVVM.Models.Instrument> instruments)
        {
            try
            {
                string productName = updatedProduct.name;
                string machineName = instruments.FirstOrDefault(i => i.id == specification.machineId).name;
                string filter = $"?productName={productName}&machineName={machineName}";

                // check if the specification already exists in the database
                var specifications = await GetAllSpecificationsAsync(filter);

                if (specifications.Count > 0)
                {
                    // if yes, Update the existing specification's properties and save it in the database
                    var existingSpecification = specifications.FirstOrDefault();

                    existingSpecification.productId = existingSpecification.product.id;
                    existingSpecification.machineId = specification.machineId;
                    existingSpecification.temp = specification.temp != 0 ? specification.temp : null;
                    existingSpecification.load = specification.load != 0 ? specification.load : null;
                    existingSpecification.rpm = specification.rpm != 0 ? specification.rpm : null;
                    existingSpecification.inUse = specification.inUse;

                    await UpdateSpecificationAsync(existingSpecification);
                }
                else
                {
                    if (specification.temp == 0 && specification.load == 0 && specification.rpm == 0)
                    {
                        return;
                    }
                    // if not, save a new specification in the database
                    // Save the specification to the database
                    var newSpecification = new Specification();
                    newSpecification.productId = specification.productId;
                    newSpecification.machineId = specification.machineId;
                    newSpecification.inUse = specification.inUse;

                    if (specification.temp != 0)
                    {
                        newSpecification.temp = specification.temp;
                    }

                    if (specification.load != 0)
                    {
                        newSpecification.load = specification.load;
                    }

                    if (specification.rpm != 0)
                    {
                        newSpecification.rpm = specification.rpm;
                    }

                    await CreateSpecificationAsync(newSpecification);
                }
            }
            catch
            {
                throw new Exception("Error saving the specification to the database.");
            }
        }
    }
}
