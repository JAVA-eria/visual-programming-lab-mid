using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

abstract class User
{
    public int UserID { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }

    public virtual void Register() => Console.WriteLine($"{Name} Registered");
    public virtual void DisplayProfile() => Console.WriteLine($"User: {Name}, Phone: {PhoneNumber}");
}

class Rider : User
{
    public List<Trip> RideHistory { get; private set; } = new List<Trip>();

    public override void Register()
    {
        base.Register();
        Console.WriteLine("Rider profile created.");
    }

    public void RequestRide(RideSharingSystem system, string startLocation, string destination)
    {
        Trip trip = system.RequestRide(this, startLocation, destination);
        if (trip != null)
            RideHistory.Add(trip);
    }

    public void ViewRideHistory()
    {
        Console.WriteLine("Ride History:");
        foreach (var trip in RideHistory)
            trip.DisplayTripDetails();
    }

    public override void DisplayProfile()
    {
        base.DisplayProfile();
        Console.WriteLine("Rider specific information can be displayed here.");
    }
}

class Driver : User
{
    public int DriverID { get; set; }
    public string VehicleDetails { get; set; }
    public bool IsAvailable { get; private set; } = true;
    public List<Trip> TripHistory { get; private set; } = new List<Trip>();

    public override void Register()
    {
        base.Register();
        Console.WriteLine("Driver profile created.");
    }

    public void AcceptRide(Trip trip)
    {
        trip.DriverName = Name;
        trip.Status = "Accepted";
        IsAvailable = false;
        TripHistory.Add(trip);
        Console.WriteLine($"{Name} accepted the ride.");
    }

    public void CompleteTrip(Trip trip)
    {
        if (trip.Status != "In Progress")
        {
            Console.WriteLine("Cannot complete trip. Trip is not in progress.");
            return;
        }

        trip.EndTrip();
        ToggleAvailability();
        Console.WriteLine($"Trip completed by {Name}.");
    }

    public void ToggleAvailability() => IsAvailable = !IsAvailable;

    public override void DisplayProfile()
    {
        base.DisplayProfile();
        Console.WriteLine($"Vehicle: {VehicleDetails}, Available: {IsAvailable}");
    }
}

class Trip
{
    public int TripID { get; set; }
    public string RiderName { get; set; }
    public string DriverName { get; set; }
    public string StartLocation { get; set; }
    public string Destination { get; set; }
    public double Fare { get; private set; }
    public string Status { get; set; } = "Requested";

    public void CalculateFare() => Fare = new Random().Next(10, 50); 

    public void EndTrip() => Status = "Completed";

    public void DisplayTripDetails()
    {
        Console.WriteLine($"TripID: {TripID}, Rider: {RiderName}, Driver: {DriverName}, Start: {StartLocation}, Destination: {Destination}, Fare: {Fare}, Status: {Status}");
    }
}

class RideSharingSystem
{
    private List<Rider> registeredRiders = new List<Rider>();
    private List<Driver> registeredDrivers = new List<Driver>();
    private List<Trip> availableTrips = new List<Trip>();
    private int tripCounter = 0;

    public void RegisterUser(User user)
    {
        if (user is Rider)
            registeredRiders.Add(user as Rider);
        else if (user is Driver)
            registeredDrivers.Add(user as Driver);

        user.Register();
    }

    public Trip RequestRide(Rider rider, string startLocation, string destination)
    {
        Driver availableDriver = FindAvailableDriver();
        if (availableDriver == null)
        {
            Console.WriteLine("No available drivers at the moment.");
            return null;
        }

        Trip trip = new Trip
        {
            TripID = ++tripCounter,
            RiderName = rider.Name,
            DriverName = availableDriver.Name,
            StartLocation = startLocation,
            Destination = destination
        };
        trip.CalculateFare();
        availableDriver.AcceptRide(trip);
        availableTrips.Add(trip);

        Console.WriteLine("Ride requested successfully.");
        return trip;
    }

    public void CompleteTrip(Trip trip)
    {
        trip.EndTrip();
        Driver driver = registeredDrivers.Find(d => d.Name == trip.DriverName);
        if (driver != null)
        {
            driver.CompleteTrip(trip);
        }
    }

    public void DisplayAllTrips()
    {
        Console.WriteLine("All Trips:");
        foreach (var trip in availableTrips)
            trip.DisplayTripDetails();
    }

    public List<Rider> GetRegisteredRiders() => registeredRiders;
    public List<Driver> GetRegisteredDrivers() => registeredDrivers;

    private Driver FindAvailableDriver()
    {
        return registeredDrivers.Find(d => d.IsAvailable);
    }

