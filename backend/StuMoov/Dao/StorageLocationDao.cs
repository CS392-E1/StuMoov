namespace StuMoov.Dao;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.StorageLocationModel;

public class StorageLocationDao
{
    [Required]
    private readonly AppDbContext _dbContext;

    public StorageLocationDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // No need for InitAsync method anymore as we'll use dbContext directly

    // Method to retrieve all storage locations
    public async Task<List<StorageLocation>> GetAllAsync()
    {
        return await _dbContext.StorageLocations.ToListAsync();
    }

    // Get a specific storage location by ID
    public async Task<StorageLocation?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StorageLocations.FindAsync(id);
    }

    // Get storage locations by lender's user ID
    public async Task<List<StorageLocation>> GetByLenderIdAsync(Guid lenderId)
    {
        return await _dbContext.StorageLocations
            .Where(loc => loc.LenderId == lenderId)
            .ToListAsync();
    }

    // Create a new storage location
    public async Task<Guid> CreateAsync(StorageLocation storageLocation)
    {
        // Add to the database
        await _dbContext.StorageLocations.AddAsync(storageLocation);
        await _dbContext.SaveChangesAsync();

        return storageLocation.Id;
    }

    // Update an existing storage location
    public async Task<bool> UpdateAsync(Guid id, StorageLocation updatedStorageLocation)
    {
        var existingLocation = await _dbContext.StorageLocations.FindAsync(id);
        if (existingLocation == null)
        {
            return false;
        }

        // Detach the existing entity and attach the updated one
        _dbContext.Entry(existingLocation).State = EntityState.Detached;
        _dbContext.StorageLocations.Update(updatedStorageLocation);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Delete a storage location by ID
    public async Task<bool> DeleteAsync(Guid id)
    {
        var location = await _dbContext.StorageLocations.FindAsync(id);
        if (location == null)
        {
            return false;
        }

        _dbContext.StorageLocations.Remove(location);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Find storage locations within a certain geographic radius
    public async Task<List<StorageLocation>> FindNearbyAsync(double lat, double lng, double radiusKm)
    {
        // Get all locations first
        var allLocations = await _dbContext.StorageLocations.ToListAsync();

        // Then filter by distance
        List<StorageLocation> nearbyLocations = new List<StorageLocation>();
        foreach (var location in allLocations)
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
    public async Task<List<StorageLocation>> FindByDimensionsAsync(double requiredLength, double requiredWidth, double requiredHeight)
    {
        var query = _dbContext.StorageLocations.AsQueryable();

        // Only include dimension in filter if requirement is specified (not -1)
        if (requiredLength != -1)
            query = query.Where(loc => loc.StorageLength >= requiredLength);

        if (requiredWidth != -1)
            query = query.Where(loc => loc.StorageWidth >= requiredWidth);

        if (requiredHeight != -1)
            query = query.Where(loc => loc.StorageHeight >= requiredHeight);

        return await query.ToListAsync();
    }

    // Find storage locations with sufficient volume
    public async Task<List<StorageLocation>> FindWithSufficientCapacityAsync(double requiredVolume)
    {
        return await _dbContext.StorageLocations
            .Where(loc => loc.StorageVolumeTotal >= requiredVolume)
            .ToListAsync();
    }

    // Find storage locations with price less than or equal to the specified price
    public async Task<List<StorageLocation>> FindWithPriceAsync(double price)
    {
        return await _dbContext.StorageLocations
            .Where(loc => loc.Price <= price)
            .ToListAsync();
    }

    // Count total number of storage locations
    public async Task<int> CountAsync()
    {
        return await _dbContext.StorageLocations.CountAsync();
    }

    // Check if a storage location exists by ID
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.StorageLocations.AnyAsync(loc => loc.Id == id);
    }
}