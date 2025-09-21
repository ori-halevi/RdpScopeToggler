namespace RdpScopeToggler.Services.LanguageService
{
    public interface ILanguageService
    {
        public string SelectedLanguage { get; set; }
        void SetLanguage(string language);
        void LoadLanguage();
    }
}
