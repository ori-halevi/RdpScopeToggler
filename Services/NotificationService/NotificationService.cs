using System.IO;

namespace RdpScopeToggler.Services.NotificationService
{
    public class NotificationService : INotificationService
    {

        private readonly string pathToTxtFile;
        public NotificationService(string pathToTxtFile)
        {
            this.pathToTxtFile = pathToTxtFile;
            EnsureFileExists(pathToTxtFile);
        }

        #region Public methodes
        public void SendPreDisconnectAlert()
        {
            File.WriteAllText(pathToTxtFile, "החיבור שלך עומד להסגר! נא להתכונן בהתאם.");
        }
        #endregion



        #region Private methodes
        private void EnsureFileExists(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            // Check if directory exists
            if (!Directory.Exists(directoryPath) || !File.Exists(filePath))
            {
                CreateFile(filePath);
            }
        }

        private void CreateFile(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            // Create directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the file if it doesn't exist
            /*if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose(); // Dispose to release the handle
            }*/
        }


        #endregion
    }
}
