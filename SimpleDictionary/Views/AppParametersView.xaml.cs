using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleDictionary.ViewModels;

namespace SimpleDictionary.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class AppParametersView : UserControl
    {
        public AppParametersView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ((AppParametersViewModel) this.DataContext).ConnectExecute();
            }
            catch
            {
            }
        }
    }
}