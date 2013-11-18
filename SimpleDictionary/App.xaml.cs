using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using SimpleDictionary.Dialog;
//using SimpleDictionary.Models;
using SimpleDictionary.ViewModels;
using SimpleDictionary.Views;

namespace SimpleDictionary
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow View;
        public static string ConnectionString; //прописывается в классе AppParametersViewModel

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Ensure the current culture passed into bindings 
            // is the OS culture. By default, WPF uses en-US 
            // as the culture, regardless of the system settings.

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof (FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            View = new MainWindow();
            View.Show();
            //Подключение DataContext производится в классе AppParametersViewModel
        }
    }
}