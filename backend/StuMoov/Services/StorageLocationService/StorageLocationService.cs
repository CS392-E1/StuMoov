/**
 * StorageLocationService.cs
 *
 * Manages operations related to storage locations in the StuMoov application.
 * Provides functionality to retrieve, create, update, delete, and query storage locations
 * based on various criteria such as location, dimensions, capacity, and price.
 * Integrates with the StorageLocationDao for data access.
 */

using System.ComponentModel.DataAnnotations;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.StorageLocationModel;

namespace StuMoov.Services.StorageLocationService
{
    /// <summary>
    /// Service responsible for managing storage location operations, including retrieval,
    /// creation, updates, deletion, and advanced querying based on location and attributes.
    /// </summary>
    public class StorageLocationService
    {
        /// <summary>
        /// Data access object for storage location-related database operations.
        /// </summary>
        [Required]
        private readonly StorageLocationDao _storageLocationDao;

        /// <summary>
        /// Initializes a new instance of the StorageLocationService with the required dependency.
        /// </summary>
        /// <param name="storageLocationDao">DAO for storage location operations.</param>
        public StorageLocationService(StorageLocationDao storageLocationDao)
        {
            this._storageLocationDao = storageLocationDao;
        }

        /// <summary>
        /// Retrieves all storage locations from the database.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of storage locations.</returns>
        public async Task<Response> GetAllLocationsAsync()
        {
            // Fetch the list of storage locations from the DAO
            List<StorageLocation> locations = await this._storageLocationDao.GetAllAsync();
            // Return a structured response with status code, message, and retrieved data
            return new Response(StatusCodes.Status200OK, "OK", locations);
        }

        /// <summary>
        /// Retrieves a storage location by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the storage location.</param>
        /// <returns>A Response object with the status, message, and storage location data.</returns>
        public async Task<Response> GetLocationByIdAsync(Guid id)
        {
            // Fetch the storage location from the DAO
            StorageLocation? location = await this._storageLocationDao.GetByIdAsync(id);
            // Return appropriate response based on whether the location was found
            if (location == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                new List<StorageLocation> { location }
            );
        }

        /// <summary>
        /// Retrieves all storage locations associated with a specific lender.
        /// </summary>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <returns>A Response object with the status, message, and list of storage locations.</returns>
        public async Task<Response> GetLocationsByUserIdAsync(Guid lenderId)
        {
            // Fetch locations by user ID from the DAO
            List<StorageLocation> locations = await this._storageLocationDao.GetByLenderIdAsync(lenderId);

            // Return appropriate response
            if (locations == null || locations.Count == 0)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "No storage locations found for this user",
                    null
                );
            }

