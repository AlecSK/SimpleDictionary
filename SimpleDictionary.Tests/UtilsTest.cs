using System.Diagnostics;
using SimpleDictionary.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SimpleDictionary.Tests
{
    
    
    /// <summary>
    ///Это класс теста для UtilsTest, в котором должны
    ///находиться все модульные тесты UtilsTest
    ///</summary>
    [TestClass()]
    public class UtilsTest
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
        ///Тест для IsValidName
        ///</summary>
        [TestMethod()]
        public void IsValidNameTest()
        {
            string varName = "1rrr";
            Debug.Print("Ответ: {0}", Utils.IsValidName("r_rr"));
        }
    }
}
