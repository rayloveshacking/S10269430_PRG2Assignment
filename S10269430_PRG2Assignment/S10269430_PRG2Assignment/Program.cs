using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using S10269430_PRG2Assignment;

/*
//==========================================================
// Student Number : S10269430K
// Student Name : Thar Htet Shein
// Partner Name : Nadella Bhaveesh Sai
//==========================================================
*/

// Method to load flights from a CSV file and return a Dictionary of Flight objects
Dictionary<string, Flight> LoadFlights(string filePath)
{
    var flights = new Dictionary<string, Flight>();

    if (!File.Exists(filePath))
    {
        Console.WriteLine("File not found.");
        return flights;
    }

    var lines = File.ReadAllLines(filePath).Skip(1); // Skip the header line

    foreach (var line in lines)
    {
        var columns = line.Split(',');

        string flightNumber = columns[0].Trim();
        string origin = columns[1].Trim();
        string destination = columns[2].Trim();
        DateTime expectedTime = DateTime.ParseExact(columns[3].Trim(), "h:mm tt", CultureInfo.InvariantCulture);
        string specialRequestCode = columns.Length > 4 ? columns[4].Trim() : string.Empty;

        Flight flight;

        switch (specialRequestCode)
        {
            case "DDJB":
                flight = new DDJBFlight(flightNumber, origin, destination, expectedTime, "Scheduled");
                break;
            case "CFFT":
                flight = new CFFTFlight(flightNumber, origin, destination, expectedTime, "Scheduled");
                break;
            case "LWTT":
                flight = new LWTTFlight(flightNumber, origin, destination, expectedTime, "Scheduled");
                break;
            default:
                flight = new NORMFlight(flightNumber, origin, destination, expectedTime, "Scheduled");
                break;
        }

        flights.Add(flightNumber, flight);
    }

    return flights;
}

// Example usage
string filePath = "flights.csv";
var flightsDictionary = LoadFlights(filePath);

// Print the loaded flights
foreach (var flight in flightsDictionary.Values)
{
    Console.WriteLine(flight.ToString());
}