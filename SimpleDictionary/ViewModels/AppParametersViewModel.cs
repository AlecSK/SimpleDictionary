using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SimpleDictionary.Dialog;
using SimpleDictionary.Infrastructure;
using SimpleDictionary.Models;
using SimpleDictionary.Properties;

namespace SimpleDictionary.ViewModels
{
    public class AppParametersViewModel : ObservableObject
    {
        private ConnString _activeConnString;
        private static ObservableCollection<ConnString> _connStringList;
        private bool _isExpanded = true;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AppParametersViewModel()
        {
            // Get the ConnectionStrings collection.
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;

            if (_connStringList == null) _connStringList = new ObservableCollection<ConnString>();

            // Get the collection elements.
            foreach (ConnectionStringSettings c in connections)
            {
                string name = c.Name.Split('.').Last();
                string provider = c.ProviderName;
                string connString = c.ConnectionString;
                if (provider == "System.Data.SqlClient") _connStringList.Add(new ConnString(name, connString));
            }

            //восстанавливаем параметры последнего подключения по наименованию
            string lastConnName = Utility.RegistryHelper.GetSetting("AppParameters", "LastConnName",
                Settings.Default.UseConnection);
            ActiveConnString = _connStringList.FirstOrDefault(r => r.Name == lastConnName);
        }

        #region Свойства

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public ConnString ActiveConnString
        {
            get { return _activeConnString; }
            set
            {
                _activeConnString = value;
                RaisePropertyChanged("ActiveConnString");
            }
        }

        public ObservableCollection<ConnString> ConnStringList
        {
            get { return _connStringList; }
        }

        #endregion

        public ICommand ConnectCommand
        {
            get { return new RelayCommand(ConnectExecute, CanConnectExecute); }
        }


        [DebuggerStepThrough]
        private Boolean CanConnectExecute()
        {
            return _activeConnString != null;
        }

        public ICommand ShowDDLScriptCommand
        {
            get { return new RelayCommand(ShowDDLScriptExecute); }
        }

        public void ShowDDLScriptExecute()
        {
            string fileName = Path.Combine(Path.GetTempPath(), @"CreateTable.txt");
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
                {
                    sw.WriteLine(Resources.CreateTable);
                    sw.Flush();
                    //sw.Close();
                }
                Process.Start("notepad.exe", fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        public ICommand ShowInsertDataScriptCommand
        {
            get { return new RelayCommand(ShowInsertDataScriptExecute); }
        }

        public void ShowInsertDataScriptExecute()
        {
            string fileName = Path.Combine(Path.GetTempPath(), @"InsertData.txt");
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
                {
                    sw.WriteLine(Resources.InsertData);
                    sw.Flush();
                    //sw.Close();
                }
                Process.Start("notepad.exe", fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        public ICommand ShowSchemaCommand
        {
            get { return new RelayCommand(ShowSchemaExecute); }
        }

        public void ShowSchemaExecute()
        {
            string fileName = Path.Combine(Path.GetTempPath(), @"Схема.png");
            try
            {
                Resources.Схема.Save(fileName);
                Process.Start("iexplore.exe", fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }


        public ICommand ShowSampleCommand
        {
            get { return new RelayCommand(ShowSampleExecute); }
        }

        public void ShowSampleExecute()
        {
            string fileName = Path.Combine(Path.GetTempPath(), @"Sample.png");
            try
            {
                Resources.CS_Code_Sample.Save(fileName);
                Process.Start("iexplore.exe", fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        #region Подключение DataContext

        public void ConnectExecute()
        {
            if (Utility.Utils.CheckSDConnection(ActiveConnString.Value))
            {
                App.ConnectionString = ActiveConnString.Value;

                if (Compose())
                {
                    App.View.DataContext = MainWindowViewModel;
                    ((MainWindowViewModel) MainWindowViewModel).LoadSearchResults();
                    this.IsExpanded = false;
                }
                Utility.RegistryHelper.SaveSetting("AppParameters", "LastConnName", ActiveConnString.Name);
            }
        }

        private bool Compose()
        {
            // An aggregate catalog can contain one or more types of catalog
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            CompositionContainer container;
            container = new CompositionContainer(catalog);
            try
            {
                // Perform the composition
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString());
                return false;
            }
            return true;
        }

        [Import(typeof (MainWindowViewModel))]
        private object MainWindowViewModel { get; set; }

        private IDialogService _dialogService;

        [Import(typeof (IDialogService))]
        private object ModalDialogService
        {
            set { _dialogService = (IDialogService) value; }
        }

        #endregion
    }
}