// ***********************************************************************
// Assembly         : SimpleDictionary
// Author           : Круглов Олег
// Created          : 12-27-2012
//
// Last Modified By : Круглов Олег
// Last Modified On : 12-27-2012
// ***********************************************************************
// <summary> 
// Генерация классов C# для использования параметров и констант в программном коде проекта. 
// </summary>
// ***********************************************************************

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using SimpleDictionary.Linq;
using SimpleDictionary.Models;


namespace SimpleDictionary.Utility
{
    /// <summary>
    /// Class CodeGenerator
    /// </summary>
    public static class CodeGenerator
    {
        //public static string GetFullFileName()
        //{
        //    string dirName = Utils.ReadStringParameter("ConstantsSavePath");
        //    dirName = dirName ?? AppDomain.CurrentDomain.BaseDirectory;
        //    string fileName = Utils.ReadStringParameter("ConstantsFileName");
        //    return System.IO.Path.Combine(dirName, fileName);
        //}


        private static string GetSuffix(string paramCode)
        {
            string suffix = "";
            switch (paramCode)
            {
                case "IntValue":
                    suffix = "Int";
                    break;
                case "FloatValue":
                    suffix = "Real";
                    break;
                case "StringValue":
                    suffix = "Str";
                    break;
                case "DateValue":
                    suffix = "D";
                    break;
                case "MultiValue":
                    suffix = "Multi";
                    break;
                case "MemoValue":
                    suffix = "Memo";
                    break;
            }
            return suffix;
        }


        private static string GetConstType(string paramCode)
        {
            string paramType = "";
            switch (paramCode)
            {
                case "IntValue":
                    paramType = "const int";
                    break;
                case "FloatValue":
                    paramType = "const double";
                    break;
                case "StringValue":
                    paramType = "const string";
                    break;
                case "DateValue":
                    paramType = "static readonly DateTime";
                    break;
                case "MultiValue":
                    paramType = "const string";
                    break;
                case "MemoValue":
                    paramType = "const string";
                    break;
            }
            return paramType;
        }


        private static string GetConstValue(string paramCode, Linq.SimpleDictionary sdRow)
        {
            string paramValue = "";
            switch (paramCode)
            {
                case "IntValue":
                    paramValue = sdRow.IntValue.ToString();
                    break;
                case "FloatValue":
                    paramValue = Convert.ToString(sdRow.FloatValue, new CultureInfo("en-US"));
                    break;
                case "StringValue":
                    if (sdRow.StringValue == null) return null;
                    paramValue = "@\"" + sdRow.StringValue + "\"";
                    break;
                case "DateValue":
                    if (sdRow.DateValue == null) return null;
                    paramValue = string.Format("new DateTime({0}, {1}, {2}, {3}, {4}, {5})",
                        sdRow.DateValue.Value.Year, sdRow.DateValue.Value.Month, sdRow.DateValue.Value.Day,
                        sdRow.DateValue.Value.Hour, sdRow.DateValue.Value.Minute, sdRow.DateValue.Value.Second);
                    //paramValue = "new DateTime(" + Convert.ToString(sdRow.DateValue, new CultureInfo("en-US")) + ")";
                    break;
                case "MultiValue":
                    if (sdRow.MultiValue == null) return null;
                    paramValue = "@\"" + sdRow.MultiValue + "\"";
                    break;
                case "MemoValue":
                    if (sdRow.MemoValue == null) return null;
                    paramValue = "@\"" + sdRow.MemoValue + "\"";
                    break;
            }
            return paramValue;
        }


        private static string Tabs(int indent)
        {
            string tabs = "";
            for (int i = 0; i < indent; i++)
            {
                tabs += "\t";
            }
            return tabs;
        }


        private static void WriteSummary(System.IO.StreamWriter sw, string text, int indent)
        {
            if (string.IsNullOrEmpty(text)) return;

            sw.WriteLine("{0}/// <summary>", Tabs(indent));
            sw.WriteLine("{0}/// {1}", Tabs(indent), text);
            sw.WriteLine("{0}/// </summary>", Tabs(indent));
        }


        private static void WriteRemarks(System.IO.StreamWriter sw, string multitext, int indent)
        {
            if (string.IsNullOrEmpty(multitext)) return;
            var textLines = multitext.Split(Const.CrLf, StringSplitOptions.RemoveEmptyEntries);

            sw.WriteLine("{0}/// <remarks>", Tabs(indent));
            foreach (string textLine in textLines)
            {
                sw.WriteLine("{0}/// {1}", Tabs(indent), textLine);
            }
            sw.WriteLine("{0}/// </remarks>", Tabs(indent));
        }


        private static void WriteDictionariesList(StreamWriter sw)
        {
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                sw.WriteLine();
                WriteSummary(sw, "Список всех словарей", 1);
                sw.WriteLine("{0}public struct Dictionaries", Tabs(1));
                sw.WriteLine("{0}{{", Tabs(1));

                //Для всех словарей, отмеченных для генерации
                var dicts =
                    dc.SimpleDictionary.Where(e => e.RecType == 'D' && e.SD > 11 && e.MemoValue != null)
                        .OrderBy(r => r.CurrentN);
                foreach (var dict in dicts)
                {
                    ObservableCollection<SDOption> options;
                    //Загружаем параметры генерации для словаря
                    try
                    {
                        options = Serializator<ObservableCollection<SDOption>>.FromXml(dict.MemoValue);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (options.Any(r => r.GenerateStruct || r.GenerateEnum))
                    {
                        //Записываем в список словарей
                        sw.WriteLine();
                        if (!string.IsNullOrEmpty(dict.Description))
                            sw.WriteLine("{0}///<summary> {1} </summary>", Tabs(2), dict.Description);
                        WriteRemarks(sw, dict.Comment, 2);
                        sw.Write("{0}public const int {1} = {2};", Tabs(2), dict.Name, dict.SD);
                    }
                }
                sw.WriteLine();
                sw.WriteLine("{0}}}", Tabs(1));
                sw.WriteLine();
            }
        }


