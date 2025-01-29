using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using S10269430_PRG2Assignment;

// -------------------------------------------
// Top-Level Statements Begin
// -------------------------------------------

// 1) Create the Terminal
Terminal terminal = new Terminal("Changi Airport Terminal 5");

// 2) Load files (airlines + boarding gates + flights)
Console.WriteLine("Loading Airlines...");
LoadAirlines(terminal, "airlines.csv");
Console.WriteLine();

Console.WriteLine("Loading Boarding Gates...");
LoadBoardingGates(terminal, "boardinggates.csv");
Console.WriteLine();

Console.WriteLine("Loading Flights...");
LoadFlights(terminal, "flights.csv");
Console.WriteLine();

// Repeatedly show the main menu until the user chooses to exit
while (true)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("Welcome to Changi Airport Terminal 5");
    Console.WriteLine("=============================================");
    Console.WriteLine("1. List All Flights");
    Console.WriteLine("2. List Boarding Gates");
    Console.WriteLine("3. Assign a Boarding Gate to a Flight");
    Console.WriteLine("4. Create Flight");
    Console.WriteLine("5. Display Airline Flights");
    Console.WriteLine("6. Modify Flight Details");
    Console.WriteLine("7. Display Flight Schedule");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Please select your option:\n");

    string choice = (Console.ReadLine() ?? "").Trim();
    Console.WriteLine(); // extra spacing after user input

    switch (choice)
    {
        case "1":
            ListAllFlights(terminal);
            break;
        case "2":
            ListAllBoardingGates(terminal);
            break;
        case "3":
            AssignBoardingGate(terminal);
            break;
        case "4":
            CreateFlight(terminal, "flights.csv");
            break;
        case "5":
            DisplayAirlineFlights(terminal);
            break;
        case "6":
            ModifyFlightDetails(terminal);
            break;
        case "7":
            DisplayFlightScheduleChronologically(terminal);
            break;
        case "0":
            Console.WriteLine("Goodbye!");
            return; // Exit the program
        default:
            Console.WriteLine("Invalid option. Please try again.\n");
            break;
    }
}

// -------------------------------------------
// Local (static) Methods
// -------------------------------------------

static void LoadAirlines(Terminal terminal, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var lines = File.ReadAllLines(filePath).Skip(1); // skip header
    int count = 0;

    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        var columns = line.Split(',');
        if (columns.Length < 2) continue;

        string airlineName = columns[0].Trim();
        string airlineCode = columns[1].Trim();

        if (!terminal.Airlines.ContainsKey(airlineCode))
        {
            Airline airline = new Airline(airlineName, airlineCode);
            terminal.Airlines.Add(airlineCode, airline);
            count++;
        }
    }

    Console.WriteLine($"{count} Airlines Loaded!");
}


static void LoadFlights(Terminal terminal, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var lines = File.ReadAllLines(filePath).Skip(1); // skip header
    int count = 0;

    // By default, let's assume the CSV flights are all on 18/1/2025
    DateTime defaultDate = new DateTime(2025, 1, 18);

    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        var columns = line.Split(',');
        if (columns.Length < 4) continue;

        string flightNumber = columns[0].Trim();
        string origin = columns[1].Trim();
        string destination = columns[2].Trim();
        string timeString = columns[3].Trim();
        string specialRequestCode = (columns.Length > 4) ? columns[4].Trim() : "";

        // Attempt to parse time in "h:mm tt" format
        if (!DateTime.TryParseExact(timeString, "h:mm tt", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime timeOnly))
        {
            continue;
        }

        // Combine with default date
        DateTime flightDateTime = new DateTime(
            defaultDate.Year,
            defaultDate.Month,
            defaultDate.Day,
            timeOnly.Hour,
            timeOnly.Minute,
            0
        );

        // Create flight object
        Flight flight;
        switch (specialRequestCode.ToUpper())
        {
            case "DDJB":
                flight = new DDJBFlight(flightNumber, origin, destination, flightDateTime, "Scheduled");
                break;
            case "CFFT":
                flight = new CFFTFlight(flightNumber, origin, destination, flightDateTime, "Scheduled");
                break;
            case "LWTT":
                flight = new LWTTFlight(flightNumber, origin, destination, flightDateTime, "Scheduled");
                break;
            default:
                flight = new NORMFlight(flightNumber, origin, destination, flightDateTime, "Scheduled");
                break;
        }

        // Add flight to Terminal
        if (!terminal.Flights.ContainsKey(flightNumber))
        {
            terminal.Flights.Add(flightNumber, flight);

            // Also try to attach to correct airline if flightNumber prefix matches
            string airlineCodeCandidate = flightNumber.Split(' ')[0].Trim().ToUpper();
            if (terminal.Airlines.ContainsKey(airlineCodeCandidate))
            {
                terminal.Airlines[airlineCodeCandidate].Flights.Add(flightNumber, flight);
            }

            count++;
        }
    }

    Console.WriteLine($"{count} Flights Loaded!");
}

