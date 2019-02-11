using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProgrammingChallenge.PropertiesClasses
{
    public class ExcelData
    {
        public string ArrivalDate { get; set; }
        public string DepartureDate { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string RateName { get; set; }
        public int Adults { get; set; }
        public int BreakfastIncluded { get; set; }
    }
}