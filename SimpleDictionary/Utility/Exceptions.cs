using System;

namespace SimpleDictionary.Utility
{
    [Serializable]
    public class BaseException : Exception
    {
        protected BaseException()
            : base()
        {
        }

        protected BaseException(string message)
            : base(message)
        {
        }
    }

    public class DataNotFoundException : BaseException
    {
        public DataNotFoundException()
            : base()
        {
        }

        public DataNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class DataValidationException : BaseException
    {
        public DataValidationException()
            : base()
        {
        }

        public DataValidationException(string message)
            : base(message)
        {
        }
    }

    public class DataIntegrityException : BaseException
    {
        public DataIntegrityException()
            : base()
        {
        }

        public DataIntegrityException(string message)
            : base(message)
        {
        }

        public DataIntegrityException(string format, string entityName, int id)
            : base(string.Format(format, entityName, id))
        {
        }
    }

    public class MSPException : BaseException
    {
        public MSPException()
            : base()
        {
        }

        public MSPException(string message)
            : base(message)
        {
        }
    }

    public class XmlFormatException : BaseException
    {
        public XmlFormatException()
            : base()
        {
        }

        public XmlFormatException(string message)
            : base(message)
        {
        }
    }
}