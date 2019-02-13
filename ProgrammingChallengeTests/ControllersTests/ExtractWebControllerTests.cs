using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgrammingChallenge.Controllers;
using ProgrammingChallenge.Models;
using System;
using System.Web.Mvc;

namespace ProgrammingChallengeTests.ControllersTests
{
    [TestClass]
    public class ExtractWebControllerTests
    {
        public ExtractWebController extractWebController;

        [TestInitialize]
        public void Initialize()
        {
            extractWebController = new ExtractWebController();
        }

        [TestMethod]
        public void ExtractDataWithNullFileNameShouldReturnNull()
        {
            string fileName = null;
            var response = extractWebController.ExtractData(fileName);
            Assert.IsNull(response);
        }

        [TestMethod]
        public void IndexReturnRightViewAndRightModel()
        {
            var result = extractWebController.Index();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var returnedView = result as ViewResult;
            Assert.IsInstanceOfType(returnedView.Model, typeof(JsonData));
        }

        [TestMethod]
        public void StringShouldBeFormattedCorrectly()
        {
            var value = "\n\r this is a \n test string\r";
            var formattedString = extractWebController.FormatString(value);

            Assert.AreEqual(" this is a  test string", formattedString);
        }

        [TestMethod]
        public void MethodShouldReturnNeededValueFromString()
        {
            var value = "this is a test string_stars_4";
            var formattedString = extractWebController.GetValue(value);

            Assert.AreEqual(4, formattedString);
        }

        [TestMethod]
        public void DownloadMethodShouldReturnFileResult()
        {
            var result = extractWebController.DownloadExtractedData();
            Assert.IsInstanceOfType(result, typeof(FileResult));
        }
    }
}
