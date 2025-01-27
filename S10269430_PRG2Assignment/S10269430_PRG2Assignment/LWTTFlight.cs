﻿using System;
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
    public class LWTTFlight : Flight
    {
        private double requestFee = 500;

        public LWTTFlight() : base() { }

        public LWTTFlight(string flightNumber, string origin, string destination, DateTime expectedTime, string status)
        : base(flightNumber, origin, destination, expectedTime, status)
        {
        }

        public override double CalculateFees()
        {
            return base.CalculateFees() + requestFee;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Special Request: LWTT (Fee: ${requestFee})";
        }
    }
}
