using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.ViewModels.SettingVM.Command;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.SettingVM
{
    [AddINotifyPropertyChangedInterface]
    public class SettingViewModel
    {
        public string IPAddress { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string COAPath { get; set; }
        public string CompanyAddress { get; set; }
        public string FormulationActiveFolder { get; set; }
        public OnSaveCommand OnSaveCommand { get; set; }
        public OnCancelCommand OnCancelCommand { get; set; }


        public SettingViewModel()
        {
            InitializeParameters();
            OnSaveCommand = new OnSaveCommand(this);
            OnCancelCommand = new OnCancelCommand(this);
        }

        private void InitializeParameters()
        {
            var settingAPI = new SettingAPI();
            IPAddress = settingAPI.GetIPAddress();
            Username = settingAPI.GetUsername();
            Email = settingAPI.GetEmail();
            COAPath = settingAPI.GetCOAPath();
            CompanyAddress = settingAPI.GetCompanyAddress();
            FormulationActiveFolder = settingAPI.GetFormulationExcelFolder();
        }

        public void SaveNewSettingValues()
        {
            try
            {
                var settingAPI = new SettingAPI();
                settingAPI.UpdateIPAddress(IPAddress);
                settingAPI.UpdateUsername(Username);
                settingAPI.UpdateEmail(Email);
                settingAPI.UpdateCOAPath(COAPath);
                settingAPI.UpdateCompanyAddress(CompanyAddress);
                settingAPI.UpdateFormulationExcelFolder(FormulationActiveFolder);

                InitializeParameters();
                Constants.Constants.LoadSettingParameters();

                MessageBox.Show($"New Setting has been saved", "Saved!", MessageBoxButton.OK, MessageBoxImage.Warning);

            }catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: unable to save a new setting value. Please try again later.");
                LoggerUtility.LogError(ex);
            }
        }

        public void UnsavedNewSettingValue()
        {
            InitializeParameters();
        }
    }
}
