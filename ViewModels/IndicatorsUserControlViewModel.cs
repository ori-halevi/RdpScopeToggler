using GraphicRdpScopeToggler.Services.RdpService;
using Prism.Mvvm;

namespace RdpScopeToggler.ViewModels
{
    public class IndicatorsUserControlViewModel : BindableBase
    {
        private bool _isInternalOpen = true;
        public bool IsInternalOpen
        {
            get => _isInternalOpen;
            set => SetProperty(ref _isInternalOpen, value);
        }

        private bool _isExternalOpen;
        public bool IsExternalOpen
        {
            get => _isExternalOpen;
            set => SetProperty(ref _isExternalOpen, value);
        }
        public IndicatorsUserControlViewModel(RdpService rdpService)
        {

        }
    }
}
