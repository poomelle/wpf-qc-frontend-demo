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

        /// <summary>
        /// Sets the XML file path for settings data depending on whether the application is running in development or deployed mode.
        /// </summary>
        /// <param name="deployPath">The path to use when in deployed mode.</param>
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

        /// <summary>
        /// Loads the settings XML document from the configured file path.
        /// </summary>
        /// <returns>The loaded XDocument instance.</returns>
        private XDocument LoadXMLDoc()
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc;
        }

        /// <summary>
        /// Gets the API base IP address.
        /// </summary>
        /// <returns>The API base IP address as a string.</returns>
        public string GetIPAddress()
        {
            return "https://localhost:7243/api";
        }

        /// <summary>
        /// Updates the IP address in the settings XML file.
        /// </summary>
        /// <param name="newIPAddress">The new IP address to set.</param>
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

        /// <summary>
        /// Gets the COA (Certificate of Analysis) path from the settings XML file.
        /// </summary>
        /// <returns>The COA path as a string, or null if not found.</returns>
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

        /// <summary>
        /// Updates the COA path in the settings XML file.
        /// </summary>
        /// <param name="newCOAPath">The new COA path to set.</param>
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

        /// <summary>
        /// Gets the username from the settings XML file.
        /// </summary>
        /// <returns>The username as a string, or null if not found.</returns>
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

        /// <summary>
        /// Updates the username in the settings XML file.
        /// </summary>
        /// <param name="newUsername">The new username to set.</param>
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

        /// <summary>
        /// Gets the email address from the settings XML file.
        /// </summary>
        /// <returns>The email address as a string, or null if not found.</returns>
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

        /// <summary>
        /// Updates the email address in the settings XML file.
        /// </summary>
        /// <param name="newEmail">The new email address to set.</param>
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

        /// <summary>
        /// Gets the company address from the settings XML file.
        /// </summary>
        /// <returns>The company address as a string, or null if not found.</returns>
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

        /// <summary>
        /// Updates the company address in the settings XML file.
        /// </summary>
        /// <param name="newAddress">The new company address to set.</param>
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

        /// <summary>
        /// Gets the folder path for formulation Excel files from the settings XML file.
        /// </summary>
        /// <returns>The folder path as a string, or null if not found.</returns>
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

        /// <summary>
        /// Updates the folder path for formulation Excel files in the settings XML file.
        /// </summary>
        /// <param name="newFolder">The new folder path to set.</param>
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

        /// <summary>
        /// Creates a new FormulationExcelFolder element in the settings XML file if it does not already exist.
        /// </summary>
        /// <param name="newFolder">The folder path to add.</param>
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
