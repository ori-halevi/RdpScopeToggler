using RdpScopeToggler.Enums;
using RdpScopeToggler.Services.FilesService;

namespace RdpScopeToggler.Services.SettingsService
{
    public class SettingsService : ISettingsService
    {
        private readonly IFilesService _filesService;
        public SettingsService(IFilesService filesService)
        {
            _filesService = filesService;
        }

        public ActionsEnum GetState()
        {
            return _filesService.GetDefaultStateFromSettings();
        }

        public void SetState(ActionsEnum state)
        {
            _filesService.WriteDefaultStateToSettings(state);
        }
    }
}
