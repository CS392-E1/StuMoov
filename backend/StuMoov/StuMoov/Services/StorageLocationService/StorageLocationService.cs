using StuMoov.Dao; // Import Data Access Object (DAO) layer for accessing storage location data
using StuMoov.Models.StorageLocationModel; // Import the response model for API responses

namespace StuMoov.Services.StorageLocationService
{
    // Service class responsible for retrieving storage location data
    public class StorageLocationService
    {
        private readonly StorageLocationDao _storageLocationDao; // DAO instance for data retrieval

        // Constructor to inject StorageLocationDao dependency
        public StorageLocationService(StorageLocationDao storageLocationDao)
        {
            this._storageLocationDao = storageLocationDao;
        }

        // Method to retrieve all storage locations and return a structured response
        public StorageLocationResponse GetAllLocations()
        {
            // Fetch the list of storage locations from the DAO
            List<StorageLocation>? locations = this._storageLocationDao.GetAll();

            // Return a structured response with status code, message, and retrieved data
            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", locations);
        }

        // Method to retrieve a storage location by its ID
        public StorageLocationResponse GetLocationById(string id)
        {
            // Fetch the storage location from the DAO
            StorageLocation? location = this._storageLocationDao.GetById(id);

            // Return appropriate response based on whether the location was found
            if (location == null)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            return new StorageLocationResponse(
                StatusCodes.Status200OK,
                "OK",
                new List<StorageLocation> { location }
            );
        }

        // Method to retrieve storage locations by user ID
        public StorageLocationResponse GetLocationsByUserId(string userId)
        {
            // Fetch locations by user ID from the DAO
            List<StorageLocation>? locations = this._storageLocationDao.GetByUserId(userId);

            // Return appropriate response
            if (locations == null || locations.Count == 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "No storage locations found for this user",
                    null
                );
            }

            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", locations);
        }

        // Method to create a new storage location
        public StorageLocationResponse CreateLocation(StorageLocation storageLocation)
        {
            // Validate the storageLocation object
            if (storageLocation == null)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid storage location data",
                    null
                );
            }

            // Create the storage location using the DAO
            string id = this._storageLocationDao.Create(storageLocation);

            // Return success response with the created location
            StorageLocation? createdLocation = this._storageLocationDao.GetById(id);
            return new StorageLocationResponse(
                StatusCodes.Status201Created,
                "Storage location created successfully",
                createdLocation != null ? new List<StorageLocation> { createdLocation } : null
            );
        }

        // Method to update an existing storage location
        public StorageLocationResponse UpdateLocation(string id, StorageLocation updatedStorageLocation)
        {
            // Validate input
            if (updatedStorageLocation == null)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid storage location data",
                    null
                );
            }

            // Check if location exists
            if (!this._storageLocationDao.Exists(id))
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            // Update the storage location
            bool success = this._storageLocationDao.Update(id, updatedStorageLocation);

            if (success)
            {
                // Return the updated location
                StorageLocation? location = this._storageLocationDao.GetById(id);
                return new StorageLocationResponse(
                    StatusCodes.Status200OK,
                    "Storage location updated successfully",
                    location != null ? new List<StorageLocation> { location } : null
                );
            }

            return new StorageLocationResponse(
                StatusCodes.Status500InternalServerError,
                "Failed to update storage location",
                null
            );
        }

        // Method to delete a storage location
        public StorageLocationResponse DeleteLocation(string id)
        {
            // Check if location exists
            if (!this._storageLocationDao.Exists(id))
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "Storage location not found",
                    null
                );
            }

            // Delete the storage location
            bool success = this._storageLocationDao.Delete(id);

            if (success)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status200OK,
                    "Storage location deleted successfully",
                    null
                );
            }

            return new StorageLocationResponse(
                StatusCodes.Status500InternalServerError,
                "Failed to delete storage location",
                null
            );
        }

        // Method to find nearby storage locations
        public StorageLocationResponse FindNearbyLocations(double lat, double lng, double radiusKm)
        {
            // Validate parameters
            if (radiusKm <= 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status400BadRequest,
                    "Radius must be a positive number",
                    null
                );
            }

            // Find nearby locations
            List<StorageLocation>? nearbyLocations = this._storageLocationDao.FindNearby(lat, lng, radiusKm);

            if (nearbyLocations == null || nearbyLocations.Count == 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "No storage locations found within the specified radius",
                    null
                );
            }

            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", nearbyLocations);
        }

        // Method to find storage locations based on dimensional requirements
        public StorageLocationResponse FindLocationsByDimensions(double requiredLength, double requiredWidth, double requiredHeight)
        {
            // Find locations with sufficient dimensions
            List<StorageLocation>? locations = this._storageLocationDao.FindByDimensions(requiredLength, requiredWidth, requiredHeight);

            if (locations == null || locations.Count == 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "No storage locations found with the required dimensions",
                    null
                );
            }

            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", locations);
        }

        // Method to find storage locations with sufficient volume capacity
        public StorageLocationResponse FindLocationsWithSufficientCapacity(double requiredVolume)
        {
            // Validate parameters
            if (requiredVolume <= 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status400BadRequest,
                    "Required volume must be a positive number",
                    null
                );
            }

            // Find locations with sufficient capacity
            List<StorageLocation>? locations = this._storageLocationDao.FindWithSufficientCapacity(requiredVolume);

            if (locations == null || locations.Count == 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "No storage locations found with sufficient capacity",
                    null
                );
            }

            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", locations);
        }

        // Method to find storage locations with price less than or equal to the specified price
        public StorageLocationResponse FindLocationsByPrice(double price)
        {
            // Validate parameters
            if (price <= 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status400BadRequest,
                    "Price must be a positive number",
                    null
                );
            }

            // Find locations with price less than or equal to the specified price
            List<StorageLocation>? locations = this._storageLocationDao.FindWithPrice(price);

            if (locations == null || locations.Count == 0)
            {
                return new StorageLocationResponse(
                    StatusCodes.Status404NotFound,
                    "No storage locations found within the specified price range",
                    null
                );
            }

            return new StorageLocationResponse(StatusCodes.Status200OK, "OK", locations);
        }

        // Method to get the total count of storage locations
        public int GetLocationCount()
        {
            return this._storageLocationDao.Count();
        }

        // Method to check if a storage location exists by ID
        public bool LocationExists(string id)
        {
            return this._storageLocationDao.Exists(id);
        }
    }
}