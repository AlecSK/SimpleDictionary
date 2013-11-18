using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleDictionary.Infrastructure;

namespace SimpleDictionary.Models
{
    [Serializable]
    public abstract class CommonClass : ObservableObject
    {
        protected int _sd;
        protected int? _parentSD;
        protected char? _recType;
        protected string _name;
        protected string _description;
        protected int _currentN;
        protected int? _sortN;
        protected DateTime? _creationDate;
        protected DateTime? _changeDate;
        protected string _comment;
        protected bool _isDeleted;
        protected bool _isChanged;
        protected bool _isValid = true;
        protected bool _isEnabled = true;
        protected string _sysMessage;

        protected int? _intValue;
        protected double? _floatValue;
        protected string _stringValue;
        protected DateTime? _dateValue;
        protected string _multiValue;
        protected string _memoValue;

        public int SD
        {
            get { return _sd; }
            set
            {
                _sd = value;
                RaisePropertyChanged("SD");
            }
        }


        public int? ParentSD
        {
            get { return _parentSD; }
            set
            {
                _parentSD = value;
                this.SD = (_parentSD ?? 0)*100 + _currentN;
                RaisePropertyChanged("ParentSD");
                IsChanged = true;
            }
        }


        public char? RecType
        {
            get { return _recType; }
            set
            {
                _recType = value;
                RaisePropertyChanged("RecType");
                IsChanged = true;
            }
        }


        public string ItemName
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("ItemName");
                IsChanged = true;
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
                IsChanged = true;
            }
        }


        public int CurrentN
        {
            get { return _currentN; }
            set
            {
                _currentN = value;
                this.SD = (_parentSD ?? 0)*100 + _currentN;
                RaisePropertyChanged("CurrentN");
                IsChanged = true;
            }
        }


        public int? SortN
        {
            get { return _sortN; }
            set
            {
                _sortN = value;
                RaisePropertyChanged("SortN");
                IsChanged = true;
            }
        }


        public DateTime? CreationDate
        {
            get { return _creationDate; }
            set
            {
                _creationDate = value;
                RaisePropertyChanged("CreationDate");
                IsChanged = true;
            }
        }


        public DateTime? ChangeDate
        {
            get { return _changeDate; }
            set
            {
                _changeDate = value;
                RaisePropertyChanged("ChangeDate");
                IsChanged = true;
            }
        }


        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                RaisePropertyChanged("Comment");
                IsChanged = true;
            }
        }


        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                _isDeleted = value;
                RaisePropertyChanged("IsDeleted");
                IsChanged = true;
            }
        }


        public virtual int? IntValue
        {
            get { return _intValue; }
            set
            {
                _intValue = value;
                RaisePropertyChanged("IntValue");
                IsChanged = true;
            }
        }


        public double? FloatValue
        {
            get { return _floatValue; }
            set
            {
                _floatValue = value;
                RaisePropertyChanged("FloatValue");
                IsChanged = true;
            }
        }


        public string StringValue
        {
            get { return _stringValue; }
            set
            {
                _stringValue = value;
                RaisePropertyChanged("StringValue");
                IsChanged = true;
            }
        }


        public string MultiValue
        {
            get { return _multiValue; }
            set
            {
                _multiValue = value;
                RaisePropertyChanged("MultiValue");
                IsChanged = true;
            }
        }


        public virtual string MemoValue
        {
            get { return _memoValue; }
            set
            {
                _memoValue = value;
                RaisePropertyChanged("MemoValue");
                IsChanged = true;
            }
        }


        public DateTime? DateValue
        {
            get { return _dateValue; }
            set
            {
                _dateValue = value;
                RaisePropertyChanged("DateValue");
                IsChanged = true;
            }
        }


        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                RaisePropertyChanged("IsValid");
            }
        }


        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                RaisePropertyChanged("IsChanged");
            }
        }


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }


        public string SysMessage
        {
            get { return _sysMessage; }
            set
            {
                _sysMessage = value;
                RaisePropertyChanged("SysMessage");
            }
        }
    }
}