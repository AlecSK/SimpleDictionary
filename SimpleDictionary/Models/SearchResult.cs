using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDictionary.Models
{
    public class SearchResult
    {
        private string _itemName;
        private string _description;
        private int _sd;
        private int? _sortNumber;
        private int _valuesCount;

        public SearchResult()
        {
        }

        public int SD
        {
            get { return _sd; }
        }

        public string Description
        {
            get { return _description; }
        }

        public string ItemName
        {
            get { return _itemName; }
        }

        public string FullName
        {
            get { return String.Format("{0} - {1} ( {2} )", _sd, _itemName, _valuesCount); }
        }

        public int ValuesCount
        {
            get { return _valuesCount; }
        }

        public int? SortNumber
        {
            get { return _sortNumber; }
        }

        //Конструктор
        public SearchResult(int sd, string name, string description, int? sortNumber, int valuesCount)
        {
            _sd = sd;
            _itemName = name;
            _description = description;
            _sortNumber = sortNumber;
            _valuesCount = valuesCount;
        }
    }
}