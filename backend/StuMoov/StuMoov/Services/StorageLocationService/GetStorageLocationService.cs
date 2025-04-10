using StuMoov.Dao; // Import Data Access Object (DAO) layer for accessing storage location data
using StuMoov.Models.StorageLocationModel; // Import the response model for API responses

namespace StuMoov.Services.StorageLocationService
{
    // Service class responsible for retrieving storage location data
    public class GetStorageLocationService
    {
        private readonly StorageLocationDao _storageLocationDao; // DAO instance for data retrieval

        // Constructor to inject StorageLocationDao dependency
        public GetStorageLocationService(StorageLocationDao storageLocationDao)
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
    }
}