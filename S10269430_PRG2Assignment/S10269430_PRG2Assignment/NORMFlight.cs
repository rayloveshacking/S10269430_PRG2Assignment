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

    public class NORMFlight : Flight
    {

        public NORMFlight() : base() { }

        public NORMFlight(string flightNumber, string origin, string destination, DateTime expectedTime, string status)
        : base(flightNumber, origin, destination, expectedTime, status)
        {
        }

        // Inherits CalculateFees from Flight

        public override string ToString()
        {
            return $"{base.ToString()} (No Special Requests)";
        }
    }
}
