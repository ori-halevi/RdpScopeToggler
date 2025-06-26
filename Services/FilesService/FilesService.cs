using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RdpScopeToggler.Stores;

namespace RdpScopeToggler.Services.FilesService
{
    public class FilesService : IFilesService
    {
        public List<WhiteListClient> GetWhiteList()
        {
            EnsureWhiteListFileExists();
            return ReadWhiteList();
        }


        public void AddToWhiteList(string ip, string name = "Unnamed")
        {
            EnsureWhiteListFileExists();
            WriteToWhiteList(ip, name);
        }


        public void CleanWhiteList()
        {
            const string filePath = @"C:\ProgramData\RdpScopeToggler\WhiteList.json";
            List<string> whiteList = new List<string>();
            string updatedJson = JsonSerializer.Serialize(whiteList, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, updatedJson);
        }


        #region Private Methodes
        private void WriteToWhiteList(string ip, string name)
        {
            name = name.Trim();
            ip = ip.Trim();
            string filePath = @"C:\ProgramData\RdpScopeToggler\WhiteList.json";
            List<WhiteListClient> whiteList;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    whiteList = JsonSerializer.Deserialize<List<WhiteListClient>>(json) ?? new List<WhiteListClient>();
                }
                catch
                {
                    whiteList = new List<WhiteListClient>();
                }
            }
            else
            {
                whiteList = new List<WhiteListClient>();
            }

            if (whiteList.Any(entry => entry.Address == ip && !entry.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                var entryToUpdate = whiteList.First(e => e.Address == ip);
                entryToUpdate.Name = name;

                string updatedJson = JsonSerializer.Serialize(whiteList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, updatedJson);
            }
            else if (!whiteList.Any(entry => entry.Address == ip && entry.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                whiteList.Add(new WhiteListClient { Address = ip, Name = name });

                string updatedJson = JsonSerializer.Serialize(whiteList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, updatedJson);
            }
        }


        private List<WhiteListClient> ReadWhiteList()
        {
            string filePath = @"C:\ProgramData\RdpScopeToggler\WhiteList.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Whitelist file not found.", filePath);
            }

            string jsonContent = File.ReadAllText(filePath);

            try
            {
                var list = JsonSerializer.Deserialize<List<WhiteListClient>>(jsonContent);
                return list ?? new List<WhiteListClient>();
            }
            catch (JsonException)
            {
                return new List<WhiteListClient>();
            }
        }



        private void EnsureWhiteListFileExists()
        {
            string folderPath = @"C:\ProgramData\RdpScopeToggler";
            string filePath = Path.Combine(folderPath, "WhiteList.json");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                var defaultList = new List<WhiteListClient>();

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
