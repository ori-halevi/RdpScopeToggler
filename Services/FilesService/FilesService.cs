using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GraphicRdpScopeToggler.Services.FilesService
{
    public class FilesService : IFilesService
    {
        public List<string> GetWhiteList()
        {
            EnsureWhiteListFileExists();
            List<string> list = ReadWhiteList();
            return list;
        }

        public void AddToWhiteList(string ip)
        {
            EnsureWhiteListFileExists();
            WriteToWhiteList(ip);
        }



        #region Private Methodes
        private void WriteToWhiteList(string ip)
        {
            string filePath = @"C:\ProgramData\GraphicRdpScopeToggler\WhiteList.json";
            List<string> whiteList;

            // אם הקובץ קיים – קרא אותו, אחרת התחל עם רשימה ריקה
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    whiteList = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
                catch
                {
                    whiteList = new List<string>();
                }
            }
            else
            {
                whiteList = new List<string>();
            }

            // אם ה-IP לא קיים – נוסיף אותו
            if (!whiteList.Contains(ip))
            {
                whiteList.Add(ip);

                string updatedJson = JsonSerializer.Serialize(whiteList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, updatedJson);
            }
        }

        private List<string> ReadWhiteList()
        {
            string filePath = @"C:\ProgramData\GraphicRdpScopeToggler\WhiteList.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Whitelist file not found.", filePath);
            }

            string jsonContent = File.ReadAllText(filePath);

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(jsonContent);
                return list ?? new List<string>();
            }
            catch (JsonException)
            {
                // אם יש טעות בפורמט הקובץ – נחזיר רשימה ריקה
                return new List<string>();
            }
        }


        private void EnsureWhiteListFileExists()
        {
            string folderPath = @"C:\ProgramData\GraphicRdpScopeToggler";
            string filePath = Path.Combine(folderPath, "WhiteList.json");

            // Create directory if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create file if it doesn't exist
            if (!File.Exists(filePath))
            {
                var defaultList = new List<string>{};

                string json = JsonSerializer.Serialize(defaultList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
            }
        }

        #endregion
    }
}
