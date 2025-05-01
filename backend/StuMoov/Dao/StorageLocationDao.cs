/**
 * StorageLocationDao.cs
 *
 * Handles data access operations for StorageLocation entities including retrieval,
 * creation, update, and deletion, as well as various search filters such as
 * geographic proximity and dimensional requirements. Uses Entity Framework Core
 * for database interactions.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.StorageLocationModel;

namespace StuMoov.Dao
{
    public class StorageLocationDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for storage locations

        /// <summary>
        /// Initialize the StorageLocationDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for storage operations</param>
        public StorageLocationDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all storage locations from the database.
        /// </summary>
        /// <returns>List of all StorageLocation entities</returns>
        public async Task<List<StorageLocation>> GetAllAsync()
        {
            return await _dbContext.StorageLocations.ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific storage location by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the storage location</param>
        /// <returns>The StorageLocation entity if found; otherwise null</returns>
        public async Task<StorageLocation?> GetByIdAsync(Guid id)
        {
            return await _dbContext.StorageLocations.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all storage locations associated with a specific lender.
        /// </summary>
        /// <param name="lenderId">The GUID of the lender</param>
        /// <returns>List of StorageLocation entities for the lender</returns>
        public async Task<List<StorageLocation>> GetByLenderIdAsync(Guid lenderId)
        {
            return await _dbContext.StorageLocations
                .Where(loc => loc.LenderId == lenderId)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new storage location record in the database.
        /// </summary>
        /// <param name="storageLocation">The StorageLocation entity to create</param>
        /// <returns>The GUID of the newly created storage location</returns>
        public async Task<Guid> CreateAsync(StorageLocation storageLocation)
        {
            await _dbContext.StorageLocations.AddAsync(storageLocation);
            await _dbContext.SaveChangesAsync();
            return storageLocation.Id;
        }

        /// <summary>
        /// Updates an existing storage location's data.
        /// </summary>
        /// <param name="id">The GUID of the storage location to update</param>
        /// <param name="updatedStorageLocation">StorageLocation model containing updated values</param>
        /// <returns>True if update succeeded; otherwise false</returns>
        public async Task<bool> UpdateAsync(Guid id, StorageLocation updatedStorageLocation)
        {
            var existingLocation = await _dbContext.StorageLocations.FindAsync(id);
            if (existingLocation == null)
            {
                return false;
            }

            _dbContext.Entry(existingLocation).State = EntityState.Detached;
            _dbContext.StorageLocations.Update(updatedStorageLocation);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Deletes a storage location by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the storage location to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
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

        /// <summary>
        /// Finds storage locations within a certain geographic radius (in kilometers).
        /// </summary>
        /// <param name="lat">Latitude of the center point</param>
        /// <param name="lng">Longitude of the center point</param>
        /// <param name="radiusKm">Search radius in kilometers</param>
        /// <returns>List of StorageLocation entities within the radius</returns>
        public async Task<List<StorageLocation>> FindNearbyAsync(double lat, double lng, double radiusKm)
        {
            var allLocations = await _dbContext.StorageLocations.ToListAsync();
            var nearbyLocations = new List<StorageLocation>();
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

        /// <summary>
        /// Calculates the distance between two geographic points using the Haversine formula.
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadius = 6371; // Earth's radius in kilometers
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadius * c;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Finds storage locations that meet minimum dimensional requirements.
        /// Use -1 for any dimension that should be ignored.
        /// </summary>
        public async Task<List<StorageLocation>> FindByDimensionsAsync(double requiredLength, double requiredWidth, double requiredHeight)
        {
            var query = _dbContext.StorageLocations.AsQueryable();
            if (requiredLength != -1)
                query = query.Where(loc => loc.StorageLength >= requiredLength);
            if (requiredWidth != -1)
                query = query.Where(loc => loc.StorageWidth >= requiredWidth);
            if (requiredHeight != -1)
                query = query.Where(loc => loc.StorageHeight >= requiredHeight);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Finds storage locations with total volume greater than or equal to the required volume.
        /// </summary>
        public async Task<List<StorageLocation>> FindWithSufficientCapacityAsync(double requiredVolume)
        {
            return await _dbContext.StorageLocations
                .Where(loc => loc.StorageVolumeTotal >= requiredVolume)
                .ToListAsync();
        }

        /// <summary>
        /// Finds storage locations with price less than or equal to the specified price.
        /// </summary>
        public async Task<List<StorageLocation>> FindWithPriceAsync(double price)
        {
            return await _dbContext.StorageLocations
                .Where(loc => loc.Price <= price)
                .ToListAsync();
        }

        /// <summary>
        /// Counts the total number of storage locations in the database.
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _dbContext.StorageLocations.CountAsync();
        }

        /// <summary>
        /// Checks if a storage location exists by its unique identifier.
        /// </summary>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.StorageLocations.AnyAsync(loc => loc.Id == id);
        }
    }
}
