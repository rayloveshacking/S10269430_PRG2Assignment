using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10269430_PRG2Assignment
{
    public class CFFTFlight : Flight
    {
        private double requestFee = 150;

        public CFFTFlight() : base() { }

        public CFFTFlight(string flightNumber, string origin, string destination, DateTime expectedTime, string status)
        : base(flightNumber, origin, destination, expectedTime, status)
        {
        }

        public override double CalculateFees()
        {
            return base.CalculateFees() + requestFee;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Special Request: CFFT (Fee: ${requestFee})";
        }
    }
}