        private static void WriteConstants(StreamWriter sw)
        {
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                //Для всех словарей, отмеченных для генерации
                var dicts = dc.SimpleDictionary.Where(e => e.RecType == 'D' && e.SD > 11 && e.MemoValue != null);
                foreach (var dict in dicts)
                {
                    ObservableCollection<SDOption> options;
                    //Загружаем параметры генерации для словаря
                    try
                    {
                        options = Serializator<ObservableCollection<SDOption>>.FromXml(dict.MemoValue);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    var selectedOptions = options.Where(o => o.GenerateStruct);
                    foreach (SDOption option in selectedOptions)
                    {
                        bool withComment = option.AddComment;
                        int parentSD = dict.SD;

                        //Генерим перечисление в виде класса (только для целочисленных параметров)
                        if (option.GenerateEnum && option.Name == "IntValue")
                        {
                            sw.WriteLine();
                            WriteSummary(sw, dict.Description, 1);
                            WriteRemarks(sw, dict.Comment, 1);
                            sw.WriteLine("{0}public enum {1}Enum", Tabs(1), dict.Name);
                            sw.WriteLine("{0}{{", Tabs(1));

                            bool firstLine = true;
                            var dictValues = dc.SimpleDictionary.Where(e => e.ParentSD == parentSD && e.IntValue != null);
                            foreach (var v in dictValues)
                            {
                                if (!firstLine) sw.WriteLine(",");

                                sw.WriteLine();
                                if (withComment && !string.IsNullOrEmpty(v.Description))
                                    sw.WriteLine("{0}///<summary> {1} </summary>", Tabs(2), v.Description);
                                if (withComment)
                                    WriteRemarks(sw, v.Comment, 2);
                                sw.Write("{0} {1} = {2}", Tabs(2), v.Name, v.IntValue);
                                firstLine = false;
                            }
                            sw.WriteLine();
                            sw.WriteLine("{0}}}", Tabs(1));
                            sw.WriteLine();
                        }
                        //Генерим константы в виде структуры
                        if (option.GenerateStruct)
                        {
                            sw.WriteLine();
                            WriteSummary(sw, dict.Description, 1);
                            WriteRemarks(sw, dict.Comment, 1);
                            sw.WriteLine("{0}public struct {1}{2}", Tabs(1), dict.Name, GetSuffix(option.Name));
                            sw.WriteLine("{0}{{", Tabs(1));

                            var dictValues = dc.SimpleDictionary.Where(e => e.ParentSD == parentSD);
                            foreach (var v in dictValues)
                            {
                                string constType = GetConstType(option.Name);
                                string constValue = GetConstValue(option.Name, v);
                                if (string.IsNullOrEmpty(constValue)) continue;

                                sw.WriteLine();
                                if (withComment && !string.IsNullOrEmpty(v.Description))
                                    sw.WriteLine("{0}///<summary> {1} </summary>", Tabs(2), v.Description);
                                if (withComment)
                                    WriteRemarks(sw, v.Comment, 2);
                                sw.WriteLine("{0}public {3} {1} = {2};", Tabs(2), v.Name, constValue, constType);
                            }
                            sw.WriteLine("{0}}}", Tabs(1));
                            sw.WriteLine();
                        }
                    }
                }
            }
        }


        public static string WriteConstantsToFile()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "Autogenerated.txt");

            StreamWriter sw = new System.IO.StreamWriter(tempFile, false);
            using (var dc = new SDLinqDataContext(App.ConnectionString))
            {
                Linq.SimpleDictionary row = dc.SimpleDictionary.FirstOrDefault(r => r.Name == "Header");
                if (row != null)
                {
                    string fileHeader = row.MemoValue;
                    row = dc.SimpleDictionary.FirstOrDefault(r => r.Name == "Version");
                    if (row != null)
                        sw.WriteLine(fileHeader, Environment.UserName, row.DateValue, row.StringValue + "." + row.IntValue);
                }

                row = dc.SimpleDictionary.FirstOrDefault(r => r.Name == "DirectivesUsing");
                if (row != null) sw.WriteLine(row.MemoValue);
                sw.WriteLine();
                sw.WriteLine("namespace {0}", Utils.ReadStringParameter("Namespace"));
                sw.WriteLine("{");
                sw.WriteLine();
                sw.WriteLine("{0}#region Autogenerated. Do not change this code!", Tabs(1));
                sw.WriteLine();
                sw.WriteLine(Utils.ReadMultiTextParameter("Repository"));
            }

            WriteDictionariesList(sw);
            WriteConstants(sw);

            sw.WriteLine("{0}#endregion Autogenerated. Do not change this code!", Tabs(1));
            sw.WriteLine();
            sw.WriteLine("}");

            sw.Flush();
            sw.Close();
            //sw.Dispose();

            string dirName = Utils.ReadStringParameter("ConstantsSavePath");
            dirName = dirName ?? AppDomain.CurrentDomain.BaseDirectory;
            string targetFile = Path.Combine(dirName, Utils.ReadStringParameter("ConstantsFileName"));

            if (File.Exists(targetFile))
                if (MessageBox.Show("Перезаписать существующий файл " + targetFile + "?", "Файл уже существует!",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                    return tempFile;

            File.Copy(tempFile, targetFile, true);
            return targetFile;
        }
    }
}