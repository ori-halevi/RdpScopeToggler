using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Windows.Forms.Design.AxImporter;

namespace RdpScopeToggler.ViewModels
{
    public class SettingsWindowViewModel : BindableBase
    {
        private string selectedLanguage;
        public string SelectedLanguage
        {
            get { return selectedLanguage; }
            set { SetProperty(ref selectedLanguage, value); }
        }

        public ObservableCollection<string> LanguagesOptions { get; }

        public SettingsWindowViewModel()
        {
            LanguagesOptions = new ObservableCollection<string>
            {
                "עברית",
                "English"
            };
            SelectedLanguage = LanguagesOptions[0];
        }
    }
}