            return new Response(StatusCodes.Status200OK, "OK", locations);
        }

        /// <summary>
        /// Creates a new storage location in the database.
        /// </summary>
        /// <param name="storageLocation">The storage location data to create.</param>
        /// <returns>A Response object with the status, message, and created storage location.</returns>
        public async Task<Response> CreateLocationAsync(StorageLocation storageLocation)
        {
            // Validate the storageLocation object
            if (storageLocation == null)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid storage location data",
                    null
                );
            }

            // Create the storage location using the DAO
            Guid id = await this._storageLocationDao.CreateAsync(storageLocation);

            // Return success response with the created location
            StorageLocation? createdLocation = await this._storageLocationDao.GetByIdAsync(id);
            return new Response(
                StatusCodes.Status201Created,
                "Storage location created successfully",
                createdLocation != null ? new List<StorageLocation> { createdLocation } : null
            );
        }

        /// <summary>
        /// Updates an existing storage location with new data.
        /// </summary>
        /// <param name="id">The ID of the storage location to update.</param>
        /// <param name="updatedStorageLocation">The updated storage location data.</param>
        /// <returns>A Response object with the status, message, and updated storage location.</returns>
        public async Task<Response> UpdateLocationAsync(Guid id, StorageLocation updatedStorageLocation)
        {
            // Validate input
            if (updatedStorageLocation == null)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid storage location data",
                    null
                );
            }

            // Check if location exists
            if (!await this._storageLocationDao.ExistsAsync(id))
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            // Update the storage location
            bool success = await this._storageLocationDao.UpdateAsync(id, updatedStorageLocation);
            if (success)
            {
                StorageLocation? location = await this._storageLocationDao.GetByIdAsync(id);
                return new Response(
                    StatusCodes.Status200OK,
                    "Storage location updated successfully",
                    location != null ? new List<StorageLocation> { location } : null
                );
            }

            return new Response(
                StatusCodes.Status500InternalServerError,
                "Failed to update storage location",
                null
            );
        }

        /// <summary>
        /// Deletes a storage location from the database.
        /// </summary>
        /// <param name="id">The ID of the storage location to delete.</param>
        /// <returns>A Response object with the status and message.</returns>
        public async Task<Response> DeleteLocationAsync(Guid id)
        {
            // Check if location exists
            if (!await this._storageLocationDao.ExistsAsync(id))
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            // Delete the storage location
            bool success = await this._storageLocationDao.DeleteAsync(id);
            if (success)
            {
                return new Response(
                    StatusCodes.Status200OK,
                    "Storage location deleted successfully",
                    null
                );
            }

            return new Response(
                StatusCodes.Status500InternalServerError,
                "Failed to delete storage location",
                null
            );
        }

        /// <summary>
        /// Finds storage locations within a specified radius from a given latitude and longitude.
        /// </summary>
        /// <param name="lat">The latitude of the center point.</param>
        /// <param name="lng">The longitude of the center point.</param>
        /// <param name="radiusKm">The radius in kilometers to search within.</param>
        /// <returns>A Response object with the status, message, and list of nearby storage locations.</returns>
        public async Task<Response> FindNearbyLocationsAsync(double lat, double lng, double radiusKm)
        {
            // Validate parameters
            if (radiusKm <= 0)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Radius must be a positive number",
                    null
                );
            }

            // Find nearby locations
            List<StorageLocation> nearbyLocations = await this._storageLocationDao.FindNearbyAsync(lat, lng, radiusKm);

            if (nearbyLocations == null || nearbyLocations.Count == 0)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "No storage locations found within the specified radius",
                    null
                );
            }

            return new Response(StatusCodes.Status200OK, "OK", nearbyLocations);
        }

        /// <summary>
        /// Finds storage locations that meet specified dimensional requirements.
        /// </summary>
        /// <param name="requiredLength">The minimum required length.</param>
        /// <param name="requiredWidth">The minimum required width.</param>
        /// <param name="requiredHeight">The minimum required height.</param>
        /// <returns>A Response object with the status, message, and list of matching storage locations.</returns>
        public async Task<Response> FindLocationsByDimensionsAsync(double requiredLength, double requiredWidth, double requiredHeight)
        {
            // Find locations with sufficient dimensions
            List<StorageLocation> locations = await this._storageLocationDao.FindByDimensionsAsync(requiredLength, requiredWidth, requiredHeight);
            if (locations == null || locations.Count == 0)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "No storage locations found with the required dimensions",
                    null
                );
            }

            return new Response(StatusCodes.Status200OK, "OK", locations);
        }

        /// <summary>
        /// Finds storage locations with sufficient volume capacity.
        /// </summary>
        /// <param name="requiredVolume">The minimum required volume.</param>
        /// <returns>A Response object with the status, message, and list of matching storage locations.</returns>
        public async Task<Response> FindLocationsWithSufficientCapacityAsync(double requiredVolume)
        {
            // Validate parameters
            if (requiredVolume <= 0)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Required volume must be a positive number",
                    null
                );
            }

            // Find locations with sufficient capacity
            List<StorageLocation> locations = await this._storageLocationDao.FindWithSufficientCapacityAsync(requiredVolume);
            if (locations == null || locations.Count == 0)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "No storage locations found with sufficient capacity",
                    null
                );
            }

            return new Response(StatusCodes.Status200OK, "OK", locations);
        }

        /// <summary>
        /// Finds storage locations with a price less than or equal to the specified amount.
        /// </summary>
        /// <param name="price">The maximum price for the storage locations.</param>
        /// <returns>A Response object with the status, message, and list of matching storage locations.</returns>
        public async Task<Response> FindLocationsByPriceAsync(double price)
        {
            // Validate parameters
            if (price <= 0)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Price must be a positive number",
                    null
                );
            }

            // Find locations with price less than or equal to the specified price
            List<StorageLocation> locations = await this._storageLocationDao.FindWithPriceAsync(price);
            if (locations == null || locations.Count == 0)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "No storage locations found within the specified price range",
                    null
                );
            }

            return new Response(StatusCodes.Status200OK, "OK", locations);
        }

        /// <summary>
        /// Retrieves the total count of storage locations in the database.
        /// </summary>
        /// <returns>The total number of storage locations.</returns>
        public async Task<int> GetLocationCountAsync()
        {
            return await this._storageLocationDao.CountAsync();
        }

        /// <summary>
        /// Checks if a storage location exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the storage location.</param>
        /// <returns>True if the storage location exists, false otherwise.</returns>
        public async Task<bool> LocationExistsAsync(Guid id)
        {
            return await this._storageLocationDao.ExistsAsync(id);
        }
    }
}