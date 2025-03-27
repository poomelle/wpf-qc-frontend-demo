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

        public async Task<Specification> CreateSpecificationAsync(Specification specification)
        {
            return await _specificationRestAPI.CreateSpecificationAsync(specification);
        }

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

        public async Task<Specification> DeleteSpecificationAsync(Specification specification, string deleteConfirm)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirm)) return null;
            return await _specificationRestAPI.DeleteSpecificationAsync(specification);
        }

        public async Task<List<Specification>> GetAllSpecificationsAsync(string filter = "", string sort = "")
        {
            return await _specificationRestAPI.GetAllSpecificationsAsync(filter, sort);
        }

        public async Task<List<Specification>> GetAllActiveSpecificationsAsync()
        {
            string filter = "?inUse=true";
            string sort = "&sortBy=productName&isAscending=true";

            return await GetAllSpecificationsAsync(filter, sort);
        }

        public async Task<Specification> GetSpecificationByIdAsync(int id)
        {
            return await _specificationRestAPI.GetSpecificationByIdAsync(id);
        }

        public async Task<Specification> UpdateSpecificationAsync(Specification specification)
        {
            return await _specificationRestAPI.UpdateSpecificationAsync(specification);
        }

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
