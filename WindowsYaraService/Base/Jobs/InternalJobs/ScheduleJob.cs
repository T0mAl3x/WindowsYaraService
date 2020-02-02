using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static WindowsYaraService.Base.Jobs.ScheduleJob;

namespace WindowsYaraService.Base.Jobs
{
    class ScheduleJob : BaseObservableJob<IListener>
    {
        internal enum Priorities
        {
            SMALL,
            MEDIUM,
            BIG
        }

        internal interface IListener
        {
            void OnJobReady(ScheduleJob job);
        }

        public Priorities mPriority { get; set; }
        private Thread mFileChecker;
        private ScheduleJob job;

        public ScheduleJob(string filePath) : base(filePath)
        {
            
        }

        public void ExecuteFileChecker()
        {
            job = this;
            FileInfo file = new FileInfo(mFilePath);

            mFileChecker = new Thread(new ParameterizedThreadStart(CheckerTask));
            mFileChecker.Start(file);
        }

        private void CheckerTask(object file)
        {
            try
            {
                while (true)
                {
                    if (checkIfFileIsReady(file as FileInfo))
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }

                var size = (file as FileInfo).Length;
                switch (size)
                {
                    case long n when (n >= 0 && n < 50000000):
                        mPriority = Priorities.SMALL;
                        break;
                    case long n when (n >= 50000000 && n < 200000000):
                        mPriority = Priorities.MEDIUM;
                        break;
                    case long n when (n >= 200000000):
                        mPriority = Priorities.BIG;
                        break;
                }

                foreach (IListener listener in job.GetListeners())
                {
                    listener.OnJobReady(job);
                }
            }
            catch (FileNotFoundException)
            {
                
            }
        }

        private bool checkIfFileIsReady(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch(FileNotFoundException exception)
            {
                throw exception;
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }
    }
}
