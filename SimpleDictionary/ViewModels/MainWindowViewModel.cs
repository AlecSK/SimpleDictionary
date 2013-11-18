using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SimpleDictionary.Dialog;
using SimpleDictionary.Infrastructure;
using SimpleDictionary.Models;
using SimpleDictionary.Utility;

namespace SimpleDictionary.ViewModels
{

    #region вспомогательные классы

    public class RowDetailsVisibilityMode
    {
        public DataGridRowDetailsVisibilityMode VisibilityMode { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return (Description);
        }
    }

    public class CurrentNRangeRule : ValidationRule
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null)
            {
                Debug.Print("Тип: {0}, значение: {1}", value.GetType(), value);
                BindingGroup bindingGroup = value as BindingGroup;
                if (bindingGroup != null)
                {
                    SDValue dictValue = bindingGroup.Items[0] as SDValue;
                    if (dictValue != null)
                    {
                        int currentN;
                        try
                        {
                            currentN = dictValue.CurrentN;
                        }
                        catch (Exception e)
                        {
                            return new ValidationResult(false, "Illegal characters or " + e.Message);
                        }
                        if ((currentN < Min) || (currentN > Max))
                            return new ValidationResult(false, "Код должен быть в диапазоне: " + Min + " - " + Max + ".");
                    }
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class SDValueValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null)
            {
                SDValue dictValue = (value as BindingGroup).Items[0] as SDValue;

                Debug.Assert(dictValue != null, "dictValue != null");
                if (!dictValue.IsValid) return new ValidationResult(false, dictValue.SysMessage);

                if (dictValue.SortN < 0)
                {
                    return new ValidationResult(false, "Вес в сортировке должен быть больше нуля.");
                }
            }
            return ValidationResult.ValidResult;
        }
    }

    #endregion

    [Export(typeof (MainWindowViewModel))]
    public class MainWindowViewModel : ObservableObject
    {
        #region Declarations

        private readonly IDialogService _dialogService;
        private ISDRepository _dataRepository;
        private SDictionary _activeDictionary;
        private SDValue _activeSDValue;
        private ObservableCollection<SearchResult> _searchResults;
        private SearchResult _searchResultSelectedItem;
        private bool _isDataChanged;
        private bool _canNavigate = true;
        private string _filterString;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор тестовых данных.
        /// </summary>
        public MainWindowViewModel()
        {
        }


        [ImportingConstructor]
        public MainWindowViewModel(ISDRepository repositoryInterface, IDialogService dialogInterface)
        {
            if (repositoryInterface == null) throw new ArgumentNullException("repositoryInterface");
            if (dialogInterface == null) throw new ArgumentNullException("dialogInterface");


            _dataRepository = repositoryInterface;
            _dialogService = dialogInterface;
            LoadSearchResults();
        }

        #endregion //Constructor

        #region Свойства элементов интерфейса, не связанных с моделью данных

        private RowDetailsVisibilityMode _rowDetailsVisibilityMode =
            new RowDetailsVisibilityMode
            {
                VisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected,
                Description = "Строка"
            };

        public ObservableCollection<RowDetailsVisibilityMode> RowDetailsVisibilityModesList
        {
            get
            {
                return new ObservableCollection<RowDetailsVisibilityMode>
                {
                    new RowDetailsVisibilityMode
                    {
                        VisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed,
                        Description = "Скрыты"
                    },
                    new RowDetailsVisibilityMode
                    {
                        VisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected,
                        Description = "Строка"
                    },
                    new RowDetailsVisibilityMode
                    {
                        VisibilityMode = DataGridRowDetailsVisibilityMode.Visible,
                        Description = "Все"
                    }
                };
            }
        }


        public RowDetailsVisibilityMode SelectedRowDetailsVisibilityMode
        {
            get { return (_rowDetailsVisibilityMode); }
            set
            {
                _rowDetailsVisibilityMode = value;
                RaisePropertyChanged("SelectedRowDetailsVisibilityMode");
                RaisePropertyChanged("IsRowDetailsVisibleWhenSelected");
                RaisePropertyChanged("IsRowDetailsCollapsed");
                RaisePropertyChanged("IsRowDetailsVisible");
            }
        }


        private void SetRowDetailsVisibility(DataGridRowDetailsVisibilityMode newMode)
        {
            SelectedRowDetailsVisibilityMode =
                RowDetailsVisibilityModesList.FirstOrDefault(m => m.VisibilityMode == newMode);
        }

        public bool IsRowDetailsVisibleWhenSelected
        {
            get
            {
                return SelectedRowDetailsVisibilityMode.VisibilityMode ==
                       DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
            set { if (value) SetRowDetailsVisibility(DataGridRowDetailsVisibilityMode.VisibleWhenSelected); }
        }

        public bool IsRowDetailsCollapsed
        {
            get
            {
                return SelectedRowDetailsVisibilityMode.VisibilityMode == DataGridRowDetailsVisibilityMode.Collapsed;
            }
            set { if (value) SetRowDetailsVisibility(DataGridRowDetailsVisibilityMode.Collapsed); }
        }

        public bool IsRowDetailsVisible
        {
            get { return SelectedRowDetailsVisibilityMode.VisibilityMode == DataGridRowDetailsVisibilityMode.Visible; }
            set { if (value) SetRowDetailsVisibility(DataGridRowDetailsVisibilityMode.Visible); }
        }

        #endregion

        #region Properites

        public String VersionNumber
        {
            get { return _dataRepository.GetVersionNumber(); }
        }

        public DateTime VersionDate
        {
            get { return _dataRepository.GetVersionDate(); }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                if (_filterString != value)
                {
                    _filterString = value;
                    ResetForm();
                    RaisePropertyChanged("FilterString");
                    RaisePropertyChanged("FilterName");
                }
            }
        }

        public string FilterName
        {
            get { return "Фильтр: " + _filterString; }
        }

        private bool CheckDataChange()
        {
            //функция вызывается ротационно из  метода CanSaveExecute
            bool res;
            if (ActiveDictionary == null)
                res = false;
            else
            {
                res = (ActiveDictionary.IsChanged || ActiveDictionary.DictionaryValues.Any(r => r.IsChanged)
                       || ActiveDictionary.DictionaryOptions.Any(r => r.IsChanged));
            }
            IsDataChanged = res;
            CanNavigate = !res;
            return res;
        }

        public bool IsDataChanged
        {
            get { return _isDataChanged; }
            set
            {
                if (_isDataChanged != value)
                {
                    _isDataChanged = value;
                    RaisePropertyChanged("IsDataChanged");
                }
            }
        }

        public SearchResult SearchResultSelectedItem
        {
            get { return _searchResultSelectedItem; }
            set
            {
                _searchResultSelectedItem = value;
                RaisePropertyChanged("SearchResultSelectedItem");
                RecordSelected(value);
            }
        }

        public ObservableCollection<SearchResult> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                RaisePropertyChanged("SearchResults");
            }
        }

        public SDValue ActiveSDValue
        {
            get { return _activeSDValue; }
            set
            {
                _activeSDValue = value;
                RaisePropertyChanged("ActiveSDValue");
                if (value == null)
                {
                    Utils.TraceLog("Выбор элемента", "Пусто", -1);
                }
                else
                {
                    Utils.TraceLog("Выбор элемента", _activeSDValue.ItemName, _activeSDValue.SD);
                }
            }
        }


        public SDictionary ActiveDictionary
        {
            get { return _activeDictionary; }
            set
            {
                _activeDictionary = value;
                RaisePropertyChanged("ActiveDictionary");
            }
        }

        #endregion

        #region Commands

        public ICommand DeleteCommand
        {
            get { return new RelayCommand(DeleteExecute, CanDeleteExecute); }
        }

        public ICommand SaveCommand
        {
            get { return new RelayCommand(SaveExecute, CanSaveExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand(CancelExecute, CanCancelExecute); }
        }

        public ICommand GenerateCommand
        {
            get { return new RelayCommand(GenerateExecute, CanGenerateExecute); }
        }

        public ICommand NewCommand
        {
            get { return new RelayCommand(NewExecute, CanNewExecute); }
        }

        public ICommand NewChildCommand
        {
            get { return new RelayCommand(NewChildExecute, CanNewChildExecute); }
        }

        public ICommand DeleteChildCommand
        {
            get { return new RelayCommand(DeleteChildExecute, CanDeleteChildExecute); }
        }

        public ICommand ClearFilterCommand
        {
            get { return new RelayCommand(ClearFilterExecute, CanClearFilterExecute); }
        }

        #endregion //Commands

        #region Can Methods

        /// <summary>
        /// Блокировка списка словарей и окна поиска
        /// </summary>
        public Boolean CanNavigate
        {
            set
            {
                if (_canNavigate != value)
                {
                    _canNavigate = value;
                    RaisePropertyChanged("CanNavigate");
                }
            }
            get { return (_canNavigate); }
        }


        [DebuggerStepThrough]
        private Boolean CanNewExecute()
        {
            return !IsDataChanged;
        }


        [DebuggerStepThrough]
        private Boolean CanDeleteChildExecute()
        {
            return ActiveSDValue != null;
        }


        [DebuggerStepThrough]
        private Boolean CanSaveExecute()
        {
            return CheckDataChange();
        }


        [DebuggerStepThrough]
        private Boolean CanDeleteExecute()
        {
            return ActiveDictionary != null;
        }


        [DebuggerStepThrough]
        private Boolean CanCancelExecute()
        {
            return CheckDataChange(); //this.ActiveDictionary != null;
        }


        [DebuggerStepThrough]
        private Boolean CanGenerateExecute()
        {
            return !IsDataChanged;
        }


        [DebuggerStepThrough]
        private Boolean CanNewChildExecute()
        {
            return ActiveDictionary != null;
        }


        [DebuggerStepThrough]
        private Boolean CanClearFilterExecute()
        {
            return !string.IsNullOrEmpty(_filterString);
        }

        #endregion //Can Methods

        private void ClearFilterExecute()
        {
            if (!CanClearFilterExecute()) return;
            FilterString = null;
            ResetForm();
        }

        private void SaveExecute()
        {
            if (!CanSaveExecute()) return;
            try
            {
                if (_dataRepository.Save(ActiveDictionary))
                {
                    ResetForm();
                    //Очищаем фильтр и обновляем список словарей
                    FilterString = null;
                }
                else
                {
                    ShowSaveErrors();
                }
                RaisePropertyChanged("VersionNumber");
                RaisePropertyChanged("VersionDate");
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        private void ShowSaveErrors()
        {
            var saveErrors = ActiveDictionary.DictionaryValues.Where(r => r.IsValid == false);
            foreach (SDValue saveError in saveErrors)
            {
                _dialogService.ShowException(saveError.SysMessage);
            }
        }

        private void DeleteExecute()
        {
            if (!CanDeleteExecute() ||
                _dialogService.ShowMessage(Const.CONFIRM_DELETE_DICTIONARY, Const.CONFIRM_DELETE_CAPTION,
                    DialogButton.OKCancel, DialogImage.Question) != DialogResponse.OK) return;
            try
            {
                _dataRepository.Remove(ActiveDictionary);
                ResetForm();
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        private void CancelExecute()
        {
            if (!CanCancelExecute()) return;
            Refresh();
        }

        private void GenerateExecute()
        {
            if (!CanGenerateExecute()) return;
            try
            {
                string fileName = CodeGenerator.WriteConstantsToFile();
                Process.Start("notepad.exe", fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        private void NewExecute()
        {
            ActiveDictionary = _dataRepository.Create();
            SearchResultSelectedItem = null;
            //_filterString = null;
        }

        /// <summary>
        /// Обновление данных из модели.
        /// </summary>
        private void ResetForm()
        {
            int lastSD = -1, lastChildSD = -1;
            if (ActiveDictionary != null)
            {
                lastSD = ActiveDictionary.SD;
                if (ActiveSDValue != null)
                {
                    lastChildSD = ActiveSDValue.SD;
                    ActiveSDValue = null;
                }
                ActiveDictionary = null;
            }
            ////Очищаем репозитарий
            //_repositoryInterface = null;
            //_repositoryInterface = new SDRepository();
            LoadSearchResults();
            SearchResultSelectedItem = _searchResults.FirstOrDefault(r => r.SD == lastSD);
            if (ActiveDictionary != null)
            {
                ActiveSDValue = ActiveDictionary.DictionaryValues.FirstOrDefault(r => r.SD == lastChildSD);
            }
            Utils.TraceLog("вызван метод", "ResetForm");
        }

        /// <summary>
        /// Обновление данных из базы.
        /// </summary>
        private void Refresh()
        {
            int lastSD = -1, lastChildSD = -1;
            if (ActiveDictionary != null)
            {
                lastSD = ActiveDictionary.SD;
                if (ActiveSDValue != null)
                {
                    lastChildSD = ActiveSDValue.SD;
                    ActiveSDValue = null;
                }
                ActiveDictionary = null;
            }
            //Очищаем репозитарий
            _dataRepository = null;
            _dataRepository = new SDRepository();
            LoadSearchResults();
            SearchResultSelectedItem = _searchResults.FirstOrDefault(r => r.SD == lastSD);
            if (ActiveDictionary != null)
            {
                ActiveSDValue = ActiveDictionary.DictionaryValues.FirstOrDefault(r => r.SD == lastChildSD);
            }
            Utils.TraceLog("вызван метод", "Refresh");
        }


        public void LoadSearchResults()
        {
            try
            {
                SearchResults =
                    new ObservableCollection<SearchResult>(_dataRepository.GetSearchResults(_filterString));
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }

        private void RecordSelected(SearchResult searchResult)
        {
            if (searchResult == null) return;
            try
            {
                ActiveDictionary = _dataRepository.GetBySD(searchResult.SD);
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }


        private void NewChildExecute()
        {
            ActiveSDValue = _dataRepository.CreateChild(ActiveDictionary);
        }


        private void DeleteChildExecute()
        {
            if (!CanDeleteChildExecute() ||
                _dialogService.ShowMessage(Const.CONFIRM_DELETE_VALUE, Const.CONFIRM_DELETE_CAPTION,
                    DialogButton.OKCancel, DialogImage.Question) != DialogResponse.OK) return;
            try
            {
                _dataRepository.RemoveChild(_activeDictionary, _activeSDValue);
                //ResetForm();
            }
            catch (Exception ex)
            {
                _dialogService.ShowException(ex.Message);
            }
        }
    }
}