using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Modules.Scanner;

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
            byte[] eicar = Encoding.ASCII.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

            //Check if the file has been scanned before.
            FileReport fileReport = await mVirusTotal.GetFileReportAsync(eicar);

            bool hasFileBeenScannedBefore = fileReport.ResponseCode == FileReportResponseCode.Present;

            Console.WriteLine("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));

            //If the file has been scanned before, the results are embedded inside the report.
            if (hasFileBeenScannedBefore)
            {
                PrintScan(fileReport);
            }
            else
            {
                ScanResult fileResult = await mVirusTotal.ScanFileAsync(eicar, "EICAR.txt");
                PrintScan(fileResult);
            }

            Console.WriteLine();

            string scanUrl = "http://www.google.com/";

            UrlReport urlReport = await mVirusTotal.GetUrlReportAsync(scanUrl);

            bool hasUrlBeenScannedBefore = urlReport.ResponseCode == UrlReportResponseCode.Present;
            Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));

            //If the url has been scanned before, the results are embedded inside the report.
            if (hasUrlBeenScannedBefore)
            {
                PrintScan(urlReport);
            }
            else
            {
                UrlScanResult urlResult = await mVirusTotal.ScanUrlAsync(scanUrl);
                PrintScan(urlResult);
            }

            return new InfoModel();
        }

        private static void PrintScan(UrlScanResult scanResult)
        {
            EventLog.WriteEntry("Application", "Scan ID: " + scanResult.ScanId, EventLogEntryType.Information);
            EventLog.WriteEntry("Application", "Message: " + scanResult.VerboseMsg, EventLogEntryType.Information);
        }

        private static void PrintScan(ScanResult scanResult)
        {
            EventLog.WriteEntry("Application", "Scan ID: " + scanResult.ScanId, EventLogEntryType.Information);
            EventLog.WriteEntry("Application", "Message: " + scanResult.VerboseMsg, EventLogEntryType.Information);
        }

        private static void PrintScan(FileReport fileReport)
        {
            EventLog.WriteEntry("Application", "Scan ID: " + fileReport.ScanId, EventLogEntryType.Information);
            EventLog.WriteEntry("Application", "Message: " + fileReport.VerboseMsg, EventLogEntryType.Information);

            if (fileReport.ResponseCode == FileReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
                {
                    EventLog.WriteEntry("Application", scan.Key + " Detected: " + scan.Value.Detected, EventLogEntryType.Information);
                }
            }
        }

        private static void PrintScan(UrlReport urlReport)
        {
            EventLog.WriteEntry("Application", "Scan ID: " + urlReport.ScanId, EventLogEntryType.Information);
            EventLog.WriteEntry("Application", "Message: " + urlReport.VerboseMsg, EventLogEntryType.Information);

            if (urlReport.ResponseCode == UrlReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, UrlScanEngine> scan in urlReport.Scans)
                {
                    EventLog.WriteEntry("Application", scan.Key + " Detected: " + scan.Value.Detected, EventLogEntryType.Information);
                }
            }
        }
    }
}
