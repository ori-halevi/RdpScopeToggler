using System;
using System.IO;
using IWshRuntimeLibrary;
using File = System.IO.File;
using Path = System.IO.Path;

namespace RdpScopeToggler.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        public event Action NotificationToolInstalled;
        private readonly string pathToToastMessageFile;
        private readonly string pathToToastSoftwareFile;
        private readonly string sourceBaseDirectory;
        public NotificationService(string pathToToastMessageFile, string pathToToastSoftwareFile, string sourceBaseDirectory)
        {
            this.pathToToastMessageFile = pathToToastMessageFile;
            this.pathToToastSoftwareFile = pathToToastSoftwareFile;
            string directoryPath = Path.GetDirectoryName(pathToToastMessageFile);
            EnsureDirectoryExists(directoryPath);

            this.sourceBaseDirectory = sourceBaseDirectory;
        }


        #region Public methodes
        public void SendPreDisconnectAlert()
        {
            File.WriteAllText(pathToToastMessageFile, "החיבור שלך עומד להסגר! נא להתכונן בהתאם.");
        }
        #endregion



        #region Private methodes
        public void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        public void InitializeInstallation()
        {
            // Install the notification tool.
            string directoryPath = Path.GetDirectoryName(pathToToastSoftwareFile);
            if (!Directory.Exists(directoryPath) || !File.Exists(pathToToastSoftwareFile))
            {
                try
                {
                    InstallNotificationTool(directoryPath);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to install the notification tool.", ex);
                }
                NotificationToolInstalled.Invoke();
            }


            // Create startup shortcut
            string startupPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp";
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(pathToToastSoftwareFile);
            string shortcutPath = Path.Combine(startupPath, $"{nameWithoutExtension}.lnk");
            if (!File.Exists(shortcutPath))
            {
                try
                {
                    CreateShortcut(shortcutPath, pathToToastSoftwareFile);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to create shortcut to notifications tool.", ex);
                }
                NotificationToolInstalled.Invoke();
            }
        }

        private void InstallNotificationTool(string installPath)
        {
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceBaseDirectory);

            if (!Directory.Exists(installPath))
                Directory.CreateDirectory(installPath);

            foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourcePath, file);
                string destinationFile = Path.Combine(installPath, relativePath);
                string destinationDir = Path.GetDirectoryName(destinationFile);

                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(file, destinationFile, true);
            }
        }
        private void CreateShortcut(string shortcutPath, string targetPath)
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.WindowStyle = 1;
            shortcut.Description = "Auto-start notification tool on login";
            shortcut.Save();
        }
        #endregion
    }
}
