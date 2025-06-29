﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class AddSpecificationViewModel
    {
        private readonly ISpecificationService _specificationService;
        private readonly IProductService _productService;
        private readonly IInstrumentService _instrumentService;

        public List<Specification> Specifications { get; set; } = new List<Specification>();
        public Specification NewSpecification { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public Product SelectedProduct { get; set; }
        public Product NewCreateProduct { get; set; }
        public string NewCreateProductName { get; set; }
        public List<Instrument> Instruments { get; set; } = new List<Instrument>();
        public Instrument SelectedInstrument { get; set; }
        public int? NewTemp { get; set; }
        public int? NewLoad { get; set; }
        public int? NewRPM { get; set; }
        public double? NewTorqueWarning { get; set; }
        public double? NewTorqueFail { get; set; }
        public double? NewFusionWarning { get; set; }
        public double? NewFusionFail { get; set; }
        public string NewColour { get; set; }
        public bool UseExistingProduct { get; set; } = true;
        public DateTime? NewUpdateDate { get; set; } = DateTime.Now;
        public double? NewSampleAmount { get; set; }
        public string NewComment { get; set; }
        public DateTime? NewDbDate { get; set; } = DateTime.Now;
        public bool SelectedCOA { get; set; } = false;
        public List<string> COAComboBoxItems { get; set; } = new List<string> { "COA", "None" };
        public bool isAddingSampleOrCOA { get; set; } = false;
        public SaveNewSpecificationCommand SaveNewSpecificationCommand { get; set; }
        public OnExistingProductSelection OnExistingProductSelection { get; set; }

        public AddSpecificationViewModel(
            ISpecificationService specificationService,
            IProductService productService,
            IInstrumentService instrumentService
            )
        {
            // services
            this._specificationService = specificationService;
            this._productService = productService;
            this._instrumentService = instrumentService;

            // commands
            SaveNewSpecificationCommand = new SaveNewSpecificationCommand(this);
            OnExistingProductSelection = new OnExistingProductSelection(this);

            // initialize
            InitializeParameters();
        }

        /// <summary>
        /// Initializes the parameters for the AddSpecificationViewModel by loading active products, all specifications, and all active instruments asynchronously.
        /// </summary>
        private async void InitializeParameters()
        {
            Products = await _productService.LoadActiveProducts();
            Specifications = await _specificationService.GetAllSpecificationsAsync();
            Instruments = await _instrumentService.GetAllActiveInstrument();
        }

        /// <summary>
        /// Saves a new specification by either updating an existing product or creating a new product,
        /// along with the associated specification. Validates required fields, handles errors, and displays
        /// notifications based on the result. Shows a busy cursor during the operation.
        /// </summary>
        public async void SaveNewSpecification()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var isSuccessCreation = false;

                if (!InputValidationUtility.ValidateNotNullObject(SelectedInstrument, "Instrument")) return;

                if (UseExistingProduct)
                {
                    if (!InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product")) return;
                    var updateProduct = GetProductObject();
                    updateProduct.id = SelectedProduct.id;
                    updateProduct.name = SelectedProduct.name;
                    updateProduct.status = SelectedProduct.status;

                    var createSpecification = GetCreateSpecificationObject();
                    isSuccessCreation = await _specificationService.CreateSpecificationAndUpdateProduct(updateProduct, createSpecification);
                }
                else
                {
                    if (!InputValidationUtility.ValidateNotNullInput(NewCreateProductName, "Product Name")) return;
                    var createProduct = GetProductObject();
                    createProduct.name = NewCreateProductName;
                    createProduct.status = true;

                    var createSpecification = GetCreateSpecificationObject();
                    isSuccessCreation = await _specificationService.CreateSpecificationAndCreateProduct(createProduct, createSpecification);
                }

                if (isSuccessCreation)
                {
                    NotificationUtility.ShowSuccess("New Specification Created Successfully");
                }
                else
                {
                    NotificationUtility.ShowWarning("Error Creating New Specification");
                }
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error Creating New Specification");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error Creating New Specification");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Specification"/> object using the current values from the view model.
        /// Sets the machine ID, usage status, temperature, load, and RPM for the specification.
        /// </summary>
        /// <returns>A new <see cref="Specification"/> instance populated with the relevant data.</returns>
        private Specification GetCreateSpecificationObject()
        {
            return new Specification
            {
                machineId = SelectedInstrument.id,
                inUse = true,
                temp = NewTemp,
                load = NewLoad,
                rpm = NewRPM,
            };
        }

        /// <summary>
        /// Creates and returns a new <see cref="Product"/> object populated with the current values
        /// from the view model's product-related properties. This method is used to construct a product
        /// instance for saving or updating operations, ensuring all relevant fields are set.
        /// </summary>
        /// <returns>A new <see cref="Product"/> instance with the specified property values.</returns>
        public Product GetProductObject()
        {
            return new Product
            {
                dbDate = NewDbDate,
                sampleAmount = NewSampleAmount,
                comment = NewComment,
                coa = SelectedCOA,
                colour = NewColour,
                torqueWarning = NewTorqueWarning,
                torqueFail = NewTorqueFail,
                fusionWarning = NewFusionWarning,
                fusionFail = NewFusionFail,
                updateDate = NewUpdateDate,
            };
        }

        /// <summary>
        /// Populates the view model's fields with data from the currently selected product.
        /// This method copies relevant properties from <see cref="SelectedProduct"/> to the corresponding
        /// fields in the view model, allowing the UI to display and edit the existing product's data.
        /// </summary>
        public void ShowExistingProductData()
        {
            if (SelectedProduct != null)
            {
                NewDbDate = SelectedProduct.dbDate == null ? null : SelectedProduct.dbDate;
                NewSampleAmount = SelectedProduct.sampleAmount == null ? null : SelectedProduct.sampleAmount;
                NewComment = SelectedProduct.comment == null ? "" : SelectedProduct.comment;
                SelectedCOA = SelectedProduct.coa == null ? SelectedCOA : (bool)SelectedProduct.coa;

                NewColour = SelectedProduct.colour == null ? null : SelectedProduct.colour;
                NewTorqueWarning = SelectedProduct.torqueWarning == null ? null : SelectedProduct.torqueWarning;
                NewFusionWarning = SelectedProduct.fusionWarning == null ? null : SelectedProduct.fusionWarning;
                NewTorqueFail = SelectedProduct.torqueFail == null ? null : SelectedProduct.torqueFail;
                NewFusionFail = SelectedProduct.fusionFail == null ? null : SelectedProduct.fusionFail;
                NewUpdateDate = SelectedProduct.updateDate == null ? null : SelectedProduct.updateDate;
            }
        }
    }
}
