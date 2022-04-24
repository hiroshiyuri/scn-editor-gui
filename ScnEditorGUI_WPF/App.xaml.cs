using ScnEditorGUI_WPF.Helpers;
using System;
using System.Windows;

namespace ScnEditorGUI_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string theme = "Light";

            if(WindowsMode.GetTheme() == WindowsMode.Theme.Dark)
            {
                theme = "Dark";
            }

            ResourceDictionary resources = new ResourceDictionary
            {
                Source = new Uri($"/Themes/{theme}Theme.xaml", UriKind.Relative)
            };
            this.Resources.MergedDictionaries.Add(resources);
        }
    }
}