    public List<Trip> GetAvailableTrips() => availableTrips.FindAll(t => t.Status == "Requested");
}

class Program
{
    static void Main()
    {
        RideSharingSystem system = new RideSharingSystem();

        while (true)
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Register as Rider");
            Console.WriteLine("2. Register as Driver");
            Console.WriteLine("3. Request Ride");
            Console.WriteLine("4. Complete Trip");
            Console.WriteLine("5. View Ride History");
            Console.WriteLine("6. Display All Trips");
            Console.WriteLine("7. Accept Ride");
            Console.WriteLine("8. Exit");
            Console.Write("Enter your choice: ");

            int choice;
            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                continue;
            }

            switch (choice)
            {
                case 1:
                    Rider rider = new Rider();
                    Console.Write("Enter Rider Name: ");
                    rider.Name = Console.ReadLine();

                    Console.Write("Enter Phone Number (Format: 123-456-7890): ");
                    string phoneNumber = Console.ReadLine();
                    while (!IsValidPhoneNumber(phoneNumber))
                    {
                        Console.Write("Invalid format. Please enter a valid Phone Number (Format: 123-456-7890): ");
                        phoneNumber = Console.ReadLine();
                    }
                    rider.PhoneNumber = phoneNumber;
                    rider.UserID = system.GetRegisteredRiders().Count + 1; 
                    system.RegisterUser(rider);
                    break;

                case 2:
                    Driver driver = new Driver();
                    Console.Write("Enter Driver Name: ");
                    driver.Name = Console.ReadLine();

                    Console.Write("Enter Phone Number (Format: 123-456-7890): ");
                    phoneNumber = Console.ReadLine();
                    while (!IsValidPhoneNumber(phoneNumber))
                    {
                        Console.Write("Invalid format. Please enter a valid Phone Number (Format: 123-456-7890): ");
                        phoneNumber = Console.ReadLine();
                    }
                    driver.PhoneNumber = phoneNumber;

                    Console.Write("Enter Vehicle Details: ");
                    driver.VehicleDetails = Console.ReadLine();
                    driver.UserID = system.GetRegisteredDrivers().Count + 1;
                    driver.DriverID = driver.UserID; 
                    system.RegisterUser(driver);
                    break;

                case 3:
                    Console.Write("Enter Start Location: ");
                    string start = Console.ReadLine();
                    Console.Write("Enter Destination: ");
                    string destination = Console.ReadLine();
                    if (system.GetRegisteredRiders().Count > 0)
                    {
                        Rider currentRider = system.GetRegisteredRiders()[0]; 
                        currentRider.RequestRide(system, start, destination);
                    }
                    else
                    {
                        Console.WriteLine("No riders registered.");
                    }
                    break;

                case 4:
                    if (system.GetRegisteredDrivers().Count > 0)
                    {
                        Driver currentDriver = system.GetRegisteredDrivers()[0]; 
                        if (currentDriver.TripHistory.Count > 0)
                        {
                            Trip ongoingTrip = currentDriver.TripHistory[0];
                            system.CompleteTrip(ongoingTrip);
                        }
                        else
                        {
                            Console.WriteLine("No ongoing trips.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No drivers registered.");
                    }
                    break;

                case 5:
                    if (system.GetRegisteredRiders().Count > 0)
                    {
                        Rider currentRider = system.GetRegisteredRiders()[0]; 
                        currentRider.ViewRideHistory();
                    }
                    else
                    {
                        Console.WriteLine("No riders registered.");
                    }
                    break;

                case 6:
                    system.DisplayAllTrips();
                    break;

                case 7:
                    if (system.GetRegisteredDrivers().Count > 0)
                    {
                        Driver currentDriver = system.GetRegisteredDrivers()[0]; 
                        List<Trip> availableTrips = system.GetAvailableTrips();
                        if (availableTrips.Count > 0)
                        {
                            Console.WriteLine("Available Trips:");
                            foreach (var trip in availableTrips)
                            {
                                trip.DisplayTripDetails();
                            }

                            Console.Write("Enter the Trip ID to accept: ");
                            if (int.TryParse(Console.ReadLine(), out int tripID))
                            {
                                Trip tripToAccept = availableTrips.Find(t => t.TripID == tripID);
                                if (tripToAccept != null)
                                {
                                    currentDriver.AcceptRide(tripToAccept);
                                }
                                else
                                {
                                    Console.WriteLine("Trip not found.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid input.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No available trips.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No drivers registered.");
                    }
                    break;

                case 8:
                    return;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    static bool IsValidPhoneNumber(string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, @"^\d{3}-\d{3}-\d{4}$");
    }
}


