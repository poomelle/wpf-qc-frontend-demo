using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public interface IDataLoaderService
    {
        TestResult ProcessMTFTestResults(OleDbConnection connection, string fileName, List<Product> products, List<MVVM.Models.Instrument> instruments);
        List<Evaluation> ProcessMTFEvaluations(OleDbConnection connection, string fileName);
        List<Measurement> ProcessMTFMeasurements(OleDbConnection connection, string fileName);
        Task<List<TestResult>> AssignDefaultValuesAndArrangeResults(List<TestResult> testResults);
        List<TestResult> AutoReBatchName(List<TestResult> testResults, bool isTwoX);
        TestResult ProcessTxtTestResult(string file, List<Product> products, List<MVVM.Models.Instrument> instruments);
        (List<Measurement> Measurements, List<int> Times, List<double> Torques) ProcessTXTMeasurement(string file);
        List<Evaluation> ProcessTXTEvaluation(string file, List<int> times, List<double> torques);
        Task<bool> SavingDataLoader(List<TestResult> testResults, List<Evaluation> evaluations, List<Measurement> measurements);
        
    }
}
