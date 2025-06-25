using System.Collections.Generic;

namespace RdpScopeToggler.Services.FilesService
{
    public interface IFilesService
    {
        public List<string> GetWhiteList();

        public void AddToWhiteList(string ip);
    }
}
