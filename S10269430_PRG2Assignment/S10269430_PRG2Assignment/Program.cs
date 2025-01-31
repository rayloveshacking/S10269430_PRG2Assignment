using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using S10269430_PRG2Assignment;

/*
//==========================================================
// Student Number : S10269430K
// Student Name : Thar Htet Shein
// Partner Name : Nadella Bhaveesh Sai
//==========================================================
*/

// 1) Create the Terminal
Terminal terminal = new Terminal("Changi Airport Terminal 5");

// 2) Load files (airlines + boarding gates + flights)
Console.WriteLine("Loading Airlines...");
LoadAirlines(terminal, "airlines.csv");


Console.WriteLine("Loading Boarding Gates...");
LoadBoardingGates(terminal, "boardinggates.csv");


Console.WriteLine("Loading Flights...");
LoadFlights(terminal, "flights.csv");



// Repeatedly show the main menu until the user chooses to exit
while (true)
{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
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
    Console.WriteLine("8. Bulk Process Unassigned Flights"); //For new advanced feature
    Console.WriteLine("9. Display Total Fee per Airline for the Day"); //New advanced feature
    Console.WriteLine("10. Search and Filter Flights"); //Additional Feature A by Thar Htet Shein
    Console.WriteLine("11. Display Weather at Changi"); //Additional Feature A by Nadella Bhaveesh Sai
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Please select your option:\n");

    string choice = (Console.ReadLine() ?? "").Trim();


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
        case "8":
            BulkProcessUnassignedFlights(terminal);
            break;
        case "9":
            DisplayTotalFeePerAirlineForTheDay(terminal);
            break;
        case "10":
            SearchAndFilterFlights(terminal);
            break;
        case "11":
            await WeatherDisplay.DisplayWeather();
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
        if (columns.Length < 2) continue; //This will check if the line have two columns or not

        string airlineName = columns[0].Trim();
        string airlineCode = columns[1].Trim();

        if (!terminal.Airlines.ContainsKey(airlineCode)) //We will check whether the AirLines dictionary from terminal class contains the code or not
        {
            Airline airline = new Airline(airlineName, airlineCode); //If not then we will create a new airline with the above information
            terminal.Airlines.Add(airlineCode, airline); //And here we will add it to airlines dictionary in the terminal class
            count++; //This will increase the count
        }
    }

    Console.WriteLine($"{count} Airlines Loaded!");
}

static void LoadBoardingGates(Terminal terminal, string filePath) //Input the terminal and filepath as the params through this function
{
    if (!File.Exists(filePath)) //This will check whether the file exists or not first
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var lines = File.ReadAllLines(filePath).Skip(1); // skip header
    int count = 0;

    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue; //Check first if the string is null or whitespaces
        var columns = line.Split(','); //Split the lines and assign in to columns variable
        if (columns.Length < 4) continue; //Check if there are 4 columns or not

        string gateName = columns[0].Trim();
        bool ddjb = bool.Parse(columns[1].Trim()); //Passes true or false
        bool cfft = bool.Parse(columns[2].Trim());
        bool lwtt = bool.Parse(columns[3].Trim());

        if (!terminal.BoardingGates.ContainsKey(gateName)) //Check if the BoardingGates Dictionary from terminal contain the gatename or not
        {
            BoardingGate gate = new BoardingGate(gateName, cfft, ddjb, lwtt); //If not then a new BoardingGate will be created
            terminal.BoardingGates.Add(gateName, gate); //Added to the BoardingGates Dictionary
            count++;
        }
    }

    Console.WriteLine($"{count} Boarding Gates Loaded!");
}