// ------------------------------------------------------------------------
// 1) LIST ALL FLIGHTS
// ------------------------------------------------------------------------
static void ListAllFlights(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("List of Flights for Changi Airport Terminal 5");
    Console.WriteLine("=============================================");

    // Print headings
    Console.WriteLine("Flight Number   Airline Name           Origin                 Destination              Expected Departure/Arrival Time");

    foreach (var flight in terminal.Flights.Values)
    {
        Airline airline = terminal.GetAirlineFromFlight(flight);
        string airlineName = (airline == null) ? "Unknown Airline" : airline.Name;

        Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4}",
            flight.FlightNumber,
            airlineName,
            flight.Origin,
            flight.Destination,
            flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    }
    Console.WriteLine();
}

// ------------------------------------------------------------------------
// 2) LIST ALL BOARDING GATES
// ------------------------------------------------------------------------
static void ListAllBoardingGates(Terminal terminal)
{
}

// ------------------------------------------------------------------------
// 3) ASSIGN A BOARDING GATE TO A FLIGHT
// ------------------------------------------------------------------------
static void AssignBoardingGate(Terminal terminal)
{
   
}

// ------------------------------------------------------------------------
// 4) CREATE A NEW FLIGHT
// ------------------------------------------------------------------------
static void CreateFlight(Terminal terminal, string flightsCsvPath)
{
    while (true)
    {
        Console.Write("Enter Flight Number: ");
        string flightNumber = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            Console.WriteLine("Invalid flight number. Try again.\n");
            continue;
        }

        Console.Write("Enter Origin: ");
        string origin = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Enter Destination: ");
        string destination = Console.ReadLine()?.Trim() ?? "";

        // parse date/time
        DateTime expectedDateTime;
        while (true)
        {
            Console.Write("Enter Expected Departure/Arrival Time (dd/mm/yyyy hh:mm): ");
            string dateTimeInput = (Console.ReadLine() ?? "").Trim();
            if (DateTime.TryParseExact(dateTimeInput, "d/M/yyyy H:mm", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out expectedDateTime))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid date/time format. Please use: dd/mm/yyyy hh:mm\n");
            }
        }

        Console.Write("Enter Special Request Code (CFFT/DDJB/LWTT/None): ");
        string spCode = (Console.ReadLine() ?? "").Trim().ToUpper();
        Flight newFlight;
        switch (spCode)
        {
            case "DDJB":
                newFlight = new DDJBFlight(flightNumber, origin, destination, expectedDateTime, "Scheduled");
                break;
            case "CFFT":
                newFlight = new CFFTFlight(flightNumber, origin, destination, expectedDateTime, "Scheduled");
                break;
            case "LWTT":
                newFlight = new LWTTFlight(flightNumber, origin, destination, expectedDateTime, "Scheduled");
                break;
            default:
                newFlight = new NORMFlight(flightNumber, origin, destination, expectedDateTime, "Scheduled");
                break;
        }

        if (!terminal.Flights.ContainsKey(flightNumber))
        {
            terminal.Flights.Add(flightNumber, newFlight);

            // Also link to airline if flight code matches
            string airlineCodeCandidate = flightNumber.Split(' ')[0].Trim().ToUpper();
            if (terminal.Airlines.ContainsKey(airlineCodeCandidate))
            {
                terminal.Airlines[airlineCodeCandidate].Flights.Add(flightNumber, newFlight);
            }

            // Append to CSV
            AppendFlightToCsv(flightsCsvPath, newFlight);
            Console.WriteLine($"Flight {flightNumber} has been added!\n");
        }
        else
        {
            Console.WriteLine($"Flight {flightNumber} already exists in the system!\n");
        }

        Console.Write("Would you like to add another flight? (Y/N): ");
        string again = (Console.ReadLine() ?? "").Trim().ToUpper();
        if (again != "Y") break;
    }


    Console.WriteLine();
}

