using System;
using SimpleDictionary.Infrastructure;

namespace SimpleDictionary.Models
{
    [Serializable]
    public class ConnString : ObservableObject
    {
        protected string _name;
        protected string _value;

        internal ConnString(string Name, string Value)
        {
            _name = Name;
            _value = Value;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }


        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }


        public override string ToString()
        {
            return _name;
        }
    }
}