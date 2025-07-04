using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RdpScopeToggler.Managers
{
    public static class LanguageManager
    {
        public static void ChangeLanguage(string culture)
        {
            var dict = new ResourceDictionary();
            switch (culture)
            {
                case "he":
                    dict.Source = new Uri("Resources/Language/StringResources.he.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Language/StringResources.en.xaml", UriKind.Relative);
                    break;
            }

            // הסרת מילון קודם אם קיים
            var oldDict = Application.Current.Resources.MergedDictionaries
                            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("StringResources."));
            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);

            // טיפול בכיוון ימין לשמאל
            if (culture == "he")
                Application.Current.MainWindow.FlowDirection = FlowDirection.RightToLeft;
            else
                Application.Current.MainWindow.FlowDirection = FlowDirection.LeftToRight;
        }


    }

}
