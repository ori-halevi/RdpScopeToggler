using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GraphicRdpScopeToggler.Services.FilesService
{
    public interface IFilesService
    {
        public List<string> GetWhiteList();

        public void AddToWhiteList(string ip);
    }
}
