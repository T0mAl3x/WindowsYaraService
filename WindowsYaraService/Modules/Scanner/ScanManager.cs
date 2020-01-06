using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Modules.Scanner.Models;

namespace WindowsYaraService.Modules.Scanner
{
    public class ScanManager
    {
        private readonly YaraScanner YaraScanner;
        private readonly VirusTotalScanner VirusTotalScanner;

        public ScanManager(string rulesPath)
        {
            // Initialise Scanners
            YaraScanner = new YaraScanner(rulesPath);
            VirusTotalScanner = new VirusTotalScanner();
        }
        public void ScanFile(string filePath)
        {
            ThreadPool.QueueUserWorkItem(async i =>
            {
                var scanJob = new ScanJob(filePath);
                Task<InfoModel> infoModel = VirusTotalScanner.ScanFile(scanJob);
                List<YaraResult> yaraResults = YaraScanner.ScanFile(scanJob);
                InfoModel model = await infoModel;
            });
        }
    }
}
