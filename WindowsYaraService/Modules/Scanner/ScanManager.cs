using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base;
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
                List<YaraResult> yaraResults = null;
                Message scanMessage = null;
                try
                {
                    yaraResults = YaraScanner.ScanFile(scanJob);
                }
                catch (Exception e)
                {
                    scanMessage = new Message { Information = e.Message, Type = MessageType.ERROR };
                }
                InfoModel model = await infoModel;
                model.YaraResults = yaraResults;
                if (scanMessage != null)
                    model.Messages.Add(scanMessage);

                foreach (IListener listener in GetListeners())
                {
                    listener.OnFileScanned(model);
                }
            });
        }
    }
}
