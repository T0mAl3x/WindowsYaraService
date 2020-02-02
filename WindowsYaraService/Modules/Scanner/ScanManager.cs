using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Modules.Scanner.Models;

namespace WindowsYaraService.Modules.Scanner
{
    class ScanManager : BaseObservable<ScanManager.IListener>
    {
        internal interface IListener
        {
            void OnFileScanned(InfoModel report);
        }

        private readonly YaraScanner YaraScanner;
        private readonly VirusTotalScanner VirusTotalScanner;

        public ScanManager()
        {
            // Initialise Scanners
            YaraScanner = new YaraScanner();
            VirusTotalScanner = new VirusTotalScanner();
        }
        
        public void SetRules(string rulesPath)
        {
            YaraScanner.SetRules(rulesPath);
        }

        public void ScanFile(string filePath)
        {
            ThreadPool.QueueUserWorkItem(async i =>
            {
                var scanJob = new ScanJob(filePath);
                Task<InfoModel> infoModel = VirusTotalScanner.ScanFile(scanJob);
                List<YaraResult> yaraResults = new List<YaraResult>();
                string scanMessage = null;
                try
                {
                    yaraResults = YaraScanner.ScanFile(scanJob);
                }
                catch (Exception e)
                {
                    scanMessage = e.Message;
                }
                
                InfoModel model = await infoModel;
                model.YaraResults = yaraResults;

                // Check if virus total throw error
                if (model.ScandId == null)
                {
                    byte[] modelToBytes = Encoding.UTF8.GetBytes(model.GetHashCode().ToString());
                    model.SHA1 = HashHandler.ComputeSha1Hash(modelToBytes);
                    model.SHA256 = HashHandler.ComputeSha256Hash(modelToBytes);
                    model.ScandId = model.SHA256;
                    model.Date = DateTime.Now;
                    model.FilePath = scanJob.mFilePath;
                    model.Positives = 0;
                    model.Total = 0;
                    model.ReportTag = Tag.WARNING;
                }
                else
                {
                    if (model.Positives == 0 && model.YaraResults.Count == 0)
                    {
                        model.ReportTag = Tag.OK;
                    }
                    else if (model.Positives == 0 && model.YaraResults.Count > 0)
                    {
                        model.ReportTag = Tag.WARNING;
                    }
                    else
                    {
                        model.ReportTag = Tag.DANGER;
                    }
                }

                if (scanMessage != null)
                    model.Messages.Add(scanMessage);

                foreach (IListener listener in GetListeners().Keys)
                {
                    listener.OnFileScanned(model);
                }
            });
        }
    }
}
