using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SimpleDictionary.Linq;
using SimpleDictionary.Properties;
using SimpleDictionary.Utility;

namespace SimpleDictionary.Models
{
    [Serializable]
    public class SDictionary : CommonClass, IEditableObject, IDataErrorInfo
    {
        private ObservableCollection<SDValue> _dictionaryValues;
        private ObservableCollection<SDOption> _options;

        /// <summary>
        /// Конструктор для тестовых данных .
        /// </summary>
        public SDictionary()
        {
        }

        /// <summary>
        /// Конструктор класса <see cref="SDictionary"/> для загрузки данных из базы.
        /// </summary>
        internal SDictionary(int sd, char? recType, int? parentSD, int currentN, string name, string description,
            int? sortN,
            int? intValue, double? floatValue, string stringValue, DateTime? dateValue, string multiValue,
            string memoValue,
            string comment, bool isDeleted, DateTime? creationDate, DateTime? changeDate)
        {
            _sd = sd;
            _recType = recType;
            _parentSD = parentSD;
            _currentN = currentN;
            _name = name;
            _description = description;
            _sortN = sortN;
            _comment = comment;
            _isDeleted = isDeleted;
            _creationDate = creationDate;
            _changeDate = changeDate;

            _intValue = intValue;
            _floatValue = floatValue;
            _stringValue = stringValue;
            _dateValue = dateValue;
            _multiValue = multiValue;
            _memoValue = memoValue;
        }


        /// <summary>
        /// Конструктор нового элемента <see cref="SDictionary" /> .
        /// </summary>
        public SDictionary(int newDictionaryNumber)
        {
            ParentSD = 0;
            RecType = 'D';
            this.CurrentN = newDictionaryNumber;
            this.ItemName = "Dictionary" + CurrentN;
            this.Description = "Словарь параметров " + CurrentN;
            this.SortN = CurrentN*10;
            this.CreationDate = DateTime.Now;
            this.ChangeDate = DateTime.Now;
            this.IsChanged = true;
        }


        public ObservableCollection<SDValue> DictionaryValues
        {
            get
            {
                if (_dictionaryValues == null) DictionaryValues = LoadDictionaryValues();
                return _dictionaryValues;
            }
            set
            {
                _dictionaryValues = value;
                RaisePropertyChanged("DictionaryValues");
            }
        }

        public ObservableCollection<SDOption> DictionaryOptions
        {
            get { return _options ?? (_options = LoadOptions()); }
            set
            {
                _options = value;
                RaisePropertyChanged("DictionaryOptions");
                Utils.TraceLog("Изменено ствойство", "DictionaryOptions", _options.Count);
            }
        }

        private ObservableCollection<SDValue> LoadDictionaryValues()
        {
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                var query = from e in dc.SimpleDictionary
                    where e.ParentSD == this.SD
                    orderby e.SortN
                    select new SDValue(e.SD, e.RecType, e.ParentSD, e.CurrentN, e.Name, e.Description, e.SortN,
                        e.IntValue, e.FloatValue, e.StringValue, e.DateValue, e.MultiValue, e.MemoValue,
                        e.Comment, e.IsDeleted, e.CreationDate, e.ChangeDate);
                _dictionaryValues = new ObservableCollection<SDValue>(query.ToList());
            }
            Utils.TraceLog("Загрузка значений в", this.ItemName, _dictionaryValues.Count);
            return _dictionaryValues;
        }


        private ObservableCollection<SDOption> LoadOptions()
        {
            if (String.IsNullOrEmpty(_memoValue) || !Utils.IsValidXML(_memoValue, Resources.SDOptions))
            {
                //Заполняем набор опций значениями по-умолчанию.
                _options = new ObservableCollection<SDOption>
                {
                    new SDOption("IntValue", "Целочисленное", false),
                    new SDOption("FloatValue", "Вещественное", false),
                    new SDOption("StringValue", "Строковое", false),
                    new SDOption("DateValue", "Дата", false),
                    new SDOption("MultiValue", "Множественное", false),
                    new SDOption("MemoValue", "Многострочный текст", false)
                };
            }
            else
            {
                //Загружаем из MemoValue
                _options = Serializator<ObservableCollection<SDOption>>.FromXml(_memoValue);
                foreach (SDOption dictionaryOption in _options.Where(r => r.IsChanged))
                {
                    dictionaryOption.IsChanged = false;
                }
            }
            return _options;
        }

        #region Implementation of IEditableObject

        private SDictionary _backupCopy;
        private bool _inEdit;


        /// <summary>
        /// Начинает редактирование объекта.
        /// </summary>
        public void BeginEdit()
        {
            if (_inEdit) return;
            _inEdit = true;
            _backupCopy = this.MemberwiseClone() as SDictionary;
        }

        /// <summary>
        /// Помещает в базовый объект изменения, выполненные с момента последнего вызова метода <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> или <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
        /// </summary>
        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null;
        }

        /// <summary>
        /// Уничтожает изменения, выполненные после последнего вызова метода <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/>.
        /// </summary>
        public void CancelEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;

            this.CurrentN = _backupCopy.CurrentN;
            this.ItemName = _backupCopy.ItemName;
            this.Description = _backupCopy.Description;
            this.SortN = _backupCopy.SortN;
            this.Comment = _backupCopy.Comment;
            this.IsDeleted = _backupCopy.IsDeleted;
            this.CreationDate = _backupCopy.CreationDate;
            this.ChangeDate = _backupCopy.ChangeDate;

            this.IntValue = _backupCopy.IntValue;
            this.FloatValue = _backupCopy.FloatValue;
            this.StringValue = _backupCopy.StringValue;
            this.DateValue = _backupCopy.DateValue;
            this.MultiValue = _backupCopy.MultiValue;
            this.MemoValue = _backupCopy.MemoValue;

            this.IsChanged = false;
        }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Возвращает сообщение об ошибке для свойства с заданным именем.
        /// </summary>
        /// <returns>
        /// Сообщение об ошибке для свойства.Значением по умолчанию является пустая строка ("").
        /// </returns>
        /// <param name="columnName">Имя свойства, для которого возвращается сообщение об ошибке. </param>
        public string this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == "ItemName")
                {
                    if (_currentN > 11)
                    {
                        if (!Utils.IsValidName(_name)) result = "Наименование содержит недопустимые символы!";
                        else if (Utils.IsReservedWord(_name)) result = "Нельзя использовать зарезервированные слова C#!";
                    }
                    else
                    {
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Возвращает сообщение об ошибке, показывающее причину отказа в данном объекте.
        /// </summary>
        /// <returns>
        /// Сообщение об ошибке, показывающее причину отказа в данном объекте.Значением по умолчанию является Null.
        /// </returns>
        public string Error
        {
            get { return this.IsValid ? null : this.SysMessage; }
        }

        #endregion


    }
}