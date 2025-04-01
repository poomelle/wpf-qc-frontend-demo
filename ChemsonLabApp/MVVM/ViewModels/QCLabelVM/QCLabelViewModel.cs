using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM.Command;
using ChemsonLabApp.MVVM.Views.QCLabel;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.QCLabelVM
{
    [AddINotifyPropertyChangedInterface]
    public class QCLabelViewModel
    {
        private readonly IQcLabelService _qcLabelService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<QCLabel> QCLabels { get; set; } = new ObservableCollection<QCLabel>();
        public Product SelectedProduct { get; set; }
        public string BulkWeight { get; set; }
        public string PaperBagWeight { get; set; }
        public string BatchNumberStart { get; set; }
        public string BatchNumberEnd { get; set; }
        public string InputWeight { get; set; }
        public bool IsSelectedPaperBagWeight { get; set; }
        public ProductSelectionChangedCommand ProductSelectionChangedCommand { get; set; }
        public InputWeightToggleCommand InputWeightToggleCommand { get; set; }
        public AddQCLabelCommand AddQCLabelCommand { get; set; }
        public BatchNameToggleCommand BatchNameToggleCommand { get; set; }
        public RemoveQCLabelCommand RemoveQCLabelCommand { get; set; }
        public MakeQCLabelCommand MakeQCLabelCommand { get; set; }
        public ClearQCLabelsCommand ClearQCLabelsCommand { get; set; }
        public FromBatchChangeQCLabelCommand FromBatchChangeQCLabelCommand { get; set; }
        public ToBatchChangeQCLabelCommand ToBatchChangeQCLabelCommand { get; set; }

        public QCLabelViewModel(
            IQcLabelService qcLabelService,
            IDialogService dialogService
            )
        {
            // services
            this._qcLabelService = qcLabelService;
            this._dialogService = dialogService;

            // commands
            ProductSelectionChangedCommand = new ProductSelectionChangedCommand(this);
            InputWeightToggleCommand = new InputWeightToggleCommand(this);
            AddQCLabelCommand = new AddQCLabelCommand(this);
            BatchNameToggleCommand = new BatchNameToggleCommand(this);
            RemoveQCLabelCommand = new RemoveQCLabelCommand(this);
            MakeQCLabelCommand = new MakeQCLabelCommand(this);
            ClearQCLabelsCommand = new ClearQCLabelsCommand(this);
            FromBatchChangeQCLabelCommand = new FromBatchChangeQCLabelCommand(this);
            ToBatchChangeQCLabelCommand = new ToBatchChangeQCLabelCommand(this);
        }

        public void ProductSelectionChanged(Product product)
        {
            //if (SelectedProduct == null) return;

            SelectedProduct = product;

            var bulkWeight = SelectedProduct.bulkWeight;
            var paperBagWeight = SelectedProduct.paperBagWeight;
            var paperBagNo = SelectedProduct.paperBagNo;

            BulkWeight = bulkWeight != null ? bulkWeight.ToString() : "";
            PaperBagWeight = paperBagWeight != null && paperBagNo != null ? $"{paperBagNo} x {paperBagWeight}" : "";

            if (!string.IsNullOrWhiteSpace(PaperBagWeight))
            {
                InputWeight = PaperBagWeight;
                IsSelectedPaperBagWeight = true;
            }
            else if (!string.IsNullOrWhiteSpace(BulkWeight))
            {
                InputWeight = BulkWeight;
                IsSelectedPaperBagWeight = false;
            }
            else
            {
                InputWeight = PaperBagWeight;
                IsSelectedPaperBagWeight = true;
            }
        }

        // Toggle weight between bulk and paper bag function
        public void InputWeightToggle()
        {
            if (IsSelectedPaperBagWeight)
            {
                InputWeight = BulkWeight;
                IsSelectedPaperBagWeight = false;
            }
            else
            {
                InputWeight = PaperBagWeight;
                IsSelectedPaperBagWeight = true;
            }
        }

        // populate QCLabel data to ObservableCollection QCLabels
        public void AddQCLabel()
        {
            var qCLabels = _qcLabelService.PopulateQcLabels(SelectedProduct, BatchNumberStart, BatchNumberEnd, InputWeight);

            if (qCLabels != null)
            {
                foreach (var qcLabel in qCLabels)
                {
                    QCLabels.Add(qcLabel);
                }
            }
        }

        // Batch Name Toggle function
        public void BatchNameToggle(QCLabel qcLabel)
        {
            string batchName = GetNewBatchName(qcLabel);

            // Change batch name in ObservableCollection QCLabels
            var index = QCLabels.IndexOf(qcLabel);
            qcLabel.batchName = batchName;
            QCLabels[index] = qcLabel;
        }

        private string GetNewBatchName(QCLabel qcLabel)
        {
            var batchName = qcLabel.batchName;

            if (batchName.Contains("-"))
            {
                batchName = batchName.Substring(0, batchName.IndexOf("-") - 1).Trim();
            }
            else
            {
                var batchNamePrefix = batchName.Substring(0, 3);
                var batchNameNumber = int.Parse(batchName.Substring(3));
                var nextBatchNameNumber = batchNameNumber + 1;
                batchName = $"{batchNamePrefix}{batchNameNumber} - {nextBatchNameNumber}";
            }

            return batchName;
        }

        public void RemoveQCLabel(QCLabel qcLabel)
        {
            QCLabels.Remove(qcLabel);
        }

        public void ClearQCLabels()
        {
            QCLabels.Clear();
        }

        public void MakeQCLabels()
        {
            _dialogService.ShowMakeQcLabels(QCLabels.ToList());
        }
    }
}
