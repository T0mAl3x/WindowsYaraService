using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base;

namespace WindowsYaraService.Modules
{
    class Update : BaseObservable<Update.IUpdateListener>
    {
        internal interface IUpdateListener
        {
            void OnRulesDownloaded();
        }

        private Thread mUpdateExecutor;

        public Update()
        {
            mUpdateExecutor = new Thread(new ThreadStart(ExecuteUpdate));
            mUpdateExecutor.Start();
        }

        private void ExecuteUpdate()
        {
            while(true)
            {
                string pathToYaraScrapper = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\YaraAgent\\YaraRulesScrapper";
                string pathToYaraRules = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\YaraAgent\\YaraRules";
                string command = $"CD {pathToYaraScrapper} & .\\venv\\Scripts\\activate & scrapy crawl yara_spider -a files_path={pathToYaraRules}";
                ExecuteCommand(command);
                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            //processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            //processInfo.RedirectStandardError = true;
            //processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("output>>" + e.Data);
            //process.BeginOutputReadLine();

            //process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("error>>" + e.Data);
            //process.BeginErrorReadLine();

            process.WaitForExit();

            //Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
    }
}
