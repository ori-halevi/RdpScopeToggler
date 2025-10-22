using RdpScopeToggler.Enums;
using RdpScopeToggler.Stores;
using System.Collections.Generic;

namespace RdpScopeToggler.Services.FilesService
{
    public interface IFilesService
    {
        public List<Client> GetWhiteList();
        public List<Client> GetAlwaysOnList();
        public void AddToWhiteList(string ip, string name = "Unnamed");
        public void AddToAlwaysOnList(string ip, bool isOpen, string name = "Unnamed");
        public void CleanWhiteList();
        public void CleanAlwaysOnList();
        public string GetLanguageFromSettings();
        public void WriteLanguageToSettings(string language);

        ActionsEnum GetDefaultStateFromSettings();
        void WriteDefaultStateToSettings(ActionsEnum defaultState);
    }
}
