using System.Collections.Generic;

namespace ProgrammingChallenge.PropertiesClasses
{
    public class HotelRate
    {
        public int adults { get; set; }
        public double los { get; set; }
        public Price Price { get; set; }
        public string rateDescription { get; set; }
        public string rateID { get; set; }
        public string rateName { get; set; }
        public List<RateTags> rateTags { get; set; }
        public string targetDay { get; set; }
    }
}