using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    class YaraScanner
    {
        private YSInstance YSInstance = new YSInstance();
        private YSContext YSContext = new YSContext();
        private YSRules YSRules = null;

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
            if (YSRules == null)
            {
                throw new Exception("No rules avaible");
            }

            List<YaraResult> yaraResults = new List<YaraResult>();
            try
            {
                string Filename = scanJob.mFilePath.Replace("\\", "/");

                //  Get matches
                List<YSMatches> Matches = YSInstance.ScanFile(Filename, YSRules, null, 0);
                //  Iterate over matches
                if (Matches.Count != 0)
                {
                    foreach (YSMatches match in Matches)
                    {
                        var yaraResult = new YaraResult();
                        yaraResult.Identifier = match.Rule.Identifier;
                        yaraResult.Meta = new Dictionary<string, string>();
                        foreach (var item in match.Rule.Meta)
                        {
                            if (item.Value is string)
                            {
                                yaraResult.Meta.Add(item.Key, item.Value as string);
                            }
                        }
                        yaraResults.Add(yaraResult);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
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
