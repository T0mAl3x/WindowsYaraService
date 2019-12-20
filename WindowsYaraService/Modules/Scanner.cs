using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Jobs;
using YaraSharp;
using static WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Modules
{
    class Scanner : BaseObservable<Listener>
    {
        internal interface Listener
        {
            void onFileScanned(string report);
        }

        private YSInstance YSInstance = new YSInstance();
        private YSContext YSContext = new YSContext();
        private YSRules YSRules;

        public Scanner(string rulesPath)
        {
            List<string> mRuleFilenames = Directory.GetFiles(rulesPath, "*.yar", System.IO.SearchOption.AllDirectories).ToList();
            for (int index = 0; index < mRuleFilenames.Count; index++)
            {
                mRuleFilenames[index] = mRuleFilenames[index].Replace("\\", "/");
            }
            YSCompiler YSCompiler = YSInstance.CompileFromFiles(mRuleFilenames, null);
            YSRules = YSCompiler.GetRules();
        }

        public void scanFile(ScanJob scanJob)
        {
            ThreadPool.QueueUserWorkItem((i) =>
            {
                try
                {
                    string Filename = scanJob.mFilePath.Replace("\\", "/");

                    //  Get matches
                    List<YSMatches> Matches = YSInstance.ScanFile(Filename, YSRules, null, 0);

                    //  Iterate over matches
                    File.AppendAllText(@"D:\Master\My_Dizertation\test.txt", "*************************** -> " + Filename + Environment.NewLine);
                    if (Matches.Count == 0)
                    {
                        File.AppendAllText(@"D:\Master\My_Dizertation\test.txt", "No matches found for " + Filename + Environment.NewLine);
                    }
                    else
                    {
                        foreach (YSMatches match in Matches)
                        {
                            File.AppendAllText(@"D:\Master\My_Dizertation\test.txt", match.Rule.Identifier + Environment.NewLine);
                        }
                    }
                    File.AppendAllText(@"D:\Master\My_Dizertation\test.txt", "***************************" + Environment.NewLine);
                }
                catch(Exception e)
                {
                    File.AppendAllText(@"D:\Master\My_Dizertation\ERRORS.txt", e.Message + Environment.NewLine);
                }
            });
        }

        ~Scanner()
        {
            YSRules.Destroy();
            YSContext.Destroy();
        }
    }
}
