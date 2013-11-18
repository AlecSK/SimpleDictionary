using System;
using SimpleDictionary.Properties;

namespace SimpleDictionary.Utility
{
    /// <summary>
    /// Константы проекта
    /// </summary>
    /// <remarks>
    /// Примечания и заметки
    /// </remarks>
    public static class Const
    {
        public static readonly string[] ListItemsSeparators = {", \r\n", ", ", ",\r\n", ","};
        public static readonly string[] CrLf = {"\r\n"};

        public const string CONFIRM_DELETE_CAPTION = "Подтвердите удаление";
        public const string CONFIRM_DELETE_DICTIONARY = "Этот словарь будет удален!";
        public const string CONFIRM_DELETE_VALUE = "Этот параметр будет удален!";
        public const string RECORD_NOT_FOUND = "Не найдена запись с идентификатором ";

        public const string RESTRICT_PARENT_DELETE =
            "У объекта {0}[{1}] найдены зависымые данные. Необходимо удалить их первыми.";

        //public static string ConnectionString = "Data Source=.;initial catalog=SupportDB;Integrated Security=True;Connect Timeout=30";

        //static Const()
        //{
        //    string useConnection = Settings.Default.UseConnection;
        //    ConnectionString = Settings.Default[useConnection].ToString();
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SaveMode : int
    {
        Insert = 0,
        Update = 1
    }
}