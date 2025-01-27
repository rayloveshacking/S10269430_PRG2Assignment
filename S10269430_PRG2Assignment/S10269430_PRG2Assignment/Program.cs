using System;
using System.Collections.Generic;
using System.IO;

namespace S10269430_PRG2Assignment
{
    class Program
    {
        // Dictionary to store airlines with their airline code as the key
        static Dictionary<string, Airline> airlines = new Dictionary<string, Airline>();

        static void Main(string[] args)
        {
            // Load data from CSV files
            LoadAirlines();
            LoadFlights();

            // Display airline flights
            DisplayAirlineFlights();
        }

        // Method to load airline data from the "airline.csv" file
        static void LoadAirlines()
        {
            try
            {
                using (var reader = new StreamReader("airline.csv"))
                {
                    // Skip the header line
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        // Extract airline name and code
                        string airlineName = values[0].Trim();
                        string airlineCode = values[1].Trim();

                        // Create an Airline object and add it to the dictionary
                        var airline = new Airline(airlineName, airlineCode);
                        airlines[airlineCode] = airline;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading airlines: {ex.Message}");
            }
        }

        // Method to load flight data from the "flights.csv" file
        static void LoadFlights()
        {
            try
            {
                using (var reader = new StreamReader("flights.csv"))
                {
                    // Skip the header line
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        // Extract flight details
                        string flightNumber = values[0].Trim();
                        string origin = values[1].Trim();
                        string destination = values[2].Trim();
                        string expectedTimeString = values[3].Trim();
                        string specialRequestCode = values.Length > 4 ? values[4].Trim() : string.Empty;

                        // Parse expected departure/arrival time
                        DateTime expectedTime;
                        if (!DateTime.TryParse(expectedTimeString, out expectedTime))
                        {
                            Console.WriteLine($"Invalid date format for flight {flightNumber}. Skipping...");
                            continue;
                        }

                        // Extract airline code from the flight number (first two characters)
                        string airlineCode = flightNumber.Substring(0, 2);

                        // Check if the airline code exists in the dictionary
                        if (airlines.ContainsKey(airlineCode))
                        {
                            // Create a Flight object and add it to the airline
                            var flight = new Flight(flightNumber, origin, destination, expectedTime, specialRequestCode);

                            airlines[airlineCode].AddFlight(flight);
                        }
                        else
                        {
                            Console.WriteLine($"Unknown airline code '{airlineCode}' for flight {flightNumber}. Skipping...");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading flights: {ex.Message}");
            }
        }

        // Method to display flights for a specific airline
        public static void DisplayAirlineFlights()
        {
            // Prompt the user to enter an airline code
            Console.WriteLine("Enter Airline Code: ");
            string airlineCode = Console.ReadLine().Trim().ToUpper();

            // Check if the entered airline code exists
            if (airlines.ContainsKey(airlineCode))
            {
                Airline airline = airlines[airlineCode];

                // Display airline details and flights
                Console.WriteLine("=============================================");
                Console.WriteLine($"List of Flights for {airline.Name}");
                Console.WriteLine("=============================================");
                Console.WriteLine("Flight Number   Origin                 Destination            Expected Departure/Arrival Time   Special Request");

                foreach (var flightPair in airline.Flights)
                {
                    Flight flight = flightPair.Value;
                    Console.WriteLine($"{flight.FlightNumber,-15} {flight.Origin,-20} {flight.Destination,-25} {flight.ExpectedTime.ToString("hh:mm tt"),-35} {flight.SpecialRequest}");
                }
            }
            else
            {
                // Invalid airline code entered
                Console.WriteLine("Invalid airline code entered. Please try again.");
            }
        }
    }
