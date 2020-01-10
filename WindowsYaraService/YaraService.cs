using System.ServiceProcess;
using System.Runtime.InteropServices;
using WindowsYaraService.Modules;
using System.Threading;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Base;
using WindowsYaraService.Modules.Scanner;
using static WindowsYaraService.Base.Jobs.common.NetJob;
using WindowsYaraService.Base.Jobs.common;
using static WindowsYaraService.Modules.Update;
using System;

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

    public partial class YaraService : ServiceBase, Detector.IListener, ScanManager.IListener, INetworkListener, IUpdateListener
    {
        private Detector mDetector;
        private ScanManager mScanManager;
        private Scheduler mScheduler;
        private Networking mNetworking;
        private Update mUpdate;

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

            // Initialise Scheduler
            mScheduler = new Scheduler();

            // Initialise Detector
            mDetector = new Detector();
            mDetector.RegisterListener(this);

            // Initialise Networking
            mNetworking = new Networking();

            // Initialise Update
            mUpdate = new Update();
            mUpdate.RegisterListener(this);
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart.");

            // Need to update rules


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
            mScheduler.ScheduleJobForScannig(new Base.Jobs.ScheduleJob(filePath));
        }

        public void OnFileChanged(string filePath)
        {
            mScheduler.ScheduleJobForScannig(new Base.Jobs.ScheduleJob(filePath));
        }

        // Scanner Listener
        public void OnFileScanned(InfoModel report)
        {
            // Send report to server
        }

        // Update Listener
        public void OnRulesDownloaded()
        {
            // Initialise Scanner
            string rulesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/YaraAgent/YaraRules";
            mScanManager = new ScanManager(rulesPath);

            // Need to register
            NetRegisterJob netRegJob = new NetRegisterJob();
            netRegJob.RegisterListener(this);
            mNetworking.ExecuteAsync(netRegJob);

            // Unregister from updates
            mUpdate.UnregisterListener(this);
        }

        // Registration Listener
        public void OnSuccess(object response)
        {
            // Fetch jobs from scheduler
            mJobFetcher = new Thread(new ThreadStart(() =>
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
            }));
            mJobFetcher.Start();
        }

        public void OnFailure(NetJob netJob)
        {
            // Do nothing for now
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
