using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10269430_PRG2Assignment
{
    public class Airline
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public Dictionary<string, Flight> Flights { get; set; }

        public Airline()
        {
            Flights = new Dictionary<string, Flight>();
        }

        public Airline(string name, string code) : this()
        {
            Name = name;
            Code = code;
        }

        public bool AddFlight(Flight flight)
        {
            if (flight == null || Flights.ContainsKey(flight.FlightNumber))
                return false;
            Flights.Add(flight.FlightNumber, flight);
            return true;
        }

        public double CalculateFees()
        {
            double totalBeforeDiscounts = Flights.Values.Sum(f => f.CalculateFees());
            int discount2Count = 0;
            int discount3Count = 0;
            int discount4Count = 0;

            foreach (var flight in Flights.Values)
            {
                DateTime time = flight.ExpectedTime;
                if (time.Hour < 11 || time.Hour >= 21)
                    discount2Count++;

                if (new[] { "DXB", "BKK", "NRT" }.Contains(flight.Origin))
                    discount3Count++;

                if (flight is NORMFlight)
                    discount4Count++;
            }

            int discount1 = (Flights.Count / 3) * 350;
            int discount2 = discount2Count * 110;
            int discount3 = discount3Count * 25;
            int discount4 = discount4Count * 50;
            double totalDiscounts = discount1 + discount2 + discount3 + discount4;

            double total = totalBeforeDiscounts;

            if (Flights.Count > 5)
                total *= 0.97;

            total -= totalDiscounts;

            return total;
        }

        public bool RemoveFlight(Flight flight)
        {
            if (flight == null || !Flights.ContainsKey(flight.FlightNumber))
                return false;
            Flights.Remove(flight.FlightNumber);
            return true;
        }

        public override string ToString()
        {
            return $"{Name} ({Code}) - Flights: {Flights.Count}";
        }
    }
}
