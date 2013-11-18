using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using SimpleDictionary.Utility;
using SimpleDictionary.Linq;

namespace SimpleDictionary.Models
{
    [Export(typeof (ISDRepository))]
    public class SDRepository : ISDRepository
    {
        private readonly ObservableCollection<SDictionary> _sDictionaries; //Присвоение значения только в конструкторе


        /// <summary>
        /// Конструктор с автоматической загрузкой данных
        /// </summary>
        public SDRepository()
        {
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                var query = from e in dc.SimpleDictionary
                    where e.RecType == 'D'
                    orderby e.SortN
                    select
                        new SDictionary(e.SD, e.RecType, e.ParentSD, e.CurrentN, e.Name, e.Description, e.SortN,
                            e.IntValue, e.FloatValue, e.StringValue, e.DateValue, e.MultiValue, e.MemoValue,
                            e.Comment, e.IsDeleted, e.CreationDate, e.ChangeDate);
                _sDictionaries = new ObservableCollection<SDictionary>(query.ToList());
            }
            Utils.TraceLog("Загрузка словарей в репозитарий", "SDRepository", _sDictionaries.Count);
        }


        private SDictionary GetLocalCopyBySD(int sd)
        {
            return _sDictionaries.SingleOrDefault(r => r.SD.Equals(sd));
        }


        public SDictionary GetBySD(int sd)
        {
            var simpleDictionary = _sDictionaries.SingleOrDefault(e => e.SD.Equals(sd));
            return simpleDictionary == null ? null : DeepCopy.Make(simpleDictionary);
        }


