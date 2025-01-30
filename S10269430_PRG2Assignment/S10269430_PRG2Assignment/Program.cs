using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using S10269430_PRG2Assignment;


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

static void LoadBoardingGates(Terminal terminal, string filePath)
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
        if (columns.Length < 4) continue;

        string gateName = columns[0].Trim();
        bool ddjb = bool.Parse(columns[1].Trim());
        bool cfft = bool.Parse(columns[2].Trim());
        bool lwtt = bool.Parse(columns[3].Trim());

        if (!terminal.BoardingGates.ContainsKey(gateName))
        {
            BoardingGate gate = new BoardingGate(gateName, cfft, ddjb, lwtt);
            terminal.BoardingGates.Add(gateName, gate);
            count++;
        }
    }

    Console.WriteLine($"{count} Boarding Gates Loaded!");
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
    Console.WriteLine("=============================================");
    Console.WriteLine("List of Boarding Gates for Changi Airport Terminal 5");
    Console.WriteLine("=============================================");

    // Print headings
    Console.WriteLine("Gate Name  DDJB   CFFT   LWTT");

    // Sort by gate name
    foreach (var gate in terminal.BoardingGates.Values.OrderBy(g => g.GateName))
    {
        Console.WriteLine("{0,-10} {1,-6} {2,-6} {3,-6}",
            gate.GateName,
            gate.SupportsDDJB,
            gate.SupportsCFFT,
            gate.SupportsLWTT
        );
    }
    Console.WriteLine();
}

// ------------------------------------------------------------------------
// 3) ASSIGN A BOARDING GATE TO A FLIGHT
// ------------------------------------------------------------------------
static void AssignBoardingGate(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("Assign a Boarding Gate to a Flight");
    Console.WriteLine("=============================================");

    Console.Write("Enter Flight Number: ");
    string flightNumber = (Console.ReadLine() ?? "").Trim();
    if (!terminal.Flights.ContainsKey(flightNumber))
    {
        Console.WriteLine($"Flight {flightNumber} not found!\n");
        return;
    }
    Flight selectedFlight = terminal.Flights[flightNumber];

    while (true)
    {
        Console.Write("Enter Boarding Gate Name: ");
        string gateName = (Console.ReadLine() ?? "").Trim();
        if (!terminal.BoardingGates.ContainsKey(gateName))
        {
            Console.WriteLine($"Boarding Gate {gateName} does not exist. Please try again.\n");
            continue;
        }

        BoardingGate gate = terminal.BoardingGates[gateName];

        // Check if gate is already assigned
        if (gate.Flight != null)
        {
            Console.WriteLine($"Boarding Gate {gate.GateName} is already assigned to flight {gate.Flight.FlightNumber}.\n");
            continue;
        }

        // Assign
        gate.Flight = selectedFlight;

        Console.WriteLine($"Flight Number: {selectedFlight.FlightNumber}");
        Console.WriteLine($"Origin: {selectedFlight.Origin}");
        Console.WriteLine($"Destination: {selectedFlight.Destination}");
        Console.WriteLine($"Expected Time: {selectedFlight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt")}");
        string spRequest = GetSpecialRequestCodeFromFlight(selectedFlight);
        Console.WriteLine($"Special Request Code: {(string.IsNullOrEmpty(spRequest) ? "None" : spRequest)}");

        // Show gate info separately
        Console.WriteLine($"Boarding Gate Name: {gate.GateName}");
        Console.WriteLine($"Supports DDJB: {gate.SupportsDDJB}");
        Console.WriteLine($"Supports CFFT: {gate.SupportsCFFT}");
        Console.WriteLine($"Supports LWTT: {gate.SupportsLWTT}");

        // Prompt to update status
        Console.Write("Would you like to update the status of the flight? (Y/N): ");
        string updateStatus = (Console.ReadLine() ?? "").Trim().ToUpper();
        if (updateStatus == "Y")
        {
            Console.WriteLine("1. Delayed");
            Console.WriteLine("2. Boarding");
            Console.WriteLine("3. On Time");
            Console.Write("Please select the new status of the flight: ");
            string opt = (Console.ReadLine() ?? "").Trim();
            switch (opt)
            {
                case "1":
                    selectedFlight.Status = "Delayed";
                    break;
                case "2":
                    selectedFlight.Status = "Boarding";
                    break;
                case "3":
                    selectedFlight.Status = "On Time";
                    break;
                default:
                    Console.WriteLine("Invalid choice, status remains the same.\n");
                    break;
            }
        }
        else
        {
            // By default, set to "On Time"
            selectedFlight.Status = "On Time";
        }

        Console.WriteLine($"\nFlight {selectedFlight.FlightNumber} has been assigned to Boarding Gate {gate.GateName}!\n");
        break;
    }
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
    string code = GetSpecialRequestCodeFromFlight(flight);
    string timeString = flight.ExpectedTime.ToString("h:mm tt", CultureInfo.InvariantCulture);

    // Example CSV line: "SQ 115,Tokyo (NRT),Singapore (SIN),11:45 AM,DDJB"
    string line = $"{flight.FlightNumber},{flight.Origin},{flight.Destination},{timeString},{code}";
    File.AppendAllText(filePath, Environment.NewLine + line);
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
    Console.Write($"Enter new Origin (current: {flight.Origin}): ");
    string newOrigin = Console.ReadLine()?.Trim();
    if (!string.IsNullOrWhiteSpace(newOrigin))
        flight.Origin = newOrigin;

    Console.Write($"Enter new Destination (current: {flight.Destination}): ");
    string newDestination = Console.ReadLine()?.Trim();
    if (!string.IsNullOrWhiteSpace(newDestination))
        flight.Destination = newDestination;

    while (true)
    {
        Console.Write($"Enter new Expected Departure/Arrival Time (dd/mm/yyyy hh:mm): ");
        string input = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(input)) break; // skip if user just presses Enter

        if (DateTime.TryParseExact(input, "d/M/yyyy H:mm", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime newDateTime))
        {
            flight.ExpectedTime = newDateTime;
            break;
        }
        else
        {
            Console.WriteLine("Invalid date/time format. Please use: dd/mm/yyyy hh:mm\n");
        }
    }


    Console.WriteLine("Flight updated!\n");
}

