using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace ChemsonLabApp.RestAPI
{
    public class SettingAPI
    {

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private string xmlFilePath;

        public SettingAPI()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(userProfile , "LabAppSettingData.xml");
            SetXMLFilePath(filePath);
        }

        private void SetXMLFilePath(string deployPath)
        {
            if (Debugger.IsAttached)
            {
                // Development mode
                xmlFilePath = Path.Combine(baseDirectory, @"..\..\Data\SettingData.xml");
            }
            else
            {
                // Deployed mode
                xmlFilePath = deployPath;
            }
        }

        private XDocument LoadXMLDoc()
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc;
        }

        public string GetIPAddress()
        {
            //var doc = LoadXMLDoc();
            //string ipAddress = doc.Descendants("IPAddress").FirstOrDefault().Value;
            //if (doc != null)
            //{
            //    return ipAddress;
            //}
            //return null;

            return "https://localhost:7243/api";
        }

        public void UpdateIPAddress(string newIPAddress)
        {
            var doc = LoadXMLDoc();
            var ipAddressElement = doc.Descendants("IPAddress").FirstOrDefault();
            if (ipAddressElement != null)
            {
                ipAddressElement.Value = newIPAddress;
                doc.Save(xmlFilePath);
            }
        }

        public string GetCOAPath()
        {
            var doc = LoadXMLDoc();
            string coaPath = doc.Descendants("COAPath").FirstOrDefault().Value;
            if (coaPath != null)
            {
                return coaPath;
            }
            return null;
        }

        public void UpdateCOAPath(string newCOAPath)
        {
            var doc = LoadXMLDoc();
            var coaPathElement = doc.Descendants("COAPath").FirstOrDefault();
            if (coaPathElement != null)
            {
                coaPathElement.Value = newCOAPath;
                doc.Save(xmlFilePath);
            }
        }

        public string GetUsername()
        {
            var doc = LoadXMLDoc();
            string username = doc.Descendants("Username").FirstOrDefault().Value;
            if (username != null)
            {
                return username;
            }
            return null;
        }

        public void UpdateUsername(string newUsername)
        {
            var doc = LoadXMLDoc();
            var usernameElement = doc.Descendants("Username").FirstOrDefault();
            if (usernameElement != null)
            {
                usernameElement.Value = newUsername;
                doc.Save(xmlFilePath);
            }
        }

        public string GetEmail()
        {
            var doc = LoadXMLDoc();
            string email = doc.Descendants("Email").FirstOrDefault().Value;
            if (email != null)
            {
                return email;
            }
            return null;
        }

        public void UpdateEmail(string newEmail)
        {
            var doc = LoadXMLDoc();
            var emailElement = doc.Descendants("Email").FirstOrDefault();

            if (emailElement != null)
            {
                emailElement.Value = newEmail;
                doc.Save(xmlFilePath);
            }
        }

        public string GetCompanyAddress()
        {
            var doc = LoadXMLDoc();
            string companyAddress = doc.Descendants("CompanyAddress").FirstOrDefault().Value;
            if (companyAddress != null)
            {
                return companyAddress;
            }
            return null;
        }

        public void UpdateCompanyAddress(string newAddress)
        {
            var doc = LoadXMLDoc();
            var companyAddressElement = doc.Descendants("CompanyAddress").FirstOrDefault();
            if (companyAddressElement != null)
            {
                companyAddressElement.Value = newAddress;
                doc.Save(xmlFilePath);
            }
        }

        public string GetFormulationExcelFolder()
        {
            var doc = LoadXMLDoc();
            string formulationExcelFolder = doc.Descendants("FormulationExcelFolder")?.FirstOrDefault()?.Value ?? null;
            if (formulationExcelFolder != null)
            {
                return formulationExcelFolder;
            }
            return null;
        }

        // Update FormulationExcelFolder
        public void UpdateFormulationExcelFolder(string newFolder)
        {
            var doc = LoadXMLDoc();
            var formulationExcelFolderElement = doc.Descendants("FormulationExcelFolder").FirstOrDefault();
            if (formulationExcelFolderElement != null)
            {
                formulationExcelFolderElement.Value = newFolder;
                doc.Save(xmlFilePath);
            }
        }

        // Create FormulationExcelFolder
        public void CreateFormulationExcelFolder(string newFolder)
        {
            var doc = LoadXMLDoc();
            var formulationExcelFolderElement = doc.Descendants("FormulationExcelFolder").FirstOrDefault();
            if (formulationExcelFolderElement == null)
            {
                doc.Element("Setting").Add(new XElement("FormulationExcelFolder", newFolder));
                doc.Save(xmlFilePath);
            }
        }
    }
}
