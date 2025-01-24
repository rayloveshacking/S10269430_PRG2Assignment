using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
//==========================================================
// Student Number : S10269430K
// Student Name : Thar Htet Shein
// Partner Name : Bhaveesh
//==========================================================
*/

namespace S10269430_PRG2Assignment
{
    public abstract class Flight
    {
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime ExpectedTime { get; set; }
        public string Status { get; set; }

        protected Flight() { }

        protected Flight(string flightNumber, string origin, string destination, DateTime expectedTime, string status)
        {
            FlightNumber = flightNumber;
            Origin = origin;
            Destination = destination;
            ExpectedTime = expectedTime;
            Status = status;
        }

        public virtual double CalculateFees()
        {
            double fee = Destination == "SIN" ? 500 : 800;
            fee += 300; // Boarding gate base fee
            return fee;
        }

        public virtual string ToString()
        {
            return $"{FlightNumber} - {Origin} to {Destination}, Time: {ExpectedTime}, Status: {Status}";
        }
    }
}
