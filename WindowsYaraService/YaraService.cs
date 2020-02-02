using System.ServiceProcess;
using System.Runtime.InteropServices;
using WindowsYaraService.Modules;
using System.Threading;
using WindowsYaraService.Base.Jobs;
using WindowsYaraService.Base;
using WindowsYaraService.Modules.Scanner;
using System;
using WindowsYaraService.Base.Common;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Base.Jobs.common;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

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

    public partial class YaraService : ServiceBase, Detector.IListener, ScanManager.IListener, Update.IListener, EnrollmentJob.IListener
    {
        private Detector mDetector;
        private ScanManager mScanManager;
        private Scheduler mScheduler;
        private Networking mNetworking;
        private Update mUpdate;

        private CertHandler _certHandler = new CertHandler();

        private Thread mJobFetcher;

        public YaraService()
        {
            InitializeComponent();

            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            eventLog1 = new System.Diagnostics.EventLog();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;

            // Trusting all certificates from server
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            // Initialise Scheduler
            mScheduler = new Scheduler();

            // Initialise Detector
            mDetector = new Detector();
            mDetector.RegisterListener(this);

            // Initialise Scanner
            mScanManager = new ScanManager();
            mScanManager.RegisterListener(this);

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

            HttpClientSingleton.InitialiseInstance();

            // Build enrollment job
            EnrollmentJob enrollmentJob = new EnrollmentJob();
            enrollmentJob.RegisterListener(this);
            mNetworking.ExecuteAsync(enrollmentJob);

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


        /*
         *  Detection and Scan
         */
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
            // Check not send reports
            List<NetScanJob> netScanJobs = new List<NetScanJob>();
            //foreach (string file in Directory.EnumerateFiles(FileHandler.REPORT_ARCHIVE))
            //{
            //    byte[] encReport = FileHandler.ReadTextFromFile(file);
            //    byte[] decReport = DataProtection.Unprotect(encReport);
            //    string content = Encoding.UTF8.GetString(decReport);
            //    InfoModel infoModel = JsonConvert.DeserializeObject<InfoModel>(content);
            //    NetScanJob netScanJob = new NetScanJob(report);
            //    netScanJobs.Add(netScanJob);
            //}

            //netScanJobs.Add(new NetScanJob(report));
            //foreach (NetScanJob job in netScanJobs)
            //{
            //    mNetworking.ExecuteAsync(job);
            //}
        }
        /*
         * *******************************************************************************
         */


        // Update Listener
        public void OnRulesDownloaded()
        {
            mScanManager.SetRules(FileHandler.RULES_PATH);
            // Fetch jobs from scheduler
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

        public void OnEnrolled()
        {
            mUpdate.ExecuteUpdate();
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
