using System.ServiceProcess;
using System.Runtime.InteropServices;
using WindowsYaraService.Modules;
using System.Threading;
using WindowsYaraService.Base.Jobs;

namespace WindowsYaraService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class YaraService : ServiceBase, Detector.Listener
    {
        private Detector mDetector;
        private Scanner mScanner;
        private Scheduler mScheduler;

        private Thread mJobFetcher;

        public YaraService()
        {
            InitializeComponent();

            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;

            // Initialise Detector
            mDetector = new Detector();
            mDetector.RegisterListener(this);

            // Initialise Scanner
            mScanner = new Scanner(@"D:\Master\My_Dizertation\Project\rules");

            // Initialise Scheduler
            mScheduler = new Scheduler();
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart.");

            // Fetch jobs from scheduler
            //mJobFetcher = new Thread(new ThreadStart(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(1000);
            //        var filePath = mScheduler.FetchJobForScanning();
            //        if (filePath == null)
            //        {
            //            continue;
            //        }

            //        var scanJob = new ScanJob(filePath);
            //        mScanner.scanFile(scanJob);
            //    }
            //}));
            //mJobFetcher.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStop.");

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        // Detector Listener
        public void onFileCreated(string filePath)
        {
            mScheduler.ScheduleJobForScannig(new Base.Jobs.ScheduleJob(filePath));
        }

        public void onFileChanged(string filePath)
        {
            mScheduler.ScheduleJobForScannig(new Base.Jobs.ScheduleJob(filePath));
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
