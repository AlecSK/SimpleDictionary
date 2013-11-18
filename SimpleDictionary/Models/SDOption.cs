using System;
using System.Xml;
using SimpleDictionary.Infrastructure;
using System.Xml.Serialization;

namespace SimpleDictionary.Models
{
    [Serializable]
    public class SDOption : ObservableObject
    {
        private string _name;
        private string _description;
        private bool _generateStruct;
        private bool _generateEnum;
        private bool _addComment;
        private bool _isChanged;

        public SDOption()
        {
        } //необходимо для сериализации

        public SDOption(string name, string description, bool generateStruct)
        {
            _name = name;
            _description = description;
            _generateStruct = generateStruct;
        }

        [XmlAttribute]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
                IsChanged = true;
            }
        }

        [XmlAttribute]
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

        [XmlAttribute]
        public bool GenerateStruct
        {
            get { return _generateStruct; }
            set
            {
                _generateStruct = value;
                RaisePropertyChanged("GenerateStruct");
                IsChanged = true;
            }
        }

        [XmlAttribute]
        public bool GenerateEnum
        {
            get { return _generateEnum; }
            set
            {
                _generateEnum = value;
                RaisePropertyChanged("GenerateEnum");
                IsChanged = true;
            }
        }

        [XmlAttribute]
        public bool AddComment
        {
            get { return _addComment; }
            set
            {
                _addComment = value;
                IsChanged = true;
                RaisePropertyChanged("AddComment");
            }
        }

        [XmlIgnoreAttribute]
        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                RaisePropertyChanged("IsChanged");
            }
        }

        //[OnDeserialized()] - не работает с XmlSerializer
        //private void OnDeserialized(StreamingContext context)
        //{
        //    // Вызывается по завершении процесса десериализации.
        //    _isChanged = true;
        //}

        //[OnDeserializingAttribute] - не работает с XmlSerializer
        //private void OnDeserializing(StreamingContext context)
        //{
        //    // Вызывается во время процесса десериализации.
        //    _isChanged = false;
        //}
    }
}