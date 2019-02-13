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

        public ActionResult Reporting()
        {
            try
            {
                // GET THE "hotelrates.json" FILE
                var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hotelRatesFile);
                var hotelRates = new HotelRates();
                using (StreamReader reader = new StreamReader(jsonFilePath))
                {
                    string json = reader.ReadToEnd();
                    hotelRates = JsonConvert.DeserializeObject<HotelRates>(json);
                }
                
                // GENERATE THE REPORT FROM THE DATA
                var reportFilePath = GenerateReport(hotelRates);
                if (reportFilePath == null)
                {
                    logger.Error($"Value for file {reportFilePath} was null.");
                    return RedirectToAction("Error");
                }

                // DOWLOAD THE FILE AT USER'S LOCAL MACHINE
                return DownloadExtractedData(reportFilePath);
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"FileNotFoundException. Error: {ex.Message}. Stack trace: {ex.StackTrace}");
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                logger.Error($"Caught exception. Error: {ex.Message}. Stack trace: {ex.StackTrace}");
                return RedirectToAction("Error");
            }
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
            DeleteFileIfExists(reportFilePath);

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
            wb.SaveAs(reportFilePath); // SAVE THE FILE LOCALLY

            logger.Info($"Report file was generated successfully. Path = {reportFilePath}");
            return reportFilePath;
        }

        public string FormatDateTime(string datetime)
        {
            var result = DateTimeOffset.Parse(datetime, CultureInfo.InvariantCulture);
            return result.ToString("dd.MM.yy");
        }

        public FileResult DownloadExtractedData(string reportFilePath)
        {
            // GET THE FILE PREVIOUSLY GENERATED AND SAVED LOCALLY
            byte[] fileBytes = System.IO.File.ReadAllBytes(reportFilePath);
            string fileName = "HotelRatesReport.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult Error()
        {
            return View("Error");
        }
    }
}