⚠️ DEMO APPLICATION - This is a demonstration project and may not run in all environments.

A comprehensive laboratory management system built with WPF (.NET Framework 4.7.2) using MVVM architecture for chemical analysis and quality control operations.

📋 Overview

ChemsonLabApp is designed for chemical laboratory operations, providing tools for:

•	Quality Control (QC) label management
•	Certificate of Analysis (COA) generation
•	Batch testing and data loading
•	Product and specification management
•	Customer order tracking
•	Instrument management
•	Daily QC operations
•	Reporting and analytics with graph visualization

🏗️ Architecture
•	Framework: .NET Framework 4.7.2
•	UI Framework: WPF (Windows Presentation Foundation)
•	Pattern: MVVM (Model-View-ViewModel)
•	Dependency Injection: Microsoft.Extensions.DependencyInjection
•	Testing: MSTest with Moq for unit testing
•	Data Binding: PropertyChanged.Fody for automatic INotifyPropertyChanged implementation
•	Charting: LiveCharts.Wpf for data visualization

🚀 Features
Core Modules
•	Data Loader: Import and manage test data with search, edit, and delete capabilities
•	QC Labels: Generate and manage quality control labels for batches
•	COA Management: Create and manage Certificates of Analysis
•	Reporting: Generate detailed reports with torque graphs and analytics
•	Product Management: Handle product specifications and configurations
•	Customer Management: Track customer orders and information
•	Instrument Management: Monitor and configure testing instruments
•	Daily QC: Perform daily quality control operations
Technical Features
•	REST API integration for backend communication
•	Email services integration (Outlook)
•	PDF generation capabilities
•	Real-time data visualization with charts
•	Comprehensive unit test coverage
•	Dialog service for UI interactions
•	Logging and error handling utilities
🛠️ Dependencies
Key NuGet Packages
•	Microsoft.Extensions.DependencyInjection
•	PropertyChanged.Fody
•	LiveCharts.Wpf
•	MSTest.TestFramework
•	Moq (for testing)

📁 Project Structure
ChemsonLabApp/
├── MVVM/
│   ├── Models/          # Data models
│   ├── ViewModels/      # View models for MVVM
│   └── Views/           # WPF user controls and windows
├── Services/            # Business logic services
├── RestAPI/             # API communication layer
├── Utilities/           # Helper utilities
└── Constants/           # Application constants

ChemsonLabApp.Tests/
└── ServiceTests/        # Unit tests for services


🔧 Setup (Demo)
Note: This is a demonstration application and may require additional configuration to run properly.
Prerequisites
•	Windows 10/11
•	.NET Framework 4.7.2 or higher
•	Visual Studio 2022 (recommended)
Build Instructions
1.	Clone the repository
2.	Open ChemsonLabApp.sln in Visual Studio
3.	Restore NuGet packages
4.	Build the solution
5.	Configure the API endpoint in the settings file if needed

🧪 Testing
The solution includes comprehensive unit tests covering:
•	Service layer functionality
•	API integration mocking
•	Data manipulation operations
•	Business logic validation
Test coverage includes:
•	QcLabelServiceTests
•	CoaServiceTests
•	ProductServiceTests
•	CustomerServiceTests
•	ReportServiceTests
•	And many more...

📊 Key Components
Services
•	QcLabelService: Manages QC label creation and batch operations
•	CoaService: Handles Certificate of Analysis generation
•	DataLoaderService: Manages test data import and processing
•	ReportService: Generates analytical reports and graphs
•	ProductService: Manages product specifications
•	CustomerService: Handles customer and order management
Models
•	QCLabel: Quality control label information
•	Product: Product specifications and details
•	BatchTestResult: Test results for product batches
•	Customer: Customer information and orders
•	Specification: Testing specifications and conditions
🔍 Demo Limitations
As this is a demo application:
•	Backend API may not be accessible
•	Database connections may not be configured
•	Some features may require additional setup
•	Network paths may need adjustment
•	Email services may require configuration
