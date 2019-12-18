using System.Collections.Generic;
using System.Linq;
using System.Threading;
using YaraSharp;
using Alphaleonis.Win32.Filesystem;
using System;

namespace WindowsYaraService
{
    class YaraScanner
    {
        private YSInstance mYSInstance = new YSInstance();
        //  Declare external variables (could be null)
        Dictionary<string, object> externals = new Dictionary<string, object>()
        {
            { "filename", string.Empty },
            { "filepath", string.Empty },
            { "extension", string.Empty }
        };
        private List<string> mRuleFilenames;

        public YaraScanner(string rulesPath)
        {
            mRuleFilenames = Directory.GetFiles(rulesPath, "*.yar", System.IO.SearchOption.AllDirectories).ToList();
            for (int index = 0; index < mRuleFilenames.Count; index++)
            {
                mRuleFilenames[index] = mRuleFilenames[index].Replace("\\", "/");
            }
        }

        public void scanFile(string filePath)
        {
            ThreadPool.QueueUserWorkItem(
                    (i) =>
                    {
                        try
                        {
                            using (YSContext context = new YSContext())
                            {
                                //	Compiling rules
                                using (YSCompiler compiler = mYSInstance.CompileFromFiles(mRuleFilenames, externals))
                                {
                                    //  Get compiled rules
                                    YSRules rules = compiler.GetRules();

                                    //  Get errors
                                    YSReport errors = compiler.GetErrors();
                                    //  Get warnings
                                    YSReport warnings = compiler.GetWarnings();

                                    //  Some file to test yara rules
                                    string Filename = filePath.Replace("\\", "/");

                                    //  Get matches
                                    List<YSMatches> Matches = mYSInstance.ScanFile(Filename, rules, new Dictionary<string, object>()
                                    {
                                        { "filename", Alphaleonis.Win32.Filesystem.Path.GetFileName(Filename) },
                                        { "filepath", Alphaleonis.Win32.Filesystem.Path.GetFullPath(Filename) },
                                        { "extension", Alphaleonis.Win32.Filesystem.Path.GetExtension(Filename) }
                                    }, 0);

                                    //  Iterate over matches
                                    if (Matches.Count == 0)
                                    {
                                        File.AppendAllText(@"D:\Master\My_Dizertation\test", "No matches found for " + Filename + Environment.NewLine);
                                    }
                                    else
                                    {
                                        foreach (YSMatches match in Matches)
                                        {
                                            File.AppendAllText(@"D:\Master\My_Dizertation\test", match.Rule.Identifier + Environment.NewLine);
                                        }
                                    }
                                }
                                //  Log errors
                            }
                        } catch(Exception e)
                        {
                            File.AppendAllText(@"D:\Master\My_Dizertation\ERRORS.txt", e.Message + Environment.NewLine);
                        }
                    }
                );
        }
    }
}
