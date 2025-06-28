using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RdpScopeToggler.Stores;

namespace RdpScopeToggler.Services.FilesService
{
    public class FilesService : IFilesService
    {
        public List<Client> GetWhiteList()
        {
            EnsureListFileExists("WhiteList.json");
            return ReadList("WhiteList.json");
        }
        public List<Client> GetAlwaysOnList()
        {
            EnsureListFileExists("AlwaysOnList.json");
            return ReadList("AlwaysOnList.json");
        }


        public void AddToWhiteList(string ip, string name = "Unnamed")
        {
            EnsureListFileExists("WhiteList.json");
            WriteToList(ip, name, true, "WhiteList.json");
        }

        public void AddToAlwaysOnList(string ip, bool isOpen, string name = "Unnamed")
        {
            EnsureListFileExists("AlwaysOnList.json");
            WriteToList(ip, name, isOpen, "AlwaysOnList.json");
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

        public void CleanAlwaysOnList()
        {
            const string filePath = @"C:\ProgramData\RdpScopeToggler\AlwaysOnList.json";
            List<string> whiteList = new List<string>();
            string updatedJson = JsonSerializer.Serialize(whiteList, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, updatedJson);
        }



        #region Private Methodes
        private void WriteToList(string ip, string name, bool isOpen = true, string fileName = "WhiteList.json")
        {
            name = name.Trim();
            ip = ip.Trim();
            string filePath = $"C:\\ProgramData\\RdpScopeToggler\\{fileName}";
            List<Client> list;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    list = JsonSerializer.Deserialize<List<Client>>(json) ?? new List<Client>();
                }
                catch
                {
                    list = new List<Client>();
                }
            }
            else
            {
                list = new List<Client>();
            }

            if (list.Any(entry => entry.Address == ip && !entry.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                var entryToUpdate = list.First(e => e.Address == ip);
                entryToUpdate.Name = name;
                entryToUpdate.IsOpen = isOpen;

                string updatedJson = JsonSerializer.Serialize(list, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, updatedJson);
            }
            else if (!list.Any(entry => entry.Address == ip && entry.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                list.Add(new Client { Address = ip, Name = name, IsOpen = isOpen });

                string updatedJson = JsonSerializer.Serialize(list, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, updatedJson);
            }
        }


        private List<Client> ReadList(string fileName = "WhiteList.json")
        {
            string filePath = $"C:\\ProgramData\\RdpScopeToggler\\{fileName}";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{fileName} file not found.", filePath);
            }

            string jsonContent = File.ReadAllText(filePath);

            try
            {
                var list = JsonSerializer.Deserialize<List<Client>>(jsonContent);
                return list ?? new List<Client>();
            }
            catch (JsonException)
            {
                return new List<Client>();
            }
        }


        private void EnsureListFileExists(string fileName = "WhiteList.json")
        {
            string folderPath = @"C:\ProgramData\RdpScopeToggler";
            string filePath = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                var defaultList = new List<Client>();

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
