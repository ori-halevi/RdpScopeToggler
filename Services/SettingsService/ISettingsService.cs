using RdpScopeToggler.Enums;

namespace RdpScopeToggler.Services.SettingsService
{
    public interface ISettingsService
    {
        void SetState(ActionsEnum state);

        ActionsEnum GetState();
    }
}
