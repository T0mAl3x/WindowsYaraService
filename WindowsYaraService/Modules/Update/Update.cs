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
    class Update : BaseObservable<Update.IListener>
    {
        internal interface IListener
        {
            void OnRulesDownloaded();
        }

        private Thread mUpdateExecutor;

        public void ExecuteUpdate()
        {
            mUpdateExecutor = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    string pathToYaraScrapper = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\YaraAgent\\YaraRulesScrapper";
                    string pathToYaraRules = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\YaraAgent\\YaraRules";
                    pathToYaraRules = pathToYaraRules.Replace("\\", "/");
                    string command = $"CD {pathToYaraScrapper} & .\\venv\\Scripts\\activate & scrapy crawl yara_spider -a files_path={pathToYaraRules}";
                    //ExecuteCommand(command);

                    foreach (IListener listener in GetListeners().Keys)
                    {
                        listener.OnRulesDownloaded();
                    }
                    Thread.Sleep(10 * 1000);
                    //Thread.Sleep(60 * 60 * 1000);
                }
            }));
            mUpdateExecutor.Start();
        }

        private void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            //processInfo.RedirectStandardError = true;
            //processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    EventLog.WriteEntry("Application", "output Data>> " + e.Data, EventLogEntryType.Information);
            //process.BeginOutputReadLine();

            //process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    EventLog.WriteEntry("Application", "output Error>> " + e.Data, EventLogEntryType.Information);
            //process.BeginErrorReadLine();

            process.WaitForExit();

            //Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
    }
}
