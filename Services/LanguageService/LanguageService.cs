using RdpScopeToggler.Services.FilesService;
using System;
using System.Linq;
using System.Windows;

namespace RdpScopeToggler.Services.LanguageService
{
    public class LanguageService : ILanguageService
    {
        // TODO: Make all langs Enum!
        public string SelectedLanguage { get; set; } = "en";

        IFilesService filesService;
        public LanguageService(IFilesService filesService)
        {
            this.filesService = filesService;
        }

        public void SetLanguage(string language)
        {
            SelectedLanguage = language;

            // Update settings file
            filesService.WriteLanguageToSettings(language);

            // טיפול בכיוון ימין לשמאל
            Application.Current.MainWindow.FlowDirection =
                language == "he" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // נתיב ה־dictionary
            string dictionaryPath = $"Resources/Language/StringResources.{language}.xaml";

            // מחפשים אם כבר קיים dictionary של שפה
            var existing = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("StringResources."));
            if (existing != null)
                Application.Current.Resources.MergedDictionaries.Remove(existing);

            // מוסיפים את dictionary החדש
            var dict = new ResourceDictionary
            {
                Source = new Uri(dictionaryPath, UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }


        public void LoadLanguage()
        {
            string language = filesService.GetLanguageFromSettings();

            SetLanguage(language);
        }

    }
}