static void ModifyFlightStatus(Flight flight)
{
    Console.WriteLine("1. Delayed");
    Console.WriteLine("2. Boarding");
    Console.WriteLine("3. On Time");
    Console.Write("Select the new status: ");
    string opt = (Console.ReadLine() ?? "").Trim();
    switch (opt)
    {
        case "1":
            flight.Status = "Delayed";
            break;
        case "2":
            flight.Status = "Boarding";
            break;
        case "3":
            flight.Status = "On Time";
            break;
        default:
            Console.WriteLine("Invalid choice, status remains the same.\n");
            break;
    }
    Console.WriteLine($"Flight status updated to: {flight.Status}\n");
}

static void ModifySpecialRequestCode(Terminal terminal, Flight oldFlight)
{
    Console.Write("Enter new Special Request Code (DDJB / CFFT / LWTT / None): ");
    string newCode = (Console.ReadLine() ?? "").Trim().ToUpper();

    // Save existing flight properties
    string fn = oldFlight.FlightNumber;
    string orig = oldFlight.Origin;
    string dest = oldFlight.Destination;
    DateTime expTime = oldFlight.ExpectedTime;
    string status = oldFlight.Status;

    // Find if gate is assigned
    BoardingGate gateAssigned = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == oldFlight);

    // Remove old flight
    Airline parentAirline = terminal.GetAirlineFromFlight(oldFlight);
    if (parentAirline != null) parentAirline.Flights.Remove(fn);
    terminal.Flights.Remove(fn);

    // Create new flight with updated code
    Flight newFlight;
    switch (newCode)
    {
        case "DDJB":
            newFlight = new DDJBFlight(fn, orig, dest, expTime, status);
            break;
        case "CFFT":
            newFlight = new CFFTFlight(fn, orig, dest, expTime, status);
            break;
        case "LWTT":
            newFlight = new LWTTFlight(fn, orig, dest, expTime, status);
            break;
        default:
            newFlight = new NORMFlight(fn, orig, dest, expTime, status);
            break;
    }

    // Add back to terminal & airline
    terminal.Flights.Add(fn, newFlight);
    if (parentAirline != null) parentAirline.Flights.Add(fn, newFlight);

    // Reassign gate if needed
    if (gateAssigned != null)
    {
        gateAssigned.Flight = newFlight;
    }

    Console.WriteLine("Special Request Code updated!\n");
}

static void ModifyBoardingGate(Terminal terminal, Flight flight)
{
    // Unassign old gate
    BoardingGate oldGate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight);
    if (oldGate != null)
    {
        oldGate.Flight = null;
    }

    while (true)
    {
        Console.Write("Enter new Boarding Gate Name: ");
        string newGateName = (Console.ReadLine() ?? "").Trim();
        if (!terminal.BoardingGates.ContainsKey(newGateName))
        {
            Console.WriteLine($"Boarding Gate '{newGateName}' does not exist. Please try again.\n");
            continue;
        }

        BoardingGate newGate = terminal.BoardingGates[newGateName];
        if (newGate.Flight != null)
        {
            Console.WriteLine($"Gate '{newGate.GateName}' is already assigned to flight '{newGate.Flight.FlightNumber}'.\n");
            continue;
        }

        newGate.Flight = flight;
        Console.WriteLine($"Flight '{flight.FlightNumber}' is now assigned to gate '{newGate.GateName}'.\n");
        break;
    }
}

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
    Console.WriteLine("=============================================");
    Console.WriteLine("Flight Schedule for Changi Airport Terminal 5");
    Console.WriteLine("=============================================");

    // Sort flights by ExpectedTime
    var sortedFlights = terminal.Flights.Values.OrderBy(f => f.ExpectedTime).ToList();


    Console.WriteLine("{0,-13} {1,-20} {2,-20} {3,-22} {4,-32} {5,-12} {6}",
        "Flight Number",
        "Airline Name",
        "Origin",
        "Destination",
        "Expected Departure/Arrival Time",
        "Status",
        "Boarding Gate");

    foreach (var flight in sortedFlights)
    {
        Airline airline = terminal.GetAirlineFromFlight(flight);
        string airlineName = airline != null ? airline.Name : "Unknown Airline";

        // Check if assigned
        BoardingGate gate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight);
        string gateName = gate != null ? gate.GateName : "Unassigned";

        Console.WriteLine("{0,-13} {1,-20} {2,-20} {3,-22} {4,-32} {5,-12} {6}",
            flight.FlightNumber,
            airlineName,
            flight.Origin,
            flight.Destination,
            flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"),
            flight.Status,
            gateName);
    }
    Console.WriteLine();
}

// ------------------------------------------------------------------------
// Utility: Determine special request code from flight type
// ------------------------------------------------------------------------
static string GetSpecialRequestCodeFromFlight(Flight flight)
{
    if (flight is DDJBFlight) return "DDJB";
    if (flight is CFFTFlight) return "CFFT";
    if (flight is LWTTFlight) return "LWTT";
    return "";
}
