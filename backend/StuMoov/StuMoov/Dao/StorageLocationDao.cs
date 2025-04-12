namespace StuMoov.Dao;
using StuMoov.Models.StorageLocationModel;

public class StorageLocationDao
{
    private Dictionary<Guid, StorageLocation> storageLocations;

    // Constructor to initialize the database reference with a map
    public StorageLocationDao()
    {
        // Initialize the dictionary
        storageLocations = new Dictionary<Guid, StorageLocation>();

        // Add some initial mock data
        StorageLocation locationA = new StorageLocation(
            Guid.NewGuid(),
            "Storage A",
            "A small storage location",
            -33.860664,
            151.208138,
            10.0,
            5.0,
            3.0,
            150.0,
            120.0
        );

        StorageLocation locationB = new StorageLocation(
            Guid.NewGuid(),
            "Storage B",
            "A medium storage location",
            -33.87664,
            151.218138,
            12.0,
            6.0,
            4.0,
            200.0,
            180.0
        );

        StorageLocation locationC = new StorageLocation(
            Guid.NewGuid(),
            "Storage C",
            "A large storage location",
            -33.870664,
            151.198138,
            15.0,
            8.0,
            5.0,
            600.0,
            450.0
        );

        // Using GUIDs as keys for the dictionary
        storageLocations.Add(Guid.NewGuid(), locationA);
        storageLocations.Add(Guid.NewGuid(), locationB);
        storageLocations.Add(Guid.NewGuid(), locationC);

    }

    // Method to retrieve all storage locations
    public List<StorageLocation>? GetAll()
    {
        return storageLocations.Values.ToList();
    }

    // Get a specific storage location by ID
    public StorageLocation? GetById(Guid id)
    {
        if (storageLocations.TryGetValue(id, out var location))
        {
            return location;
        }
        return null;
    }

    // Get storage locations by lender's user ID
    public List<StorageLocation>? GetByUserId(Guid userId)
    {
        return storageLocations.Values
            .Where(loc => loc.UserId == userId)
            .ToList();
    }

    // Create a new storage location
    public Guid Create(StorageLocation storageLocation)
    {
        // Generate a new GUID for the location
        Guid id = Guid.NewGuid();

        // Add to the dictionary
        storageLocations.Add(id, storageLocation);

        return id;
    }

    // Update an existing storage location
    public bool Update(Guid id, StorageLocation updatedStorageLocation)
    {
        if (!storageLocations.ContainsKey(id))
        {
            return false;
        }

        storageLocations[id] = updatedStorageLocation;
        return true;
    }

    // Delete a storage location by ID
    public bool Delete(Guid id)
    {
        return storageLocations.Remove(id);
    }

    // Find storage locations within a certain geographic radius
    public List<StorageLocation>? FindNearby(double lat, double lng, double radiusKm)
    {
        // Implementation of the Haversine formula to calculate distance between two points on Earth
        List<StorageLocation> nearbyLocations = new List<StorageLocation>();

        foreach (var location in storageLocations.Values)
        {
            double distance = CalculateDistance(lat, lng, location.Lat, location.Lng);
            if (distance <= radiusKm)
            {
                nearbyLocations.Add(location);
            }
        }

        return nearbyLocations;
    }

    // Helper method to calculate distance using Haversine formula
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Earth's radius in kilometers
        const double EarthRadius = 6371;

        // Convert degrees to radians
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        // Haversine formula
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = EarthRadius * c;

        return distance;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // Find storage locations based on dimensional requirements
    // Use -1 for any dimension that doesn't have a requirement
    public List<StorageLocation>? FindByDimensions(double requiredLength, double requiredWidth, double requiredHeight)
    {
        return storageLocations.Values
            .Where(loc =>
                // Only include dimension in filter if requirement is specified (not -1)
                (requiredLength == -1 || loc.StorageLength >= requiredLength) &&
                (requiredWidth == -1 || loc.StorageWidth >= requiredWidth) &&
                (requiredHeight == -1 || loc.StorageHeight >= requiredHeight)
            )
            .ToList();
    }

    // Find storage locations with sufficient volume
    public List<StorageLocation>? FindWithSufficientCapacity(double requiredVolume)
    {
        return storageLocations.Values
            .Where(loc => loc.StorageVolumeTotal >= requiredVolume)
            .ToList();
    }

    // Find storage locations with price less than or equal to the specified price
    public List<StorageLocation>? FindWithPrice(double price)
    {
        return storageLocations.Values
            .Where(loc => loc.Price <= price)
            .ToList();
    }

    // Count total number of storage locations
    public int Count()
    {
        return storageLocations.Count;
    }

    // Check if a storage location exists by ID
    public bool Exists(Guid id)
    {
        return storageLocations.ContainsKey(id);
    }
}