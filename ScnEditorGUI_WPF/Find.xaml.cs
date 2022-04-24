using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows;

namespace ScnEditorGUI_WPF
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : Window
    {
        readonly MainWindow _parentWindow;

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            this._parentWindow.FindString(textSearch.Text);
            this.Close();
        }

        public Find(MainWindow parentWindow)
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;
            this.Resources.MergedDictionaries.Add(App.Current.Resources.MergedDictionaries[0]);
            this._parentWindow = parentWindow;
            this.textSearch.Focus();

            string fontSize = ConfigurationManager.AppSettings["font_size"];
            this.FontSize = float.Parse(fontSize);
        }
    }
}
