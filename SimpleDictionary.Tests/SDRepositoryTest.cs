using System.Collections.ObjectModel;
using System.Diagnostics;
using SimpleDictionary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SimpleDictionary.Tests
{
    
    
    /// <summary>
    ///Это класс теста для SDRepositoryTest, в котором должны
    ///находиться все модульные тесты SDRepositoryTest
    ///</summary>
    [TestClass()]
    public class SDRepositoryTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Тест для Save
        ///</summary>
        [TestMethod()]
        public void SaveTest()
        {
            SDRepository target = new SDRepository();
            SDictionary sDict = new SDictionary();
            sDict.SD = 99;
            sDict.Name = "Новый словарь";
            sDict.Comment = "Комментарии99";
            bool actual;
            Debug.Print("В репозитарии: {0}", target.GetCount());
            actual = target.Save(sDict);
            Debug.Print("Результат сохранения: {0}", actual);
            Debug.Print("В репозитарии: {0}", target.GetCount());

        }

        /// <summary>
        ///Тест для Remove
        ///</summary>
        [TestMethod()]
        public void RemoveTest()
        {
            SDRepository target = new SDRepository();
            Debug.Print("В репозитарии: {0}", target.GetCount());
            SDictionary sDict = target.GetBySD(99); 
            target.Remove(sDict);
            Debug.Print("В репозитарии: {0}", target.GetCount());
        }

        /// <summary>
        ///Тест для GetSearchResults
        ///</summary>
        [TestMethod()]
        public void GetSearchResultsTest()
        {
            SDRepository target = new SDRepository();
            ObservableCollection<SearchResult> actual;
            actual = target.GetSearchResults();
            Debug.Print("Всего {0}",actual.Count);
        }
    }
}
