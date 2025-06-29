﻿using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using ChemsonLabApp.MVVM.Views.Specification;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using PropertyChanged;
using ChemsonLabApp.Services.IService;
using Microsoft.Extensions.DependencyInjection;
using ChemsonLabApp.Services.DialogService;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class EditSpecificationViewModel
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDialogService _dialogService;

        public Specification Specification { get; set; }
        public List<Instrument> Instruments { get; set; } = new List<Instrument>();
        public EditModeToggleCommand EditModeToggleCommand { get; set; }
        public List<string> COAComboBoxItems { get; set; } = new List<string> { "COA", "None" };
        public ContentControl SelectedMode { get; set; }
        public ViewModeToggleCommand ViewModeToggleCommand { get; set; }
        public string EditButtonText { get; set; } = "Edit";
        public DeleteSpecificationViewCommand DeleteSpecificationViewCommand { get; set; }

        public EditSpecificationViewModel(
            IInstrumentService instrumentService,
            IServiceScopeFactory serviceScopeFactory,
            IDialogService dialogService)
        {
            // services
            this._serviceScopeFactory = serviceScopeFactory;
            this._dialogService = dialogService;

            // Commands
            EditModeToggleCommand = new EditModeToggleCommand(this);
            ViewModeToggleCommand = new ViewModeToggleCommand(this);
            DeleteSpecificationViewCommand = new DeleteSpecificationViewCommand(this);
        }

        /// <summary>
        /// Toggles the edit mode for the Specification.
        /// When entering edit mode, updates the UI to allow editing and loads the edit view for the current specification.
        /// When exiting edit mode, reverts the UI to view mode and loads the display view for the current specification.
        /// Updates the Edit button text accordingly.
        /// </summary>
        public void SpecificationEditModeToggle()
        {
            Specification.isEditMode = !Specification.isEditMode;
            Specification.isViewMode = !Specification.isViewMode;

            if (Specification.isEditMode)
            {
                EditButtonText = "Cancel";
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var editSpecificationViewModel = scope.ServiceProvider.GetRequiredService<EditSpecifiationContentControlView>();
                    editSpecificationViewModel.GetSpecificationById(Specification.id);

                    SelectedMode = new EditSpecificationContenControlView(editSpecificationViewModel);
                }
            }
            else
            {
                EditButtonText = "Edit";
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var displaySpecificationViewModel = scope.ServiceProvider.GetRequiredService<DisplaySpecificationContentControlViewModel>();
                    displaySpecificationViewModel.GetSpecificationById(Specification.id);

                    SelectedMode = new DisplaySpecificationContentControlView(displaySpecificationViewModel);
                }
            }
        }

        /// <summary>
        /// Switches the Specification to view mode.
        /// Sets isEditMode to false and isViewMode to true, then loads the display view for the current specification.
        /// </summary>
        public void SpecificationViewModeToggle()
        {
            Specification.isEditMode = false;
            Specification.isViewMode = true;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var displaySpecificationViewModel = scope.ServiceProvider.GetRequiredService<DisplaySpecificationContentControlViewModel>();
                displaySpecificationViewModel.GetSpecificationById(Specification.id);

                SelectedMode = new DisplaySpecificationContentControlView(displaySpecificationViewModel);
            }
        }

        /// <summary>
        /// Opens the delete confirmation dialog for the current Specification.
        /// </summary>
        public void PopupDeleteSpecificationView()
        {
            _dialogService.ShowDeleteView(Specification);
        }
    }
}
