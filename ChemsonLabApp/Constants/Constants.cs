using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace ChemsonLabApp.Constants
{
    public class Constants
    {
        public static List<string> Status { get; } = new List<string> { "Active", "Inactive" };
        public static List<string> COA { get; } = new List<string> { "COA", "None" };
        public static List<string> TestTypes { get; set; } = new List<string> { "W/U", "STD", "BCH" };
        public static string IPAddress { get; set; }
        public static string CompanyAddress { get; set; }
        public static string COAFilePath { get; set; }
        public static string FromAddress { get; set; }
        public static string Username { get; set; }
        public static string FormulationExcelPath { get; set; }

        public static List<string> ClippingProductName = new List<string> { "Base", "Skin", "CP", "GRX", "GKX", "-", "/", " " };


        // Constant time span for sample's mixing, weighing and so on ( 19 Min/Batch)
        public static TimeSpan SamplePrepareTimeSpan { get; set; } = TimeSpan.FromMinutes(19);

        // local variables
        private static string defaultFormulationExcelPath = "S:\\Production\\Formulations Current\\Product Master Files ACTIVE";

        // Report file path
        public static string ReportFilePath { get; set; } = "S:\\Lab\\Production Quality Control Brabender results";

        public static Dictionary<string, int> SuffixTestAttemptPair { get; set; } = new Dictionary<string, int>
        {
            { "RS", 2 },
            { "RRS", 3 },
            { "RT", 1 },
            { "Remix", 3 },
            { "Rework", 1 },
            { "2.00min", 1},
            { "4.00min", 1 },
            { "Cal", 1 }
        };

        public static List<string> AllowDuplicateBatchNameSuffix { get; set; } = new List<string>
        {
            "Cal",
            "RT",
            "2.00min",
            "4.00min"
        };

        public static List<char> MonthsAlphabets { get; set; } = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M' };

        public static List<string> Months { get; set; } =  new List<string>
        {
            "All",
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec",
        };

        public static void LoadSettingParameters()
        {
            LoadIPAddress();
            LoadEmailAddress();
            LoadCOAFilePath();
            LoadUsername();
            LoadCompanyAddress();
            LoadFormulationExcelFolder();
        }

        private static void LoadIPAddress()
        {
            var settingAPI = new SettingAPI();
            IPAddress = settingAPI.GetIPAddress();
        }

        private static void LoadEmailAddress()
        {
            var settingAPI = new SettingAPI();
            FromAddress = settingAPI.GetEmail();
        }

        private static void LoadCOAFilePath()
        {
            var settingAPI = new SettingAPI();
            COAFilePath = settingAPI.GetCOAPath();
        }

        private static void LoadUsername()
        {
            var settingAPI = new SettingAPI();
            Username = settingAPI.GetUsername();
        }

        private static void LoadCompanyAddress()
        {
            var settingAPI = new SettingAPI();
            CompanyAddress = settingAPI.GetCompanyAddress();
        }

        private static void LoadFormulationExcelFolder()
        {
            var settingAPI = new SettingAPI();
            var formulationExcelPath = settingAPI.GetFormulationExcelFolder();
            if (formulationExcelPath != null)
            {
                FormulationExcelPath = formulationExcelPath;
            }
            else
            {
                settingAPI.CreateFormulationExcelFolder(defaultFormulationExcelPath);
                FormulationExcelPath = settingAPI.GetFormulationExcelFolder();
            }
        }
    }
}
