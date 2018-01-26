using InstallMSI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace InstallMSI_TestProject
{
    
    
    /// <summary>
    ///This is a test class for ProgramTest and is intended
    ///to contain all ProgramTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ProgramTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for determineEnvironment
        ///</summary>
        [TestMethod()]
        [DeploymentItem("I.exe")]
        public void determineEnvironmentTest()
        {
            string[] args = "some.msi".Split(',');
            string machineName = "kjwhfwjkfhwjkauto1o;qkdopwkd";
            string expected = "AUTO1";
            string actual;
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "kjwhfwjkfhwjkqa1o;qkdopwkd";
            expected = "QA1";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "kjwhfwjkfhwjkqa2o;qkdopwkd";
            expected = "QA2";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "kjwhfwjkfhwjkmodel1o;qkdopwkd";
            expected = "MODEL1";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "kjwhfwjkfhwjkmodel21o;qkdopwkd";
            expected = "MODEL2";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "kjwhfwjkfhwjkprod;qkdopwkd";
            expected = "PROD";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            machineName = "qa2kjwhfwjkfhwjkprod;qkdopwkd";
            expected = "QA2";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            args = "some.msi,dev2".Split(',');
            machineName = "I'mAPRODBox";
            expected = "DEV2";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);

            args = "some.msi".Split(',');
            machineName = "NoMatch";
            expected = "?";
            actual = Program_Accessor.determineEnvironment(args, machineName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for validateEnvironment
        ///</summary>
        [TestMethod()]
        [DeploymentItem("I.exe")]
        public void validateEnvironmentTest()
        {
            string env = "DEV";
            Program_Accessor.validateEnvironment(env);

            env = "QA";
            Program_Accessor.validateEnvironment(env);

            env = "MODEL";
            Program_Accessor.validateEnvironment(env);

            env = "PROD";
            Program_Accessor.validateEnvironment(env);

            Assert.Inconclusive("If the result is inconclusive it passed!");
        }
    }
}