        public void Remove(SDictionary sDict)
        {
            if (sDict == null) throw new ArgumentNullException("sDict");
            var local = this.GetLocalCopyBySD(sDict.SD);

            //удаление
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                var children = dc.SimpleDictionary.Where(r => r.ParentSD == sDict.SD);
                if (children.Any())
                    throw new DataIntegrityException(Utility.Const.RESTRICT_PARENT_DELETE, sDict.ItemName, sDict.SD);

                var forDeleteList = dc.SimpleDictionary.Where(r => r.SD == sDict.SD);
                foreach (var d in forDeleteList)
                {
                    dc.SimpleDictionary.DeleteOnSubmit(d);
                }
                dc.SubmitChanges();
            }
            _sDictionaries.Remove(local);
        }


        public bool Save(SDictionary sDict)
        {
            bool res = true;
            if (sDict == null) throw new ArgumentNullException("sDict");

            //Сохранение
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                SaveMode saveMode;
                Linq.SimpleDictionary row = dc.SimpleDictionary.FirstOrDefault(r => r.SD == sDict.SD);
                if (row == null)
                {
                    saveMode = SaveMode.Insert;
                    row = new Linq.SimpleDictionary();
                }
                else
                {
                    saveMode = SaveMode.Update;
                }
                row.SD = sDict.SD;
                row.RecType = sDict.RecType;
                row.ParentSD = sDict.ParentSD;
                row.CurrentN = sDict.CurrentN;
                row.Name = sDict.ItemName;
                row.Description = sDict.Description;
                row.SortN = sDict.SortN;

                //сохранение опций
                if (sDict.DictionaryOptions.Any(r => r.IsChanged))
                {
                    row.MemoValue = Serializator<ObservableCollection<SDOption>>.ToXml(sDict.DictionaryOptions);
                    //ObservableCollection<SDOption> o2 = Serializator<ObservableCollection<SDOption>>.FromXml(sDict.MemoValue);
                }

                row.IntValue = sDict.IntValue;
                row.FloatValue = sDict.FloatValue;
                row.StringValue = sDict.StringValue;
                row.DateValue = sDict.DateValue;
                row.MultiValue = sDict.MultiValue;
                //row.MemoValue = sDict.MemoValue;

                row.Comment = sDict.Comment;
                row.IsDeleted = sDict.IsDeleted;
                row.CreationDate = sDict.CreationDate;
                row.ChangeDate = DateTime.Now;
                if (saveMode == SaveMode.Insert)
                {
                    dc.SimpleDictionary.InsertOnSubmit(row);
                }
                dc.SubmitChanges();
                //Сохраняем все параметры этого словаря
                foreach (SDValue dictionaryValue in sDict.DictionaryValues.Where(r => r.IsChanged))
                {
                    res &= dictionaryValue.Save();
                    // Проверяем, не менялся ли вручную номер версии
                    if (sDict.SD == 11 && dictionaryValue.ItemName == "Version")
                    {
                        ReadVersion();
                    }
                }
            }

            WrightVersion();

            sDict.IsValid = true;
            sDict.IsChanged = false;
            sDict.SysMessage = "";
            foreach (SDOption dictionaryOption in sDict.DictionaryOptions.Where(r => r.IsChanged))
            {
                dictionaryOption.IsChanged = false;
            }

            var local = this.GetLocalCopyBySD(sDict.SD);
            if (local != null) _sDictionaries.Remove(local);
            _sDictionaries.Add(sDict);
            return res;
        }


        public int GetCount()
        {
            return _sDictionaries.Count;
        }


        public SDictionary Create()
        {
            int maxN;
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                try
                {
                    maxN = dc.SimpleDictionary.Where(r => r.ParentSD == 0).Select(r => r.CurrentN).Max();
                }
                catch (ArgumentNullException)
                {
                    maxN = 10;
                }
            }
            return new SDictionary(maxN + 1);
        }


        public IEnumerable<SearchResult> GetSearchResults(string filterString = null)
        {
            IEnumerable<SearchResult> query;

            if (string.IsNullOrEmpty(filterString))
            {
                query = from e in _sDictionaries
                    orderby e.SortN
                    select new SearchResult(e.SD, e.ItemName, e.Description, e.SortN, e.DictionaryValues.Count());
            }
            else
            {
                query = from e in _sDictionaries
                    where
                        e.ItemName.ToUpper().Contains(filterString.ToUpper()) ||
                        e.Description.ToUpper().Contains(filterString.ToUpper())
                    orderby e.SortN
                    select new SearchResult(e.SD, e.ItemName, e.Description, e.SortN, e.DictionaryValues.Count());
            }
            return new ObservableCollection<SearchResult>(query.ToList());
        }


        public void RemoveChild(SDictionary sDict, SDValue dictValue)
        {
            if (sDict == null) throw new ArgumentNullException("sDict");
            if (dictValue == null) throw new ArgumentNullException("dictValue");

            SDictionary localDict = this.GetLocalCopyBySD(sDict.SD);

            SDValue localValue = localDict.DictionaryValues.SingleOrDefault(r => r.SD.Equals(dictValue.SD));


            if (localValue != null)
            {
                //Элемент был сохранен ранее
                using (var dc = new SDLinqDataContext(App.ConnectionString))
                {
                    var forDeleteList = dc.SimpleDictionary.Where(r => r.SD == localValue.SD);
                    foreach (var d in forDeleteList)
                        dc.SimpleDictionary.DeleteOnSubmit(d);
                    dc.SubmitChanges();
                }
                localDict.DictionaryValues.Remove(localValue);
            }

            bool res = sDict.DictionaryValues.Remove(dictValue);
            Utils.TraceLog("Исключение элемента из коллекции", dictValue.ItemName, Convert.ToInt32(res));
        }


        public SDValue CreateChild(SDictionary simpleDictionary)
        {
            SDValue child = new SDValue(simpleDictionary);
            simpleDictionary.DictionaryValues.Add(child);
            return child;
        }

        #region Версия словаря

        private string _majorVersion;
        private int _minorVersion;
        private DateTime _versionDate;

        private void WrightVersion()
        {
            int newMinorVersion = _minorVersion + 1;
            DateTime newVersionDate = DateTime.Now;

            var d = GetLocalCopyBySD(11);
            var v = d.DictionaryValues.FirstOrDefault(r => r.ItemName == "Version");
            if (v != null)
            {
                v.IntValue = newMinorVersion;
                v.DateValue = newVersionDate;
                v.IsChanged = false;
            }
            //d.IsChanged = false;

            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                var row = dc.SimpleDictionary.FirstOrDefault(r => r.Name == "Version");
                if (row != null)
                {
                    row.StringValue = _majorVersion;
                    row.IntValue = newMinorVersion;
                    row.DateValue = newVersionDate;
                    dc.SubmitChanges();
                }
            }
            _minorVersion = newMinorVersion;
            _versionDate = newVersionDate;
        }

        private void ReadVersion()
        {
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                var row = dc.SimpleDictionary.FirstOrDefault(r => r.Name == "Version");
                if (row != null)
                {
                    _majorVersion = row.StringValue ?? "A";
                    _minorVersion = row.IntValue != null ? (int) row.IntValue : 0;
                    _versionDate = row.DateValue != null ? (DateTime) row.DateValue : DateTime.Now;
                }
            }
        }

        public string GetVersionNumber()
        {
            if (_majorVersion == null) ReadVersion();
            return _majorVersion + "." + _minorVersion;
        }


        public DateTime GetVersionDate()
        {
            if (_majorVersion == null) ReadVersion();
            return _versionDate;
        }

        #endregion
    }
}