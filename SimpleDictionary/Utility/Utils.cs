using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Win32;
using SimpleDictionary.Linq;

namespace SimpleDictionary.Utility
{
    public static class Utils
    {
        private static List<string> _reservedWords;

        public enum TraceModeEnum
        {
            None,
            Minimal,
            Full
        }

        public static TraceModeEnum TraceMode = TraceModeEnum.Full;

        public static void TraceLog(string message, string source, int id = 0)
        {
            if (TraceMode != TraceModeEnum.None)
            {
                Debug.Print("{0}: {1} - {2}[{3}]", DateTime.Now.ToLongTimeString(), message, source, id);
            }
        }

        // Writes specified text to the log file.
        public static void WriteToLog(string textToLog)
        {
            try
            {
                // Define log file path and name. 
                string currentLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    AppDomain.CurrentDomain.FriendlyName, @"Log.txt");
                //string CurrentLogFilePath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + @"Log.txt";
                StreamWriter sw = new StreamWriter(currentLogFilePath, true);
                // Write data to log file. 
                sw.WriteLine(DateTime.Now + ": " + textToLog);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }


        public static bool IsValidName(string varName)
        {
            if (String.IsNullOrEmpty(varName) || !Char.IsLetter(varName, 0)) return false;

            bool res = true;
            for (int i = 1; i < varName.Length; i++)
            {
                res &= (Char.IsLetterOrDigit(varName, i) || varName[i] == '_');
            }
            return res;
        }


        public static bool IsReservedWord(string word)
        {
            if (_reservedWords == null)
            {
                string s = ReadMultiTextParameter("ReservedWords");
                _reservedWords = s.Split(Const.ListItemsSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
                Debug.Print("Reserved words loaded: {0}", _reservedWords.Count);
            }

            return _reservedWords.Any(w => w == word);
        }


        public static string ReadStringParameter(string paramName)
        {
            string s;
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                //var dictSD = dc.SimpleDictionary.Where(e => e.Name == dictionaryName & e.RecType == 'D').Select(e => e.SD).FirstOrDefault();
                const int dictSD = 11; //Номер словаря с одиночными параметрами
                s =
                    dc.SimpleDictionary.Where(e => e.Name == paramName & e.ParentSD == dictSD)
                        .Select(e => e.StringValue)
                        .FirstOrDefault();
            }
            return s;
        }


        public static string ReadMultiTextParameter(string paramName)
        {
            string s;
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                const int dictSD = 11; //Номер словаря с одиночными параметрами
                s =
                    dc.SimpleDictionary.Where(e => e.Name == paramName & e.ParentSD == dictSD)
                        .Select(e => e.MemoValue)
                        .FirstOrDefault();
            }
            return s;
        }

        #region Проверка XML на соответствие схеме

