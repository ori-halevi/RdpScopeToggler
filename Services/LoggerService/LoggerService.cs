using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.LoggerService
{
    public class LoggerService : ILoggerService
    {
        private readonly string _loggerFolderPath;
        private string _logFilePath;
        private readonly object _lock = new object();
        private int _fileIndex = 0;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public LoggerService(string loggerFolderPath)
        {
            _loggerFolderPath = loggerFolderPath ?? throw new ArgumentNullException(nameof(loggerFolderPath));

            if (!Directory.Exists(_loggerFolderPath))
            {
                Directory.CreateDirectory(_loggerFolderPath);
            }

            _logFilePath = GetNewLogFilePath();
        }

        private string GetNewLogFilePath()
        {
            var baseFileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}";
            var newFileName = $"{baseFileName}.txt";

            // במקרה שפותחים הרבה לוגים ברצף, מוסיפים index
            if (_fileIndex > 0)
            {
                newFileName = $"{baseFileName}_{_fileIndex}.txt";
            }

            _fileIndex++;
            return Path.Combine(_loggerFolderPath, newFileName);
        }

        private void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                // בודק אם הקובץ קיים ואם עבר את הגודל המקסימלי
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length >= MaxFileSizeBytes)
                    {
                        _logFilePath = GetNewLogFilePath();
                    }
                }

                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }

        public void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public void Warn(string message)
        {
            WriteLog("WARN", message);
        }

        public void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        public void Error(string message, Exception ex)
        {
            var fullMessage = $"{message} | Exception: {ex}";
            WriteLog("ERROR", fullMessage);
        }

        public void Fatal(string message)
        {
            WriteLog("FATAL", message);
        }

        public void Fatal(string message, Exception ex)
        {
            var fullMessage = $"{message} | Exception: {ex}";
            WriteLog("FATAL", fullMessage);
        }

        public void OpenLogger()
        {
            if (Directory.Exists(_loggerFolderPath))
            {
                Process.Start("explorer.exe", _loggerFolderPath);
            }
        }
    }
}
