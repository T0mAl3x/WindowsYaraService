using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService
{
    class Detector : Base.BaseObservable<Detector.Listener>
    {
        internal interface Listener
        {
            void onFileCreated(string filePath);
            void onFileChanged(string filePath);
        }

        private List<FileSystemWatcher> mFileSystemWatchers = new List<FileSystemWatcher>();

        public Detector()
        {
            // initialize watchers
            DriveInfo[] drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

                // Associate event handlers with the events
                fileSystemWatcher.Created += FileSystemWatcher_Created;
                fileSystemWatcher.Changed += FileSystemWatcher_Changed;

                // Tell watcher where to look
                fileSystemWatcher.Path = drives[i].Name;
                fileSystemWatcher.EnableRaisingEvents = true;
                //fileSystemWatcher.IncludeSubdirectories = true;
                fileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
                mFileSystemWatchers.Add(fileSystemWatcher);
            }
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            foreach (Listener listener in mListeners)
            {
                listener.onFileChanged(e.FullPath);
            }
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            foreach (Listener listener in mListeners)
            {
                listener.onFileCreated(e.FullPath);
            }
        }
    }
}
