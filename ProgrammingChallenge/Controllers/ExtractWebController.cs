using HtmlAgilityPack;
using Newtonsoft.Json;
using ProgrammingChallenge.Models;
using ProgrammingChallenge.PropertiesClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ProgrammingChallenge.Controllers
{
    public class ExtractWebController : Controller
    {
        public readonly string extractedDataFile = "ExtractedData.json";
        public readonly string htmlFile = "bookingpage.html";
        
        public ActionResult Index()
        {
            try
            {
                string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlFile);
                var jsonData = ExtractData(htmlFilePath);
                var model = new JsonData
                {
                    ExtractedJsonData = jsonData
                };
                return View(model);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public string ExtractData(string htmlFilePath)
        {
            var jsonData = "";
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, extractedDataFile);
            var doc = new HtmlDocument();
            var extractedData = new ExtractedData();

            try
            {
                using (StreamReader reader = new StreamReader(htmlFilePath))
                {
                    doc.Load(reader);

                    // GET HOTEL NAME
                    extractedData.HotelName = FormatString(doc.GetElementbyId("hp_hotel_name").InnerHtml);

                    // GET ADDRESS
                    extractedData.Address = FormatString(doc.GetElementbyId("hp_address_subtitle").InnerHtml);

                    // GET CLASSIFICATION
                    var classification = doc.DocumentNode.Descendants("i").Where(d => d.GetAttributeValue("class", "")
                        .Contains("ratings_stars")).First();
                    foreach (var attribute in classification.Attributes)
                    {
                        if (attribute.Name == "class")
                        {
                            extractedData.Classification = GetValue(attribute.Value);
                        }
                    }

                    // GET REVIEW POINTS
                    var reviewPoints = doc.DocumentNode.Descendants("span").Where(d => d.GetAttributeValue("class", "")
                        .Contains("rating notranslate")).First();
                    var points = FormatString(reviewPoints.InnerText);
                    points = points.Split('/')[0];
                    extractedData.ReviewPoints = Convert.ToDouble(points);

                    // GET NUMBER OF REVIEWS
                    var reviews = doc.DocumentNode.Descendants("strong").Where(d => d.GetAttributeValue("class", "")
                        .Contains("count")).First();
                    var reviewsNumber = FormatString(reviews.InnerText);
                    extractedData.NumberOfReviews = Convert.ToInt32(reviewsNumber);

                    // GET DESCRIPTION
                    var descriptionPart1 = doc.GetElementbyId("summary");
                    foreach (var summaryNode in descriptionPart1.ChildNodes)
                    {
                        if (summaryNode.Name == "p")
                            extractedData.Description += FormatString(summaryNode.InnerText);
                    }

                    var descriptionPart2 = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "")
                        .Contains("hotel_description_wrapper_exp")).First();
                    foreach (var summaryNode in descriptionPart2.ChildNodes)
                    {
                        if (summaryNode.Name == "p")
                            extractedData.Description += FormatString(summaryNode.InnerText);
                    }

                    // GET ROOM CATEGORIES
                    var hotelRooms = doc.GetElementbyId("maxotel_rooms").Descendants("td").Where(d => d.GetAttributeValue("class", "")
                        .Contains("ftd"));
                    var roomCategories = new List<string>();
                    foreach (var childNode in hotelRooms)
                    {
                        roomCategories.Add(FormatString(childNode.InnerText));
                    }
                    extractedData.RoomCategories = roomCategories.ToArray();

                    // GET ALTERNATIVE HOTELS
                    var altHotels = doc.GetElementbyId("altHotelsRow").Descendants("a").Where(d => d.GetAttributeValue("class", "").Contains("althotel_link"));
                    var alternativeHotels = new List<string>();
                    foreach (var hotel in altHotels)
                    {
                        alternativeHotels.Add(FormatString(hotel.InnerText));
                    }
                    extractedData.AlternativeHotels = alternativeHotels.ToArray();

                    DeleteFileIfExists(jsonFilePath);
                    jsonData = JsonConvert.SerializeObject(extractedData, Formatting.Indented);
                    System.IO.File.WriteAllText(jsonFilePath, jsonData);
                }
                return jsonData;
            }
            catch
            {
                throw new Exception();
            }
        }

        public string FormatString(string value)
        {
            value = value.Replace("\n", "");
            value = value.Replace("\r", "");
            return value;
        }

        public int GetValue(string value)
        {
            var lastindex = value.LastIndexOf("_stars_");
            int startIndex = lastindex + 7;
            value = value.Substring(startIndex, 1);
            if (value.Contains(' '))
                value.Replace(" ", string.Empty);
            return Convert.ToInt32(value);
        }

        public void DeleteFileIfExists(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public FileResult DownloadExtractedData()
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, extractedDataFile);
            byte[] fileBytes = System.IO.File.ReadAllBytes(jsonFilePath);
            string fileName = "ExtractedData.json";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}