        /// <summary>
        /// Делегат для метода проверки соответсвия документа XML его схеме.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ValidationEventArgs" /> instance containing the event data.</param>
        /// <exception cref="XmlFormatException">Исключение типа XmlFormatException.</exception>
        private static void XmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Debug.Print("Error: {0}" + e.Message);
                    throw new XmlFormatException("Error: XML не соответствует схеме. " + e.Message);
                    //break;
                case XmlSeverityType.Warning:
                    Debug.Print("Warning {0}" + e.Message);
                    break;
            }
        }

        /// <summary>
        /// Проверка соответсвия XML текста схеме. В случае ошибки, генерится XmlFormatException или XmlException.
        /// </summary>
        /// <param name="xmlText">Текст в формате XML.</param>
        /// <param name="xmlSchema">Текстовая схема XSD.</param>
        public static void ValidateXML(string xmlText, string xmlSchema)
        {
            XmlDocument tmpDoc = new XmlDocument();
            tmpDoc.LoadXml(xmlText);
            tmpDoc.Schemas.Add("", XmlReader.Create(new StringReader(xmlSchema)));
            ValidationEventHandler eventHandler = XmlValidationEventHandler;
            tmpDoc.Validate(eventHandler);
        }

        /// <summary>
        /// Соответствует ли XML схеме. 
        /// </summary>
        /// <param name="xmlText">Текст в формате XML.</param>
        /// <param name="xmlSchema">Текстовая схема XSD.</param>
        public static bool IsValidXML(string xmlText, string xmlSchema)
        {
            try
            {
                XmlDocument tmpDoc = new XmlDocument();
                tmpDoc.LoadXml(xmlText);
                tmpDoc.Schemas.Add("", XmlReader.Create(new StringReader(xmlSchema)));
                ValidationEventHandler eventHandler = XmlValidationEventHandler;
                tmpDoc.Validate(eventHandler);
            }
            catch (XmlException)
            {
                return false;
            }
            catch (XmlFormatException)
            {
                return false;
            }
            return true;
        }

        #endregion

        public static bool CheckSDConnection(string connString)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connString)
            {
                ApplicationName = "Simple Dictionary",
                ConnectTimeout = 2,
                WorkstationID = "Computer Name"
            };
            try
            {
                using (DataContext db = new DataContext(sb.ConnectionString))
                {
                    db.Connection.Open();
                    var res = (IEnumerable<int>) db.ExecuteQuery(typeof (int), "SELECT COUNT(*) FROM SimpleDictionary");
                    if (res.FirstOrDefault() > 0) return true;
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                MessageBox.Show("Error: " + e.Message, e.Server, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
    }


    /// <summary>
    /// Универсальный сериализатор
    /// </summary>
    /// <typeparam name="T">Сериализуемый тип</typeparam>
    public static class Serializator<T> where T : class
    {
        /// <summary>
        /// Сериализовать объект в строку XML
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>Текст с XML</returns>
        public static string ToXml(T obj)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof (T));
            using (Stream mStream = new MemoryStream())
            {
                xmlFormat.Serialize(mStream, obj);
                mStream.Position = 0;
                StreamReader sr = new StreamReader(mStream);
                var readOut = new char[mStream.Length];
                sr.Read(readOut, 0, readOut.Length);
                int i = Array.IndexOf(readOut, '\0');
                string s = (new string(readOut)).Substring(0, i);
                return s;
            }
        }

        /// <summary>
        /// Десереализация данных из строки Xml
        /// </summary>
        /// <param name="xmlString">Строка в формате Xml</param>
        /// <returns>Десериализованный объект</returns>
        public static T FromXml(string xmlString)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof (T));
            return (T) xmlFormat.Deserialize(XmlReader.Create(new StringReader(xmlString)));
        }


        /// <summary>
        /// Сериализовать объект в массив байтов
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>Сериализованный массив данных</returns>
        public static byte[] Serialize(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        /// <summary>
        /// Сериализовать объект в поток данных
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="data">Поток данных, в который будет произведена сериализация</param>
        public static void Serialize(T obj, Stream data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(data, obj);
        }

        /// <summary>
        /// Десериализация данных из массива байтов
        /// </summary>
        /// <param name="data">Данные</param>
        /// <returns>Десериализованный объект</returns>
        public static T Deserialize(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            //MemoryStream ms = (MemoryStream)(Data);
            return (T) bf.Deserialize(new MemoryStream(data));
        }

        /// <summary>
        /// Десереализация данных из потока
        /// </summary>
        /// <param name="data">Поток сериализованных данных</param>
        /// <returns>Десериализованный объект</returns>
        public static T Deserialize(Stream data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            // MemoryStream ms = (MemoryStream)(data);
            return (T) bf.Deserialize(data);
        }
    }


    public static class RegistryHelper
    {
        private static string FormRegKey(string sSect)
        {
            return sSect;
        }

        public static void SaveSetting(string section, string key, string setting)
        {
            string text1 = FormRegKey(section);
            if (System.Windows.Forms.Application.UserAppDataRegistry == null) return;
            RegistryKey key1 = System.Windows.Forms.Application.UserAppDataRegistry.CreateSubKey(text1);
            if (key1 == null) return;
            try
            {
                key1.SetValue(key, setting);
            }
            finally
            {
                key1.Close();
            }
        }

        public static string GetSetting(string section, string key, string Default = "")
        {
            if (Default == null) Default = "";
            string text2 = FormRegKey(section);
            if (System.Windows.Forms.Application.UserAppDataRegistry != null)
            {
                RegistryKey key1 = System.Windows.Forms.Application.UserAppDataRegistry.OpenSubKey(text2);
                if (key1 != null)
                {
                    object obj1 = key1.GetValue(key, Default);
                    key1.Close();
                    if (obj1 != null)
                    {
                        if (!(obj1 is string))
                        {
                            return null;
                        }
                        return (string) obj1;
                    }
                    return Default;
                }
            }
            return Default;
        }
    }
}