static void LoadFlights(Terminal terminal, string filePath) //This method will take terminal and filepath as the parameters.
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var lines = File.ReadAllLines(filePath).Skip(1); // skip header
    int count = 0;

    // The date will be the current date 
    DateTime defaultDate = DateTime.Today;

    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue; //As usual we will check if there are any null or less than the columns expected
        var columns = line.Split(',');
        if (columns.Length < 4) continue;

        string flightNumber = columns[0].Trim(); //take the specific parts from the variable accordingly.
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

        // Create flight object depending on the special request code
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

    // Print headings with appropriate spacing
    Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4}",
        "Flight Number", "Airline Name", "Origin", "Destination", "Expected Departure/Arrival Time");


    foreach (var flight in terminal.Flights.Values)
    {
        Airline airline = terminal.GetAirlineFromFlight(flight); //Get the airline from terminal 
        string airlineName = (airline == null) ? "Unknown Airline" : airline.Name; //If airline is null, it's unknown airline, if not airline name
        string formattedTime = flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt").ToLower(); //Format the time to the desired time format.

        Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4}", //Print all the information out.
            flight.FlightNumber,
            airlineName,
            flight.Origin,
            flight.Destination,
            formattedTime);
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

    // For headings, each column is 25 characters wide (-25 => left aligned)
    Console.WriteLine("{0,-25}{1,-25}{2,-25}{3,-25}",
        "Gate Name", "DDJB", "CFFT", "LWTT");

    // Sort by gate name
    foreach (var gate in terminal.BoardingGates.Values.OrderBy(g => g.GateName))
    {
        // Match the same widths for the data
        Console.WriteLine("{0,-25}{1,-25}{2,-25}{3,-25}",
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

    Console.Write("Enter Flight Number:\n");
    string flightNumber = (Console.ReadLine() ?? "").Trim();
    if (!terminal.Flights.ContainsKey(flightNumber))
    {
        Console.WriteLine($"Flight {flightNumber} not found!\n");
        return;
    }
    Flight selectedFlight = terminal.Flights[flightNumber]; //Select a flight with the flight number.

    while (true)
    {
        Console.Write("Enter Boarding Gate Name:\n");
        string gateName = (Console.ReadLine() ?? "").Trim(); //Take the gatename from the user
        if (!terminal.BoardingGates.ContainsKey(gateName)) //Search if there is a boarding gate in the BoardingGates Dictionary.
        {
            Console.WriteLine($"Boarding Gate {gateName} does not exist. Please try again.\n");
            continue;
        }

        BoardingGate gate = terminal.BoardingGates[gateName]; //Assign to a boarding gate variable if there is a gate

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
        Console.Write("Would you like to update the status of the flight? (Y/N):\n");
        string updateStatus = (Console.ReadLine() ?? "").Trim().ToUpper();
        if (updateStatus == "Y")
        {
            Console.WriteLine("1. Delayed");
            Console.WriteLine("2. Boarding");
            Console.WriteLine("3. On Time");
            Console.Write("Please select the new status of the flight:\n");
            string opt = (Console.ReadLine() ?? "").Trim();
            switch (opt)
            {
                case "1":
                    selectedFlight.Status = "Delayed"; //Assign the status of the flight depending on the user option.
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

        Console.WriteLine($"Flight {selectedFlight.FlightNumber} has been assigned to Boarding Gate {gate.GateName}!");
        break;
    }
}

// ------------------------------------------------------------------------
// 4) CREATE A NEW FLIGHT
// ------------------------------------------------------------------------
static void CreateFlight(Terminal terminal, string flightsCsvPath) //Give the method a terminal and the flights csv
{
    while (true)
    {
        Console.Write("Enter Flight Number: ");
        string flightNumber = (Console.ReadLine() ?? "").Trim(); //Take the flight number from the user
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            Console.WriteLine("Invalid flight number. Try again.\n");
            continue;
        }

        Console.Write("Enter Origin: ");
        string origin = Console.ReadLine()?.Trim() ?? ""; //Take the origin from the user
        Console.Write("Enter Destination: ");
        string destination = Console.ReadLine()?.Trim() ?? ""; //Take the destination from the user

        // parse date/time
        DateTime expectedDateTime;
        while (true)
        {
            Console.Write("Enter Expected Departure/Arrival Time (dd/mm/yyyy hh:mm): ");
            string dateTimeInput = (Console.ReadLine() ?? "").Trim();
            if (DateTime.TryParseExact(dateTimeInput, "d/M/yyyy H:mm", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out expectedDateTime)) //Take the expected date time from the user and parse it in the exact format.
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
            Console.WriteLine($"Flight {flightNumber} has been added!");
        }
        else
        {
            Console.Write($"Flight {flightNumber} already exists in the system!\n");
        }

        Console.Write("Would you like to add another flight? (Y/N):\n");
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

    Console.Write("Enter Airline Code: ");
    string code = (Console.ReadLine() ?? "").Trim().ToUpper();


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
        "Flight Number", "Airline Name", "Origin", "Destination", "Expected Departure/Arrival Time");

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


    Console.Write("Enter Airline Code:\n");
    string airlineCode = (Console.ReadLine() ?? "").Trim().ToUpper();


    if (!terminal.Airlines.ContainsKey(airlineCode))
    {
        Console.WriteLine($"Airline Code '{airlineCode}' not found!\n");
        return;
    }

    Airline selectedAirline = terminal.Airlines[airlineCode]; //Select the airline from airlines dictionary using the airline code from the user
    Console.WriteLine($"List of Flights for {selectedAirline.Name}");
    // Print headings for the flights
    Console.WriteLine("Flight Number   Airline Name           Origin                 Destination              Expected Departure/Arrival Time");
    foreach (var f in selectedAirline.Flights.Values)
    {
        Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4}",
            f.FlightNumber,
            selectedAirline.Name,
            f.Origin,
            f.Destination,
            f.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    }


    Console.Write("Choose an existing Flight to modify or delete:\n");
    string chosenFlightNum = (Console.ReadLine() ?? "").Trim();


    if (string.IsNullOrWhiteSpace(chosenFlightNum) || !selectedAirline.Flights.ContainsKey(chosenFlightNum)) //Check if user enter nothing or the flight dont exist
    {
        Console.WriteLine($"Flight '{chosenFlightNum}' not found under {selectedAirline.Name}.\n");
        return;
    }

    Flight chosenFlight = selectedAirline.Flights[chosenFlightNum]; //Select the flight from Flights based on user input

    Console.WriteLine("1. Modify Flight");
    Console.WriteLine("2. Delete Flight");
    Console.Write("Choose an option:\n");
    string modOrDelete = (Console.ReadLine() ?? "").Trim();


    if (modOrDelete == "1")
    {

        Console.WriteLine("1. Modify Basic Information");
        Console.WriteLine("2. Modify Status");
        Console.WriteLine("3. Modify Special Request Code");
        Console.WriteLine("4. Modify Boarding Gate");
        Console.Write("Choose an option:\n");
        string modifyChoice = (Console.ReadLine() ?? "").Trim();


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
                .FirstOrDefault(g => g.Flight == chosenFlight); //This is used to find the first boarding gate whose flight matches the properties of the chosen Flight.
            if (gateAssigned != null)
            {
                gateAssigned.Flight = null; //If found it will become null
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
    Console.Write($"Enter new Origin: ");
    string newOrigin = Console.ReadLine()?.Trim();
    if (!string.IsNullOrWhiteSpace(newOrigin))
        flight.Origin = newOrigin; //Change the origin of the flight to the new origin.

    Console.Write($"Enter new Destination: ");
    string newDestination = Console.ReadLine()?.Trim();
    if (!string.IsNullOrWhiteSpace(newDestination))
        flight.Destination = newDestination; //Change the destination of the flight to the new destination.

    while (true)
    {
        Console.Write($"Enter new Expected Departure/Arrival Time (dd/mm/yyyy hh:mm): ");
        string input = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(input)) break; // skip if user just presses Enter

        if (DateTime.TryParseExact(input, "d/M/yyyy H:mm", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime newDateTime)) //Format the time 
        {
            flight.ExpectedTime = newDateTime; //Change the expected time of the flight to the new date time.
            break;
        }
        else
        {
            Console.WriteLine("Invalid date/time format. Please use: dd/mm/yyyy hh:mm\n");
        }
    }


    Console.WriteLine("Flight updated!");
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
    BoardingGate oldGate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight); //Find the first boarding gate that matches the flight property of the given flight.
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

        BoardingGate newGate = terminal.BoardingGates[newGateName]; //Choose the newgate based on the user entered gatename.
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
    Airline parentAirline = terminal.GetAirlineFromFlight(flight); //Choose the flight from the terminal
    string airlineName = parentAirline != null ? parentAirline.Name : "Unknown Airline"; //If parent airline is not null, either parent air line name or unknown airline if dont exist.

    BoardingGate gate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight); //Choose a gate from the boarding gates for the flight that matches the given flight
    string gateName = gate != null ? gate.GateName : "Unassigned";

    string specialRequest = GetSpecialRequestCodeFromFlight(flight); //Get the special request code from the flight.
    if (string.IsNullOrEmpty(specialRequest)) specialRequest = "None";

    Console.WriteLine("Flight Number: " + flight.FlightNumber);
    Console.WriteLine("Airline Name: " + airlineName);
    Console.WriteLine("Origin: " + flight.Origin);
    Console.WriteLine("Destination: " + flight.Destination);
    Console.WriteLine("Expected Departure/Arrival Time: " + flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"));
    Console.WriteLine("Status: " + flight.Status);
    Console.WriteLine("Special Request Code: " + specialRequest);
    Console.WriteLine("Boarding Gate: " + gateName);
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

// ------------------------------------------------------------------------
// Thar Htet Shein's Advanced Feature A
// ------------------------------------------------------------------------

static void BulkProcessUnassignedFlights(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("Bulk Processing of Unassigned Flights");
    Console.WriteLine("=============================================");

    // 1) Build a queue of unassigned flights
    Queue<Flight> unassignedFlights = new Queue<Flight>();

    // also track how many flights are currently assigned
    int initiallyAssignedFlights = 0;

    foreach (var flight in terminal.Flights.Values)
    {
        // If no gate is found with flight == this flight, it's unassigned
        BoardingGate gate = terminal.BoardingGates.Values.FirstOrDefault(g => g.Flight == flight);
        if (gate == null)
        {
            unassignedFlights.Enqueue(flight);
        }
        else
        {
            initiallyAssignedFlights++;
        }
    }

    // 2) Display how many flights are unassigned
    int totalUnassignedFlights = unassignedFlights.Count;
    Console.WriteLine($"Total number of Flights that do NOT have any Boarding Gate assigned yet: {totalUnassignedFlights}");

    // 3) Count how many gates are unassigned
    var unassignedGatesList = terminal.BoardingGates.Values.Where(g => g.Flight == null).ToList();
    int totalUnassignedGates = unassignedGatesList.Count;
    Console.WriteLine($"Total number of Boarding Gates that do NOT have a Flight assigned yet: {totalUnassignedGates}");

    Console.WriteLine();
    Console.WriteLine("Press ENTER to begin bulk assignment...");
    Console.ReadLine();

    // 4) Dequeue flights one by one, find a suitable gate, and assign
    int flightsAssignedNow = 0;

    while (unassignedFlights.Count > 0)
    {
        Flight flight = unassignedFlights.Dequeue();

        // Check if flight has a special request
        string specialRequestCode = GetSpecialRequestCodeFromFlight(flight);
        // e.g., "DDJB", "CFFT", "LWTT", or "" for none

        // Try to find a matching gate
        BoardingGate suitableGate = null;

        if (!string.IsNullOrEmpty(specialRequestCode))
        {
            // Flight requires special request
            // e.g., for "DDJB", look for gate.SupportsDDJB == true, etc.
            suitableGate = FindUnassignedGateForRequest(terminal, specialRequestCode);
        }
        else
        {
            // No special request => find a gate that has no special request support
            suitableGate = terminal.BoardingGates.Values.FirstOrDefault(g =>
                g.Flight == null &&
                g.SupportsDDJB == false &&
                g.SupportsCFFT == false &&
                g.SupportsLWTT == false);
        }

        if (suitableGate != null)
        {
            // Assign
            suitableGate.Flight = flight;
            flightsAssignedNow++;

            // Display flight details
            Airline airline = terminal.GetAirlineFromFlight(flight);
            string airlineName = airline != null ? airline.Name : "Unknown Airline";
            Console.WriteLine("Assigned Flight:");
            Console.WriteLine($"  Flight Number:   {flight.FlightNumber}");
            Console.WriteLine($"  Airline Name:    {airlineName}");
            Console.WriteLine($"  Origin:          {flight.Origin}");
            Console.WriteLine($"  Destination:     {flight.Destination}");
            Console.WriteLine($"  Expected Time:   {flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt")}");
            Console.WriteLine($"  Special Request: {(string.IsNullOrEmpty(specialRequestCode) ? "None" : specialRequestCode)}");
            Console.WriteLine($"  Assigned Gate:   {suitableGate.GateName}");
            Console.WriteLine();
        }
        else
        {
            // No suitable gate found for this flight
            // (Optional) might re-queue it or just leave it unassigned.
            Console.WriteLine($"No suitable unassigned gate found for Flight {flight.FlightNumber} (Request={specialRequestCode}).");
        }
    }

    // 5) Display summary
    int totalAssignedFlightsFinal = initiallyAssignedFlights + flightsAssignedNow;
    Console.WriteLine("=============================================");
    Console.WriteLine("Bulk Assignment Summary");
    Console.WriteLine("=============================================");
    Console.WriteLine($"Flights automatically assigned this round:  {flightsAssignedNow}");
    Console.WriteLine($"Flights that were already assigned before:  {initiallyAssignedFlights}");
    Console.WriteLine($"Total flights currently assigned:           {totalAssignedFlightsFinal}");
    Console.WriteLine();

    // Compute a percentage = (# assigned automatically / total assigned) * 100
    double percentageFlights = 0;
    if (totalAssignedFlightsFinal > 0)
    {
        percentageFlights = ((double)flightsAssignedNow / totalAssignedFlightsFinal) * 100;
    }
    Console.WriteLine($"Percentage of flights auto-assigned vs. all assigned flights: {percentageFlights:0.00}%");

    // Similarly for gates
    int initiallyAssignedGates = terminal.BoardingGates.Values.Count(g => g.Flight != null) - flightsAssignedNow;
    // or simply `initiallyAssignedGates = initiallyAssignedFlights;` 
    // because each assigned flight implies a gate is also assigned.
    int totalAssignedGatesFinal = initiallyAssignedGates + flightsAssignedNow;

    double percentageGates = 0;
    if (totalAssignedGatesFinal > 0)
    {
        percentageGates = ((double)flightsAssignedNow / totalAssignedGatesFinal) * 100;
    }
    Console.WriteLine($"Percentage of gates auto-assigned vs. all assigned gates:   {percentageGates:0.00}%");
    Console.WriteLine();
}

/// <summary>
/// Finds an unassigned gate that supports the given special request code.
/// 'requestCode' will be one of: "DDJB", "CFFT", "LWTT"
/// If none is found, returns null.
/// </summary>
static BoardingGate FindUnassignedGateForRequest(Terminal terminal, string requestCode)
{
    foreach (var gate in terminal.BoardingGates.Values)
    {
        if (gate.Flight != null) continue; // already assigned

        switch (requestCode)
        {
            case "DDJB":
                if (gate.SupportsDDJB) return gate;
                break;
            case "CFFT":
                if (gate.SupportsCFFT) return gate;
                break;
            case "LWTT":
                if (gate.SupportsLWTT) return gate;
                break;
        }
    }
    return null; // no match
}

// ------------------------------------------------------------------------
// Bhaveesh's Advanced Feature B
// ------------------------------------------------------------------------
static void DisplayTotalFeePerAirlineForTheDay(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("Display the Total Fee per Airline for the Day");
    Console.WriteLine("=============================================");

    // 1) Check that ALL flights have been assigned a boarding gate
    bool anyUnassigned = terminal.Flights.Values.Any(f =>
        terminal.BoardingGates.Values.All(bg => bg.Flight != f));

    if (anyUnassigned)
    {
        Console.WriteLine("ERROR: Not all flights have been assigned to boarding gates!");
        Console.WriteLine("Please assign gates to ALL flights before running this feature.\n");
        return;
    }

    // 2) Compute and display fees for each airline
    double grandSubtotal = 0.0;
    double grandDiscountTotal = 0.0;
    double grandFinalTotal = 0.0;

    // list airlines in alphabetical order of their Code, for clarity
    var sortedAirlines = terminal.Airlines.Values.OrderBy(a => a.Code).ToList();

    if (sortedAirlines.Count == 0)
    {
        Console.WriteLine("No airlines found!\n");
        return;
    }

    foreach (var airline in sortedAirlines)
    {
        var flights = airline.Flights.Values.ToList();
        int flightCount = flights.Count;

        if (flightCount == 0)
        {
            // If an airline has no flights, skip or show $0
            Console.WriteLine($"{airline.Name} ({airline.Code}) has no flights.\n");
            continue;
        }

        // (a) Subtotal (before any discount) is simply the sum of each flight's .CalculateFees().
        //     This includes: base fee ($300) + (destination-based 500/800) + any special request fees.
        double subTotal = flights.Sum(f => f.CalculateFees());

        // (b) Compute discounts just like the logic in Airline.CalculateFees():
        //     discount1 = (Flights.Count / 3) * 350
        //     discount2 = 110 for flights departing < 11:00 or >= 21:00
        //     discount3 = 25 for flights from origin in {DXB, BKK, NRT}
        //     discount4 = 50 for each NORMFlight
        //     volume discount = 3% off subTotal if flightCount > 5

        // First, gather the counts for discount2/3/4:
        int discount2Count = 0; // flights < 11:00 or >= 21:00
        int discount3Count = 0; // origin is DXB, BKK, or NRT
        int discount4Count = 0; // flight is NORMFlight

        foreach (var flight in flights)
        {
            DateTime t = flight.ExpectedTime;
            if (t.Hour < 11 || t.Hour >= 21) discount2Count++;

            if (new[] { "DXB", "BKK", "NRT" }.Contains(flight.Origin))
                discount3Count++;

            if (flight is NORMFlight) discount4Count++;
        }

        // discount #1
        int discount1 = (flightCount / 3) * 350;
        // discount #2
        int discount2 = discount2Count * 110;
        // discount #3
        int discount3 = discount3Count * 25;
        // discount #4
        int discount4 = discount4Count * 50;

        double volumeDiscount = 0.0;
        if (flightCount > 5)
        {
            // 3% off the *subtotal*
            volumeDiscount = subTotal * 0.03;
        }

        double discountPromotionsSum = discount1 + discount2 + discount3 + discount4;
        double totalDiscounts = discountPromotionsSum + volumeDiscount;

        // (c) Final total = subtotal - all discounts
        double finalTotal = subTotal - totalDiscounts;

        // 3) Display breakdown for this airline
        Console.WriteLine($"Airline: {airline.Name} ({airline.Code})");
        Console.WriteLine($"  Subtotal (No Discounts):            ${subTotal:0.00}");
        Console.WriteLine($"  Volume Discount (if any):           ${volumeDiscount:0.00}");
        Console.WriteLine($"  Promotional Discounts (1~4 total):  ${discountPromotionsSum:0.00}");
        Console.WriteLine($"  TOTAL DISCOUNTS:                    ${totalDiscounts:0.00}");
        Console.WriteLine($"  Final Fees to be Charged:           ${finalTotal:0.00}");
        Console.WriteLine();

        // Accumulate for the grand totals
        grandSubtotal += subTotal;
        grandDiscountTotal += totalDiscounts;
        grandFinalTotal += finalTotal;
    }

    // 4) After listing each airline, display the overall summary
    Console.WriteLine("===========================================================");
    Console.WriteLine("Summary of All Airline Fees for the Day");
    Console.WriteLine("===========================================================");
    Console.WriteLine($"Grand Subtotal (before discounts):   ${grandSubtotal:0.00}");
    Console.WriteLine($"Grand Total of Discounts:            ${grandDiscountTotal:0.00}");
    Console.WriteLine($"Grand Final Total (fees collected):  ${grandFinalTotal:0.00}");

    // Percentage of the subtotal discounts over the final total

    double discountPercentage = 0.0;
    if (grandFinalTotal != 0)
    {
        discountPercentage = (grandDiscountTotal / grandFinalTotal) * 100.0;
    }
    Console.WriteLine($"Percentage of discounts over final total: {discountPercentage:0.00}%\n");
}

// ------------------------------------------------------------------------
// Thar Htet Shein's Additional Feature
// ------------------------------------------------------------------------
static void SearchAndFilterFlights(Terminal terminal)
{
    Console.WriteLine("=============================================");
    Console.WriteLine("Flight Search & Filter");
    Console.WriteLine("=============================================");

    Console.WriteLine("Choose a filter type:");
    Console.WriteLine("1. By Flight Number");
    Console.WriteLine("2. By Origin");
    Console.WriteLine("3. By Destination");
    Console.WriteLine("4. By Time Range");
    Console.WriteLine("0. Cancel");
    Console.Write("Enter your choice: ");
    string choice = (Console.ReadLine() ?? "").Trim();
    Console.WriteLine();

    // If user wants to cancel
    if (choice == "0")
    {
        Console.WriteLine("Returning to Main Menu...\n");
        return;
    }

    IEnumerable<Flight> filteredFlights = terminal.Flights.Values; // Start with all flights

    switch (choice)
    {
        case "1":
            filteredFlights = FilterByFlightNumber(filteredFlights);
            break;
        case "2":
            filteredFlights = FilterByOrigin(filteredFlights);
            break;
        case "3":
            filteredFlights = FilterByDestination(filteredFlights);
            break;
        case "4":
            filteredFlights = FilterByTimeRange(filteredFlights);
            break;
        default:
            Console.WriteLine("Invalid choice. Returning to Main Menu...\n");
            return;
    }

    // Convert to a list so we can reuse
    var results = filteredFlights.ToList();
    if (results.Count == 0)
    {
        Console.WriteLine("No flights found matching the criteria.\n");
        return;
    }

    // Display the results
    Console.WriteLine("Flights Matching Your Search:");
    Console.WriteLine("Flight Number   Airline Name           Origin                 Destination              Expected Time           Status");
    foreach (var flight in results)
    {
        Airline airline = terminal.GetAirlineFromFlight(flight);
        string airlineName = airline != null ? airline.Name : "Unknown Airline";
        Console.WriteLine("{0,-15} {1,-22} {2,-22} {3,-24} {4,-24} {5}",
            flight.FlightNumber,
            airlineName,
            flight.Origin,
            flight.Destination,
            flight.ExpectedTime.ToString("d/M/yyyy h:mm:ss tt"),
            flight.Status);
    }
    Console.WriteLine();
}

// =========================
// FILTER HELPER METHODS
// =========================

static IEnumerable<Flight> FilterByFlightNumber(IEnumerable<Flight> flights)
{
    Console.Write("Enter Flight Number to search (partial or full): ");
    string input = (Console.ReadLine() ?? "").Trim().ToUpper();
    if (string.IsNullOrWhiteSpace(input))
    {
        // If user didn't enter anything, return the original list (no filter)
        return flights;
    }

    return flights.Where(f => f.FlightNumber.ToUpper().Contains(input));
}

static IEnumerable<Flight> FilterByOrigin(IEnumerable<Flight> flights)
{
    Console.Write("Enter Origin to search (partial or full): ");
    string input = (Console.ReadLine() ?? "").Trim().ToUpper();
    if (string.IsNullOrWhiteSpace(input))
    {
        return flights; // no filter if blank
    }

    return flights.Where(f => f.Origin.ToUpper().Contains(input));
}

static IEnumerable<Flight> FilterByDestination(IEnumerable<Flight> flights)
{
    Console.Write("Enter Destination to search (partial or full): ");
    string input = (Console.ReadLine() ?? "").Trim().ToUpper();
    if (string.IsNullOrWhiteSpace(input))
    {
        return flights;
    }

    return flights.Where(f => f.Destination.ToUpper().Contains(input));
}

static IEnumerable<Flight> FilterByTimeRange(IEnumerable<Flight> flights)
{
    Console.Write("Enter Start Date/Time (dd/MM/yyyy HH:mm) or press Enter to skip: ");
    string startInput = (Console.ReadLine() ?? "").Trim();
    Console.Write("Enter End Date/Time (dd/MM/yyyy HH:mm) or press Enter to skip: ");
    string endInput = (Console.ReadLine() ?? "").Trim();

    DateTime? startTime = null;
    DateTime? endTime = null;

    // Try parse start time
    if (!string.IsNullOrWhiteSpace(startInput))
    {
        if (DateTime.TryParseExact(startInput, "d/M/yyyy H:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStart))
        {
            startTime = parsedStart;
        }
        else
        {
            Console.WriteLine("Invalid start date/time format. Ignoring start time filter.\n");
        }
    }
    // Try parse end time
    if (!string.IsNullOrWhiteSpace(endInput))
    {
        if (DateTime.TryParseExact(endInput, "d/M/yyyy H:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEnd))
        {
            endTime = parsedEnd;
        }
        else
        {
            Console.WriteLine("Invalid end date/time format. Ignoring end time filter.\n");
        }
    }

    // Filter
    IEnumerable<Flight> result = flights;
    if (startTime.HasValue)
    {
        result = result.Where(f => f.ExpectedTime >= startTime.Value);
    }
    if (endTime.HasValue)
    {
        result = result.Where(f => f.ExpectedTime <= endTime.Value);
    }

    return result;
}

// ------------------------------------------------------------------------
// Bhaveesh's Additional Feature
// ------------------------------------------------------------------------
public class WeatherDisplay
{
    public static async Task DisplayWeather()
    {
        using HttpClient client = new();
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        string url = $"https://api.data.gov.sg/v1/environment/4-day-weather-forecast?date={todayDate}";

        var obj = await FetchWeatherAsync(client, url);
        if (obj?.items?.Length > 0 && obj.items[0].forecasts.Length > 0)
        {
            var todayForecast = obj.items[0].forecasts[0];

            Console.WriteLine($"Weather Forecast for Today ({todayForecast.date}):");
            Console.WriteLine($"- Forecast: {todayForecast.forecast}");
            Console.WriteLine($"- Temperature: {todayForecast.temperature.low}°C - {todayForecast.temperature.high}°C");
            Console.WriteLine($"- Humidity: {todayForecast.relative_humidity.low}% - {todayForecast.relative_humidity.high}%");
            Console.WriteLine($"- Wind: {todayForecast.wind.speed.low} km/h - {todayForecast.wind.speed.high} km/h, Direction: {todayForecast.wind.direction}");
        }
        else
        {
            Console.WriteLine($"No weather data available for today ({todayDate}).");
        }
    }

    // Async method to fetch the weather data from the API
    private static async Task<Rootobject?> FetchWeatherAsync(HttpClient client, string url)
    {
        using Stream stream = await client.GetStreamAsync(url);
        return await JsonSerializer.DeserializeAsync<Rootobject>(stream);
    }
}
