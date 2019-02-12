using ClosedXML.Excel;
using log4net;
using Newtonsoft.Json;
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
        public readonly string hotelRatesFile = "hotelrates.json";
        public readonly string reportFile = "HotelRatesReport.xlsx";
        static ILog logger = LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            return View("Index");
        }

        public FileResult Reporting()
        {
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hotelRatesFile);
            var hotelRates = new HotelRates();
            using (StreamReader reader = new StreamReader(jsonFilePath))
            {
                string json = reader.ReadToEnd();
                hotelRates = JsonConvert.DeserializeObject<HotelRates>(json);
            }

            var reportFilePath = GenerateReport(hotelRates);
            if (reportFilePath == null)
            {
                logger.Error($"Value for file {reportFilePath} was null.");
                RedirectToAction("Error");
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(reportFilePath);
            string fileName = "HotelRatesReport.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public void DeleteFileIfExists(string filePath)
        {
            if(System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                logger.Info($"File {filePath} was deleted successfully!");
            }
        }

        public string GenerateReport(HotelRates hotelRates)
        {
            var reportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFile);
            if (reportFilePath != null)
                DeleteFileIfExists(reportFilePath);
            else
            {
                logger.Error($"Did not find the required file {reportFilePath}");
                return null;
            }

            var excelData = new List<ExcelData>();
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
            
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(dt, "Hotel_Rates");
            wb.SaveAs(reportFilePath);

            logger.Info($"Report file was generated successfully. Path = {reportFilePath}");

            return reportFilePath;
        }

        public string FormatDateTime(string datetime)
        {
            var result = DateTimeOffset.Parse(datetime, CultureInfo.InvariantCulture);
            return result.ToString("dd.MM.yy");
        }

        public ActionResult Error()
        {
            return View("Error");
        }
    }
}