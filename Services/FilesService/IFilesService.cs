using System.Collections.Generic;
using RdpScopeToggler.Stores;

namespace RdpScopeToggler.Services.FilesService
{
    public interface IFilesService
    {
        public List<WhiteListClient> GetWhiteList();

        public void AddToWhiteList(string ip, string name = "Unnamed");

        public void CleanWhiteList();
    }
}