static void AppendFlightToCsv(string filePath, Flight flight)
{
}

// ------------------------------------------------------------------------
// 5) DISPLAY AIRLINE FLIGHTS
// ------------------------------------------------------------------------
static void DisplayAirlineFlights(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("List of Airlines for Changi Airport Terminal 5");
    Console.WriteLine("=============================================");

    // Print headings first
    Console.WriteLine("Airline Code  Airline Name");

    // Then list each airline
    foreach (var airline in terminal.Airlines.Values)
    {
        Console.WriteLine("{0,-13} {1}",
            airline.Code,
            airline.Name);
    }
    Console.WriteLine();

    Console.Write("Enter Airline Code: ");
    string code = (Console.ReadLine() ?? "").Trim().ToUpper();
    Console.WriteLine();

    if (!terminal.Airlines.ContainsKey(code))
    {
        Console.WriteLine($"Airline Code {code} not found!\n");
        return;
    }

    Airline selectedAirline = terminal.Airlines[code];
    Console.WriteLine("=============================================");
    Console.WriteLine($"List of Flights for {selectedAirline.Name}");
    Console.WriteLine("=============================================");

    // Headings for flights
    Console.WriteLine("{0,-13} {1,-20} {2,-20} {3,-22} {4}",
        "Flight Number", "Airline Name", "Origin", "Destination", "Expected Time");

    // List flights
    foreach (var flight in selectedAirline.Flights.Values)
    {
        Console.WriteLine("{0,-13} {1,-20} {2,-20} {3,-22} {4}",
            flight.FlightNumber,
            selectedAirline.Name,
            flight.Origin,
            flight.Destination,
            flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    }
    Console.WriteLine();
}

// ------------------------------------------------------------------------
// 6) MODIFY FLIGHT DETAILS
// ------------------------------------------------------------------------
static void ModifyFlightDetails(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("List of Airlines for Changi Airport Terminal 5");
    Console.WriteLine("=============================================");

    // Print headings
    Console.WriteLine("Airline Code  Airline Name");
    foreach (var airline in terminal.Airlines.Values)
    {
        Console.WriteLine("{0,-13} {1}",
            airline.Code,
            airline.Name);
    }
    Console.WriteLine();

    Console.Write("Enter Airline Code: ");
    string airlineCode = (Console.ReadLine() ?? "").Trim().ToUpper();
    Console.WriteLine();

    if (!terminal.Airlines.ContainsKey(airlineCode))
    {
        Console.WriteLine($"Airline Code '{airlineCode}' not found!\n");
        return;
    }

    Airline selectedAirline = terminal.Airlines[airlineCode];
    Console.WriteLine($"List of Flights for {selectedAirline.Name}");
    // Print headings for the flights
    Console.WriteLine("Flight Number   Airline Name           Origin                 Destination              Expected Time");
    foreach (var f in selectedAirline.Flights.Values)
    {
        Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4}",
            f.FlightNumber,
            selectedAirline.Name,
            f.Origin,
            f.Destination,
            f.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    }
    Console.WriteLine();

    Console.Write("Choose an existing Flight Number to modify or delete: ");
    string chosenFlightNum = (Console.ReadLine() ?? "").Trim();
    Console.WriteLine();

    if (string.IsNullOrWhiteSpace(chosenFlightNum) || !selectedAirline.Flights.ContainsKey(chosenFlightNum))
    {
        Console.WriteLine($"Flight '{chosenFlightNum}' not found under {selectedAirline.Name}.\n");
        return;
    }

    Flight chosenFlight = selectedAirline.Flights[chosenFlightNum];

    Console.WriteLine("1. Modify Flight");
    Console.WriteLine("2. Delete Flight");
    Console.Write("Choose an option: ");
    string modOrDelete = (Console.ReadLine() ?? "").Trim();
    Console.WriteLine();

    if (modOrDelete == "1")
    {
        // Modify Flight
        Console.WriteLine("What would you like to modify?");
        Console.WriteLine("1. Modify Basic Information (Origin, Destination, Expected Time)");
        Console.WriteLine("2. Modify Status");
        Console.WriteLine("3. Modify Special Request Code");
        Console.WriteLine("4. Modify Boarding Gate");
        Console.Write("Choose an option: ");
        string modifyChoice = (Console.ReadLine() ?? "").Trim();
        Console.WriteLine();

        switch (modifyChoice)
        {
            case "1":
                ModifyBasicInformation(chosenFlight);
                break;
            case "2":
                ModifyFlightStatus(chosenFlight);
                break;
            case "3":
                ModifySpecialRequestCode(terminal, chosenFlight);
                break;
            case "4":
                ModifyBoardingGate(terminal, chosenFlight);
                break;
            default:
                Console.WriteLine("Invalid choice.\n");
                return;
        }

        // After modification, display updated flight details
        DisplayFullFlightDetails(terminal, chosenFlight);

    }
    else if (modOrDelete == "2")
    {
        // Delete Flight
        Console.Write($"Are you sure you want to delete Flight '{chosenFlightNum}'? (Y/N): ");
        string confirm = (Console.ReadLine() ?? "").Trim().ToUpper();
        Console.WriteLine();

        if (confirm == "Y")
        {
            BoardingGate gateAssigned = terminal.BoardingGates.Values
                .FirstOrDefault(g => g.Flight == chosenFlight);
            if (gateAssigned != null)
            {
                gateAssigned.Flight = null;
            }
            selectedAirline.Flights.Remove(chosenFlightNum);
            terminal.Flights.Remove(chosenFlightNum);

            Console.WriteLine($"Flight '{chosenFlightNum}' has been deleted.\n");
        }
        else
        {
            Console.WriteLine("Deletion cancelled.\n");
        }
    }
    else
    {
        Console.WriteLine("Invalid choice.\n");
    }
}

