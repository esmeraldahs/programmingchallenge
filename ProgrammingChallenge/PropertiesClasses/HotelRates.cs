using System.Collections.Generic;

namespace ProgrammingChallenge.PropertiesClasses
{
    public class HotelRates
    {
        public Hotel hotel { get; set; }
        public List<HotelRate> hotelRates { get; set; }
    }
}