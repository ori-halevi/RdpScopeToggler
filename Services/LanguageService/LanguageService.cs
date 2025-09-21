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

            // 
            var dict = new ResourceDictionary();
            dict.Source = new Uri("Resources/Language/StringResources." + language + ".xaml", UriKind.Relative);

            // הסרת מילון קודם אם קיים
            var oldDict = Application.Current.Resources.MergedDictionaries
                            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("StringResources."));
            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);

            // טיפול בכיוון ימין לשמאל
            if (language == "he")
                Application.Current.MainWindow.FlowDirection = FlowDirection.RightToLeft;
            else
                Application.Current.MainWindow.FlowDirection = FlowDirection.LeftToRight;
        }

        public void LoadLanguage()
        {
            string language = filesService.GetLanguageFromSettings();

            SetLanguage(language);
        }

    }
}
