using ClosedXML.Excel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ProgrammingChallenge.Models;
using ProgrammingChallenge.PropertiesClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ProgrammingChallenge.Controllers
{
    public class HomeController : Controller
    {
        public readonly string htmlFile = @"~/bookingpage.html";
        public readonly string extractedDataFile = @"~/ExtractedData.json";
        public readonly string hotelRatesFile = @"~/hotelrates.json";
        public readonly string reportFile = @"~/HotelRatesReport.xlsx";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Extract()
        {
            string htmlFilePath = Server.MapPath(htmlFile);
            string jsonFilePath = Server.MapPath(extractedDataFile);

            var doc = new HtmlDocument();
            var extractedData = new ExtractedData();
            var model = new JsonData();

            using (StreamReader reader = new StreamReader(htmlFilePath))
            {
                doc.Load(reader);

                //GET HOTEL NAME
                extractedData.HotelName = doc.GetElementbyId("hp_hotel_name").InnerHtml.Replace("\n", "");

                // GET ADDRESS
                extractedData.Address = doc.GetElementbyId("hp_address_subtitle").InnerHtml.Replace("\n", "");

                // GET CLASSIFICATION
                var classification = doc.DocumentNode.Descendants("i").Where(d => d.GetAttributeValue("class", "").Contains("ratings_stars")).First();
                foreach(var attribute in classification.Attributes)
                {
                    if(attribute.Name == "class")
                    {
                        extractedData.Classification = GetValue(attribute.Value);
                    }
                }

                // GET REVIEW POINTS
                var reviewPoints = doc.DocumentNode.Descendants("span").Where(d => d.GetAttributeValue("class", "").Contains("rating notranslate")).First();
                var points = reviewPoints.InnerText.Replace("\n", "");
                points = points.Split('/')[0];
                extractedData.ReviewPoints = Convert.ToDouble(points);

                // GET NUMBER OF REVIEWS
                var reviewsNumber = doc.DocumentNode.Descendants("strong").Where(d => d.GetAttributeValue("class", "").Contains("count")).First();
                extractedData.NumberOfReviews = Convert.ToInt32(reviewsNumber.InnerText.Replace("\n", ""));

                // GET DESCRIPTION
                var descriptionPart1 = doc.GetElementbyId("summary");
                foreach (var summaryNode in descriptionPart1.ChildNodes)
                {
                    if (summaryNode.Name == "p")
                        extractedData.Description += summaryNode.InnerText.Replace("\n", "");
                }

                var descriptionPart2 = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("hotel_description_wrapper_exp")).First();
                foreach (var summaryNode in descriptionPart2.ChildNodes)
                {
                    if (summaryNode.Name == "p")
                        extractedData.Description += summaryNode.InnerText.Replace("\n", "");
                }

                // GET ROOM CATEGORIES
                var hotelRooms = doc.GetElementbyId("maxotel_rooms").Descendants("td").Where(d => d.GetAttributeValue("class", "").Contains("ftd"));
                var roomCategories = new List<string>();
                foreach (var childNode in hotelRooms)
                {
                    roomCategories.Add(childNode.InnerText.Replace("\n", ""));
                }
                extractedData.RoomCategories = roomCategories.ToArray();

                // GET ALTERNATIVE HOTELS
                var altHotels = doc.GetElementbyId("altHotelsRow").Descendants("a").Where(d => d.GetAttributeValue("class", "").Contains("althotel_link"));
                var alternativeHotels = new List<string>();
                foreach (var hotel in altHotels)
                {
                    alternativeHotels.Add(hotel.InnerText.Replace("\n", ""));
                }
                extractedData.AlternativeHotels = alternativeHotels.ToArray();

                DeleteFileIfExists(jsonFilePath);
                var jsonData = JsonConvert.SerializeObject(extractedData, Formatting.Indented);
                System.IO.File.WriteAllText(jsonFilePath, jsonData);

                model.ExtractedJsonData = jsonData;
            }
            return View(model);
        }

        public int GetValue(string value)
        {
            int startIndex = value.LastIndexOf("_stars_") + 7;
            value = value.Substring(startIndex, 2);
            if (value.Contains(' '))
                value.Replace(" ", string.Empty);
            return Convert.ToInt32(value);
        }

        public FileResult Reporting()
        {
            var jsonFilePath = Server.MapPath(hotelRatesFile);
            var hotelRates = new HotelRates();
            using (StreamReader reader = new StreamReader(jsonFilePath))
            {
                string json = reader.ReadToEnd();
                hotelRates = JsonConvert.DeserializeObject<HotelRates>(json);
            }

            var reportFilePath = GenerateReport(hotelRates);
            byte[] fileBytes = System.IO.File.ReadAllBytes(reportFilePath);
            string fileName = "HotelRatesReport.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public void DeleteFileIfExists(string filePath)
        {
            if(System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public string GenerateReport(HotelRates hotelRates)
        {
            var excelData = new List<ExcelData>();
            var reportFilePath = Server.MapPath(reportFile);

            foreach (var hotelRate in hotelRates.hotelRates)
            {
                var departureDay = Convert.ToDateTime(hotelRate.targetDay).AddDays(hotelRate.los).ToString();
                var hotelRateData = new ExcelData
                {
                    ArrivalDate = FormatDateTime(hotelRate.targetDay),
                    DepartureDate = FormatDateTime(departureDay),
                    Price = hotelRate.Price.numericFloat,
                    Currency = hotelRate.Price.currency,
                    RateName = hotelRate.rateName,
                    Adults = hotelRate.adults,
                    BreakfastIncluded = hotelRate.rateTags.First().shape ? 1 : 0
                };
                excelData.Add(hotelRateData);
            }

            var dt = new DataTable();
            dt.Columns.Add("ARRIVAL_DATE");
            dt.Columns.Add("DEPARTURE_DATE");
            dt.Columns.Add("PRICE");
            dt.Columns.Add("CURRENCY");
            dt.Columns.Add("RATENAME");
            dt.Columns.Add("ADULTS");
            dt.Columns.Add("BREAKFAST_INCLUDED");

            foreach (var item in excelData)
            {
                dt.Rows.Add(item.ArrivalDate, item.DepartureDate, item.Price, item.Currency,
                    item.RateName, item.Adults, item.BreakfastIncluded);
            }

            DeleteFileIfExists(reportFilePath);
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(dt, "Hotel_Rates");
            wb.SaveAs(reportFilePath);

            return reportFilePath;
        }

        public string FormatDateTime(string datetime)
        {
            DateTimeOffset result = DateTimeOffset.Parse(datetime, CultureInfo.InvariantCulture);
            return result.ToString("dd.MM.yy");
        }

        public FileResult DownloadExtractedData()
        {
            string jsonFilePath = Server.MapPath(extractedDataFile);
            byte[] fileBytes = System.IO.File.ReadAllBytes(jsonFilePath);
            string fileName = "ExtractedData.json";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}