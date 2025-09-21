using System;
using System.Windows;

namespace RdpScopeToggler.Helpers
{
    public static class TranslationHelper
    {
        /// <summary>
        /// Returns the translation for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns> The translation </returns>
        public static string Translate(string key)
        {
            var result = Application.Current.TryFindResource(key) as string;
            if (result == null)
            {
                Console.WriteLine($"Missing translation for key: {key}");
                return key;
            }
            return result;
        }
    }
}