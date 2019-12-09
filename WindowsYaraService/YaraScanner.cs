using System.Collections.Generic;
using System.Linq;
using System.Threading;
using YaraSharp;
using Alphaleonis.Win32.Filesystem;

namespace WindowsYaraService
{
    class YaraScanner
    {
        private YSInstance mYSInstance = new YSInstance();
        private List<string> mRuleFilenames;

        public YaraScanner(string rulesPath)
        {
            mRuleFilenames = Directory.GetFiles(rulesPath, "*.yar", System.IO.SearchOption.AllDirectories).ToList();
        }

        public void scanFile(string filePath)
        {
            ThreadPool.QueueUserWorkItem(
                    (i) =>
                    {
                        using (YSContext context = new YSContext())
                        {
                            //	Compiling rules
                            using (YSCompiler compiler = mYSInstance.CompileFromFiles(mRuleFilenames, null))
                            {
                                //  Get compiled rules
                                YSRules rules = compiler.GetRules();

                                //  Get errors
                                YSReport errors = compiler.GetErrors();
                                //  Get warnings
                                YSReport warnings = compiler.GetWarnings();

                                //  Some file to test yara rules
                                string Filename = filePath;

                                //  Get matches
                                List<YSMatches> Matches = mYSInstance.ScanFile(Filename, rules,
                                        new Dictionary<string, object>()
                                            {
                                                { "filename", Path.GetFileName(Filename) },
                                                { "filepath", Path.GetFullPath(Filename) },
                                                { "extension", Path.GetExtension(Filename) }
                                            },
                                        0);

                                //  Iterate over matches
                                foreach (YSMatches Match in Matches)
                                {
                                    
                                }
                            }
                            //  Log errors
                        }
                    }
                );
        }
    }
}
