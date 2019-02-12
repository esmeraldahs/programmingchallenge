﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgrammingChallenge.Controllers;
using ProgrammingChallenge.Helpers;
using ProgrammingChallenge.PropertiesClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace ProgrammingChallengeTests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        public HomeController homeController;
        public TestPathProvider pathProvider;

        [TestInitialize]
        public void Initialize()
        {
            pathProvider = new TestPathProvider();
            homeController = new HomeController();
        }

        [TestMethod]
        public void IndexView()
        {
            var view = homeController.Index() as ViewResult;
            Assert.AreEqual("Index", view.ViewName);
        }

        [TestMethod]
        public void CheckIfFileIsDeleted()
        {
            var fileToDelete = @"testFile.txt";
            var directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var path = Path.Combine(directory, fileToDelete);
            File.WriteAllText(path, "");
            homeController.DeleteFileIfExists(path);
            
            Assert.IsFalse(File.Exists(path));
        }

        [TestMethod]
        public void CheckIfReportIsGenerated()
        {
            var rateTag = new List<RateTags>();
            rateTag.Add(new RateTags
            {
                name = "rate tag name",
                shape = true
            });

            var hotelRate = new List<HotelRate>();
            hotelRate.Add(new HotelRate {
                adults = 4,
                los = 2,
                Price = new Price
                {
                    currency = "EUR",
                    numericFloat = 187F,
                    numericInteger = 178
                },
                rateDescription = "Very long desc",
                rateID = "rateId",
                rateName = "rate name",
                rateTags = rateTag,
                targetDay = DateTime.Now.ToShortTimeString()
            });

            var hotelRates = new HotelRates
            {
                hotel = new Hotel
                {
                    hotelID = 1,
                    classification = 6,
                    name = "Test Hotel",
                    reviewscore = 7.8F
                },
                hotelRates = hotelRate
            };

            var filepath = homeController.GenerateReport(hotelRates);
            Assert.IsTrue(File.Exists(filepath));
        }
    }
}