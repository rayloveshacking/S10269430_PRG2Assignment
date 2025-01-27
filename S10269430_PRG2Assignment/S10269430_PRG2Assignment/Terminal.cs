using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
//==========================================================
// Student Number : S10269430K
// Student Name : Thar Htet Shein
// Partner Name : Nadella Bhaveesh Sai
//==========================================================
*/

namespace S10269430_PRG2Assignment
{
    public class Terminal
    {
        public string TerminalName { get; set; }
        public Dictionary<string, Airline> Airlines { get; set; }
        public Dictionary<string, Flight> Flights { get; set; }
        public Dictionary<string, BoardingGate> BoardingGates { get; set; }
        public Dictionary<string, double> GateFees { get; set; }

        public Terminal()
        {
            Airlines = new Dictionary<string, Airline>();
            Flights = new Dictionary<string, Flight>();
            BoardingGates = new Dictionary<string, BoardingGate>();
            GateFees = new Dictionary<string, double>();
        }

        public Terminal(string terminalName) : this()
        {
            TerminalName = terminalName;
        }

        public bool AddAirline(Airline airline)
        {
            if (airline == null || Airlines.ContainsKey(airline.Code))
                return false;
            Airlines.Add(airline.Code, airline);
            foreach (var flight in airline.Flights.Values)
            {
                if (!Flights.ContainsKey(flight.FlightNumber))
                    Flights.Add(flight.FlightNumber, flight);
            }
            return true;
        }

        public bool AddBoardingGate(BoardingGate gate)
        {
            if (gate == null || BoardingGates.ContainsKey(gate.GateName))
                return false;
            BoardingGates.Add(gate.GateName, gate);
            return true;
        }

        public Airline GetAirlineFromFlight(Flight flight)
        {
            foreach (var airline in Airlines.Values)
            {
                if (airline.Flights.ContainsKey(flight.FlightNumber))
                    return airline;
            }
            return null;
        }

        public void PrintAirlineFees()
        {
            foreach (var airline in Airlines.Values)
            {
                Console.WriteLine($"{airline.Name}: ${airline.CalculateFees():0.00}");
            }
        }

        public override string ToString()
        {
            return $"{TerminalName} - Airlines: {Airlines.Count}, Flights: {Flights.Count}, Gates: {BoardingGates.Count}";
        }
    }
}
