using System.ServiceProcess;
using System.Runtime.InteropServices;
using WindowsYaraService.Modules;
using System.Threading;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Base;
using WindowsYaraService.Modules.Scanner;
using WindowsYaraService.Base.Jobs.common;
using System;
using WindowsYaraService.Modules.Registrator;
using System.Globalization;
using WindowsYaraService.Base.Common;
using System.Text;
using System.Net;

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

    public partial class YaraService : ServiceBase, Detector.IListener, ScanManager.IListener, Update.IListener, Registrator.IListener, Networking.IListener
    {
        private Detector mDetector;
        private ScanManager mScanManager;
        private Scheduler mScheduler;
        private Networking mNetworking;
        private Update mUpdate;
        private Registrator mRegistrator;

        public static string _GUID;

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

            // Generate APP GUID
            byte[] encGuid = FileHandler.ReadBytesToFile("GUID");
            if (encGuid != null)
            {
                byte[] decGuid = DataProtection.Unprotect(encGuid);
                _GUID = Encoding.ASCII.GetString(decGuid);
            }
            else
            {
                _GUID = Guid.NewGuid().ToString();
                encGuid = DataProtection.Protect(Encoding.ASCII.GetBytes(_GUID));
                FileHandler.WriteBytesToFile("GUID", encGuid);
            }

            // Trusting all certificates
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            // Initialise Scheduler
            mScheduler = new Scheduler();

            // Initialise Detector
            mDetector = new Detector();
            mDetector.RegisterListener(this);

            // Initialise Networking
            mNetworking = new Networking();
            mNetworking.RegisterListener(this);

            // Initialise Update
            mUpdate = new Update();
            mUpdate.RegisterListener(this);

            // Initialise Registrator
            mRegistrator = new Registrator(mNetworking);
            mRegistrator.RegisterListener(this);
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart.");

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
        public void OnFileCreated(string filePath)
        {
            mScheduler.ScheduleJobForScannig(new ScheduleJob(filePath));
        }

        public void OnFileChanged(string filePath)
        {
            mScheduler.ScheduleJobForScannig(new ScheduleJob(filePath));
        }

        // Scanner Listener
        public void OnFileScanned(InfoModel report)
        {
            // IF: AuthFailed set, save on local disk
            // ELSE: Check for local reports; Send reports to server
        }

        // Update Listener
        public void OnRulesDownloaded()
        {
            // Initialise Scanner
            string rulesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\YaraAgent\\YaraRules";
            mScanManager = new ScanManager(rulesPath);
            // Unregister from updates
            mUpdate.UnregisterListener(this);

            mRegistrator.Enroll();
        }

        // Registration Listener
        public void OnRegister()
        {
            // Fetch jobs from scheduler
            AUTH_FAILED = false;
            mJobFetcher = new Thread(new ThreadStart(FetchScheduledJob));
            mJobFetcher.Start();
        }

        private void FetchScheduledJob()
        {
            while (true)
            {
                var filePath = mScheduler.FetchJobForScanning();
                if (filePath == null)
                {
                    FetchSignal.waitHandle.WaitOne();
                    continue;
                }

                mScanManager.ScanFile(filePath);
            }
        }

        private readonly object AUTH_FAILED_LOCK;
        private bool AUTH_FAILED = false;
        public void OnAuthFailed(InfoModel infoModel)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            string infoModelString = Newtonsoft.Json.JsonConvert.SerializeObject(infoModel);
            FileHandler.WriteTextToFile(timestamp, infoModelString);

            lock (AUTH_FAILED_LOCK)
            {
                if (!AUTH_FAILED)
                {
                    mJobFetcher.Abort();
                    mRegistrator.Enroll();
                    AUTH_FAILED = true;
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
