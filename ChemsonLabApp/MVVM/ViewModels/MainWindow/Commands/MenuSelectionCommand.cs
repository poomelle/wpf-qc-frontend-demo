using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.MainWindow.Commands
{
    public class MenuSelectionCommand : ICommand
    {
        public MainWindowViewModel mainWindowViewModel { get; set; }

        public MenuSelectionCommand(MainWindowViewModel vm)
        {
            mainWindowViewModel = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Menu menu)
            {
                if (menu.SubMenus.Count > 0)
                {
                    mainWindowViewModel.ShowHideSubMenu(menu);
                }
                else
                {
                    mainWindowViewModel.MenuSelected(menu.MenuTitle);
                    mainWindowViewModel.HideAllSubMenu();
                }
            }
            else if (parameter is SubMenu subMenu)
            {
                mainWindowViewModel.MenuSelected(subMenu.SubMenuTitle);
            }
        }
    }
}
