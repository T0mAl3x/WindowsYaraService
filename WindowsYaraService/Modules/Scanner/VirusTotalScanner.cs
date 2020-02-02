using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Modules.Scanner;
using WindowsYaraService.Modules.Scanner.Models;

namespace WindowsYaraService.Modules
{
    class VirusTotalScanner
    {
        private VirusTotal mVirusTotal = new VirusTotal("36e5b3febf6ec4ecd4e0f9440bb82ba80a47b894e8cb8ca49bea6736118154fd");

        public VirusTotalScanner()
        {
            mVirusTotal.UseTLS = true;
        }

        public async Task<InfoModel> ScanFile(ScanJob scanJob)
        {
            //Create the EICAR test virus. See http://www.eicar.org/86-0-Intended-use.html
            //byte[] eicar = Encoding.ASCII.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

            //If the file has been scanned before, the results are embedded inside the report.
            //if (hasFileBeenScannedBefore)
            //{
            //    PrintScan(fileReport);
            //}
            //else
            //{
            FileReport fileReport = null;
            if (scanJob.GetSize() < 33553369 && scanJob.GetSize() > 0)
            {
                try
                {
                    byte[] file = File.ReadAllBytes(scanJob.mFilePath);
                    ScanResult fileResult = await mVirusTotal.ScanFileAsync(file, scanJob.mFilePath);
                    fileReport = await mVirusTotal.GetFileReportAsync(fileResult.SHA256);
                }
                catch(Exception ex)
                {

                }
            }

            //    PrintScan(fileResult);
            //}

            //Console.WriteLine();

            //string scanUrl = "http://www.google.com/";

            //UrlReport urlReport = await mVirusTotal.GetUrlReportAsync(scanUrl);

            //bool hasUrlBeenScannedBefore = urlReport.ResponseCode == UrlReportResponseCode.Present;
            //Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));

            ////If the url has been scanned before, the results are embedded inside the report.
            //if (hasUrlBeenScannedBefore)
            //{
            //    PrintScan(urlReport);
            //}
            //else
            //{
            //    UrlScanResult urlResult = await mVirusTotal.ScanUrlAsync(scanUrl);
            //    PrintScan(urlResult);
            //}
            InfoModel infoModel;
            if (fileReport != null)
            {
                infoModel = new InfoModel
                {
                    ScandId = fileReport.ScanId,
                    Date = fileReport.ScanDate,
                    SHA1 = fileReport.SHA1,
                    SHA256 = fileReport.SHA256,
                    FilePath = scanJob.mFilePath,
                    Positives = fileReport.Positives,
                    Total = fileReport.Total
                };
                List<Scan> Scans = new List<Scan>();
                foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
                {
                    var scanModel = new Scan
                    {
                        EngineName = scan.Key,
                        Detected = scan.Value.Detected,
                        Version = scan.Value.Version,
                        Result = scan.Value.Result
                    };
                    Scans.Add(scanModel);
                }
                infoModel.Scans = Scans;
            }
            else
            {
                infoModel = new InfoModel();
                infoModel.Messages.Add("The file is too large for Virus Total, has 0 bytes or license limit.");
            }
            return infoModel;
        }

        //private static void PrintScan(UrlScanResult scanResult)
        //{
        //    EventLog.WriteEntry("Application", "Scan ID: " + scanResult.ScanId, EventLogEntryType.Information);
        //    EventLog.WriteEntry("Application", "Message: " + scanResult.VerboseMsg, EventLogEntryType.Information);
        //}

        //private static void PrintScan(ScanResult scanResult)
        //{
        //    EventLog.WriteEntry("Application", "Scan ID: " + scanResult.ScanId, EventLogEntryType.Information);
        //    EventLog.WriteEntry("Application", "Message: " + scanResult.VerboseMsg, EventLogEntryType.Information);
        //}

        //private static void PrintScan(FileReport fileReport)
        //{
        //    EventLog.WriteEntry("Application", "Scan ID: " + fileReport.ScanId, EventLogEntryType.Information);
        //    EventLog.WriteEntry("Application", "Message: " + fileReport.VerboseMsg, EventLogEntryType.Information);

        //    if (fileReport.ResponseCode == FileReportResponseCode.Present)
        //    {
        //        foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
        //        {
        //            EventLog.WriteEntry("Application", scan.Key + " Detected: " + scan.Value.Detected, EventLogEntryType.Information);
        //        }
        //    }
        //}

        //private static void PrintScan(UrlReport urlReport)
        //{
        //    EventLog.WriteEntry("Application", "Scan ID: " + urlReport.ScanId, EventLogEntryType.Information);
        //    EventLog.WriteEntry("Application", "Message: " + urlReport.VerboseMsg, EventLogEntryType.Information);

        //    if (urlReport.ResponseCode == UrlReportResponseCode.Present)
        //    {
        //        foreach (KeyValuePair<string, UrlScanEngine> scan in urlReport.Scans)
        //        {
        //            EventLog.WriteEntry("Application", scan.Key + " Detected: " + scan.Value.Detected, EventLogEntryType.Information);
        //        }
        //    }
        //}
    }
}