static void ModifyBasicInformation(Flight flight)
{
   
}

static void ModifyFlightStatus(Flight flight)
{
    
}



static void ModifyBoardingGate(Terminal terminal, Flight flight)
{
   

// Display updated flight details
static void DisplayFullFlightDetails(Terminal terminal, Flight flight)
{
    Airline parentAirline = terminal.GetAirlineFromFlight(flight);
    string airlineName = parentAirline != null ? parentAirline.Name : "Unknown Airline";

    BoardingGate gate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight);
    string gateName = gate != null ? gate.GateName : "Unassigned";

    string specialRequest = GetSpecialRequestCodeFromFlight(flight);
    if (string.IsNullOrEmpty(specialRequest)) specialRequest = "None";

    Console.WriteLine("Flight Number:          " + flight.FlightNumber);
    Console.WriteLine("Airline Name:           " + airlineName);
    Console.WriteLine("Origin:                 " + flight.Origin);
    Console.WriteLine("Destination:            " + flight.Destination);
    Console.WriteLine("Expected Departure/Arrival Time: " + flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    Console.WriteLine("Status:                 " + flight.Status);
    Console.WriteLine("Special Request Code:   " + specialRequest);
    Console.WriteLine("Boarding Gate:          " + gateName);
    Console.WriteLine();
}

// ------------------------------------------------------------------------
// 7) DISPLAY FLIGHT SCHEDULE CHRONOLOGICALLY
// ------------------------------------------------------------------------
static void DisplayFlightScheduleChronologically(Terminal terminal)
{
    
}

// ------------------------------------------------------------------------
// Utility: Determine special request code from flight type
// ------------------------------------------------------------------------
