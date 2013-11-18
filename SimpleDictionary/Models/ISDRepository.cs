using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SimpleDictionary.Models
{
    public interface ISDRepository
    {
        SDictionary GetBySD(int sd);
        void Remove(SDictionary simpleDictionary);
        bool Save(SDictionary simpleDictionary);
        int GetCount();
        SDictionary Create();
        IEnumerable<SearchResult> GetSearchResults(string filterString = null);
        void RemoveChild(SDictionary simpleDictionary, SDValue dictionaryValue);
        SDValue CreateChild(SDictionary simpleDictionary);

        /// <summary>
        /// Версия набора данных
        /// </summary>
        /// <returns>Строка, содержащая major и minor версию набора данных</returns>
        string GetVersionNumber();

        /// <summary>
        /// Дата последнего изменения набора данных
        /// </summary>
        /// <returns>Дата набора данных</returns>
        DateTime GetVersionDate();
    }
}