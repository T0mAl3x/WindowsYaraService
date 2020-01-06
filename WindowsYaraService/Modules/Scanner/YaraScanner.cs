using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Modules.Scanner.Models;
using YaraSharp;
using static WindowsYaraService.Modules.YaraScanner;

namespace WindowsYaraService.Modules
{
    class YaraScanner : BaseObservable<Listener>
    {
        internal interface Listener
        {
            void onFileScanned(string report);
        }

        private YSInstance YSInstance = new YSInstance();
        private YSContext YSContext = new YSContext();
        private YSRules YSRules;

        public YaraScanner(string rulesPath)
        {
            List<string> mRuleFilenames = Directory.GetFiles(rulesPath, "*.yar", System.IO.SearchOption.AllDirectories).ToList();
            for (int index = 0; index < mRuleFilenames.Count; index++)
            {
                mRuleFilenames[index] = mRuleFilenames[index].Replace("\\", "/");
            }
            YSCompiler YSCompiler = YSInstance.CompileFromFiles(mRuleFilenames, null);
            YSRules = YSCompiler.GetRules();
        }

        public List<YaraResult> ScanFile(ScanJob scanJob)
        {
            List<YaraResult> yaraResults = new List<YaraResult>();
            try
            {
                string Filename = scanJob.mFilePath.Replace("\\", "/");

                //  Get matches
                List<YSMatches> Matches = YSInstance.ScanFile(Filename, YSRules, null, 0);

                //  Iterate over matches
                File.AppendAllText(@"C:\Users\IEUser\Documents\Work\test.txt", "*************************** -> " + Filename + Environment.NewLine);
                if (Matches.Count == 0)
                {
                    File.AppendAllText(@"C:\Users\IEUser\Documents\Work\test.txt", "No matches found for " + Filename + Environment.NewLine);
                }
                else
                {
                    foreach (YSMatches match in Matches)
                    {
                        File.AppendAllText(@"C:\Users\IEUser\Documents\Work\test.txt", match.Rule.Identifier + Environment.NewLine);
                    }
                    
                }
            }
            catch (Exception e)
            {
                File.AppendAllText(@"C:\Users\IEUser\Documents\Work\ERRORS.txt", e.Message + Environment.NewLine);
                return null;
            }
            return yaraResults;
        }

        ~YaraScanner()
        {
            YSRules.Destroy();
            YSContext.Destroy();
        }
    }
}
