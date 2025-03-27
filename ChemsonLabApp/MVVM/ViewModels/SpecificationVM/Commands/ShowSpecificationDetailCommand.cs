using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands
{
    public class ShowSpecificationDetailCommand : ICommand
    {
        SpecificationViewModel viewModel;
        public event EventHandler CanExecuteChanged;
        public ShowSpecificationDetailCommand(SpecificationViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var specification = parameter as Specification;
            if (specification != null)
            {
                viewModel.PopupEditSpecificationView(specification);
            }
        }
    }
}
