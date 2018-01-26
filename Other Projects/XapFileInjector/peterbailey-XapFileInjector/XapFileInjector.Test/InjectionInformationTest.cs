using System;
using System.IO;
using NUnit.Framework;

namespace XapFileInjector.Test
{
    [TestFixture]
    public class InjectionInformationTest
    {
        private FileInfo output_xap;
        private FileInfo input_xap;
        private FileInfo injected_file;
        private string injected_file_name;

        [SetUp]
        public void Setup()
        {
            input_xap = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), @"TestFiles\Example.xap"));
            output_xap = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),
                                                        @"TestFiles\Output.xap"));
            injected_file = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),
                                                        @"TestFiles\New.ClientConfig"));
            injected_file_name = "something.xml";
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(output_xap.FullName))
                File.Delete(output_xap.FullName);
        }

        #region InputXapFile

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Input xap file must be specified.")]
        public void when_creating_with_no_input_xap_file__should_cause_argument_exception()
        {
            new InjectionInformation(null, output_xap.FullName, injected_file.FullName, injected_file_name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Input xap file must be specified.")]
        public void when_creating_with_no_value_for_input_xap_file__should_cause_argument_exception()
        {
            new InjectionInformation("", output_xap.FullName, injected_file.FullName, injected_file_name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Input xap file does not exist.")]
        public void when_creating_with_input_xap_file_that_does_not_exist__should_cause_argument_exception()
        {
            new InjectionInformation(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), @"TestFiles\FakeExample.xap")).FullName, output_xap.FullName, injected_file.FullName,
                                      injected_file_name);
        }

        [Test]
        public void when_creating_with_a_valid_input_xap_file__should_set_property_to_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, injected_file_name);
            Assert.AreEqual(input_xap.FullName, subject.InputXapFile);
        }

        #endregion

        #region OutputXapFile

        [Test]
        public void when_creating_with_no_output_xap_file__should_set_output_property_to_input_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, null, injected_file.FullName, injected_file_name);
            Assert.AreEqual(input_xap.FullName, subject.OutputXapFile);
        }

        [Test]
        public void when_creating_with_empty_output_xap_file__should_set_output_property_to_input_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, "", injected_file.FullName, injected_file_name);
            Assert.AreEqual(input_xap.FullName, subject.OutputXapFile);
        }

        [Test]
        public void when_creating_with_a_valid_output_xap_file__should_set_property_to_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, injected_file_name);
            Assert.AreEqual(output_xap.FullName, subject.OutputXapFile);
        }

        #endregion

        #region InjectedFile

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Injected file must be specified.")]
        public void when_creating_with_no_injected_file__should_cause_argument_exception()
        {
            new InjectionInformation(input_xap.FullName, output_xap.FullName, null, injected_file_name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Injected file must be specified.")]
        public void when_creating_with_no_value_for_injected_file__should_cause_argument_exception()
        {
            new InjectionInformation(input_xap.FullName, output_xap.FullName, "", injected_file_name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Injected file does not exist.")]
        public void when_creating_with_injected_file_that_does_not_exist__should_cause_argument_exception()
        {
            new InjectionInformation(input_xap.FullName, output_xap.FullName, new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), @"TestFiles\Fake.ClientConfig")).FullName, injected_file_name);
        }

        [Test]
        public void when_creating_with_a_valid_injected_file__should_set_property_to_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, injected_file_name);
            Assert.AreEqual(injected_file.FullName, subject.InjectedFile);
        }

        #endregion

        #region InjectedFileName

        [Test]
        public void when_creating_with_no_injected_file_name__should_set_name_property_to_name_of_injected_file()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, null);
            Assert.AreEqual(injected_file.Name, subject.InjectedFileName);
        }

        [Test]
        public void when_creating_with_empty_injected_file_name__should_set_name_property_to_name_of_injected_file()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, "");
            Assert.AreEqual(injected_file.Name, subject.InjectedFileName);
        }

        [Test]
        public void when_creating_with_a_valid_injected_file_name__should_set_property_to_value()
        {
            var subject = new InjectionInformation(input_xap.FullName, output_xap.FullName, injected_file.FullName, injected_file_name);
            Assert.AreEqual(injected_file_name, subject.InjectedFileName);
        }

        #endregion
    }
}
