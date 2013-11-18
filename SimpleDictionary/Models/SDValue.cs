using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SimpleDictionary.Linq;
using SimpleDictionary.Utility;

namespace SimpleDictionary.Models
{
    [Serializable]
    public class SDValue : CommonClass, IEditableObject, IDataErrorInfo
    {
        /// <summary>
        /// Конструктор для тестовых данных.
        /// </summary>
        public SDValue()
        {
        }


        /// <summary>
        /// Конструктор класса <see cref="SDValue"/> для загрузки данных из базы.
        /// </summary>
        internal SDValue(int sd, char? recType, int? parentSD, int currentN, string name, string description, int? sortN,
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
        /// Конструктор нового пустого элемента (на базе информации из родительского словаря).
        /// </summary>
        /// <param name="parentDict">Родительский словарь.</param>
        public SDValue(SDictionary parentDict)
        {
            int maxN;
            ParentSD = 0;
            RecType = 'V';
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                try
                {
                    maxN = dc.SimpleDictionary.Where(r => r.ParentSD == parentDict.SD).Select(r => r.CurrentN).Max();
                }
                catch (InvalidOperationException)
                {
                    maxN = 10;
                }
            }
            int maxLocalN = 10;
            try
            {
                maxLocalN = parentDict.DictionaryValues.Max(r => r.CurrentN);
                    //Максимальный в текущем экземпляре приложения
            }
            catch (InvalidOperationException)
            {
            }

            maxN = maxN > maxLocalN ? maxN : maxLocalN;
            this.ParentSD = parentDict.SD;
            this.CurrentN = maxN + 1;
            this.ItemName = "Value" + CurrentN;
            this.Description = "Словарный параметр " + CurrentN;
            this.SortN = CurrentN*10;
            this.CreationDate = DateTime.Now;
            this.ChangeDate = DateTime.Now;
            this.IsChanged = true;
        }


        public bool Save()
        {
            using (SDLinqDataContext dc = new SDLinqDataContext(App.ConnectionString))
            {
                SaveMode saveMode;
                try
                {
                    Linq.SimpleDictionary row = dc.SimpleDictionary.FirstOrDefault(r => r.SD == _sd);
                    if (row == null)
                    {
                        saveMode = SaveMode.Insert;
                        row = new Linq.SimpleDictionary();
                    }
                    else
                        saveMode = SaveMode.Update;

                    this.ChangeDate = DateTime.Now;

                    row.SD = this.SD;
                    row.RecType = this.RecType;
                    row.ParentSD = this.ParentSD;
                    row.CurrentN = this.CurrentN;
                    row.Name = this.ItemName;
                    row.Description = this.Description;
                    row.SortN = this.SortN;
                    row.Comment = this.Comment;
                    row.IsDeleted = this.IsDeleted;
                    row.CreationDate = this.CreationDate;
                    row.ChangeDate = this.ChangeDate;

                    row.IntValue = this.IntValue;
                    row.FloatValue = this.FloatValue;
                    row.StringValue = this.StringValue;
                    row.DateValue = this.DateValue;
                    row.MultiValue = this.MultiValue;
                    row.MemoValue = this.MemoValue;

                    if (saveMode == SaveMode.Insert)
                    {
                        dc.SimpleDictionary.InsertOnSubmit(row);
                    }
                    dc.SubmitChanges();
                    _isChanged = false;
                    this.SysMessage = "";
                    this.IsValid = true;
                }
                catch (Exception e)
                {
                    this.SysMessage = e.Message;
                    this.IsValid = false;
                }
            }
            Utils.TraceLog("сохранение параметра", this.ItemName, Convert.ToInt32(this.IsValid));
            return this.IsValid;
        }

        #region Implementation of IEditableObject

        private SDValue backupCopy;
        private bool inEdit;


        /// <summary>
        /// Начинает редактирование объекта.
        /// </summary>
        public void BeginEdit()
        {
            if (inEdit) return;
            inEdit = true;
            backupCopy = this.MemberwiseClone() as SDValue;
        }

        /// <summary>
        /// Помещает в базовый объект изменения, выполненные с момента последнего вызова метода <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> или <see cref="M:System.ComponentModel.IBindingList.AddNew"/>.
        /// </summary>
        public void EndEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            backupCopy = null;
        }

        /// <summary>
        /// Уничтожает изменения, выполненные после последнего вызова метода <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/>.
        /// </summary>
        public void CancelEdit()
        {
            if (!inEdit) return;
            inEdit = false;

            this.CurrentN = backupCopy.CurrentN;
            this.ItemName = backupCopy.ItemName;
            this.Description = backupCopy.Description;
            this.SortN = backupCopy.SortN;
            this.Comment = backupCopy.Comment;
            this.IsDeleted = backupCopy.IsDeleted;
            this.CreationDate = backupCopy.CreationDate;
            this.ChangeDate = backupCopy.ChangeDate;

            this.IntValue = backupCopy.IntValue;
            this.FloatValue = backupCopy.FloatValue;
            this.StringValue = backupCopy.StringValue;
            this.DateValue = backupCopy.DateValue;
            this.MultiValue = backupCopy.MultiValue;
            this.MemoValue = backupCopy.MemoValue;

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
                    if (!Utils.IsValidName(_name)) result = "Наименование содержит недопустимые символы!";
                    else if (Utils.IsReservedWord(_name)) result = "Нельзя использовать зарезервированные слова C#!";
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