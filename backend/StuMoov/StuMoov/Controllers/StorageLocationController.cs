using StuMoov.Dao; // Import Data Access Object layer
using StuMoov.Models.StorageLocationModel; // Import model for storage location response
using StuMoov.Services.StorageLocationService; // Import service layer for handling storage locations
using Microsoft.AspNetCore.Mvc; // Import ASP.NET Core MVC framework

namespace StuMoov.Controllers
{
    [ApiController] // Mark this class as an API controller
    [Route("api/[controller]")] // Define route pattern for the controller
    public class StorageLocationController : ControllerBase
    {
        private readonly StorageLocationDao storageLocationDao; // DAO for accessing storage location data

        // Constructor to inject StorageLocationDao dependency
        public StorageLocationController(StorageLocationDao storageLocationDao)
        {
            this.storageLocationDao = storageLocationDao;
        }

        [HttpGet] // Define an HTTP GET endpoint to retrieve all storage locations
        public StorageLocationResponse GetAllStorageLocation()
        {
            // Create an instance of GetStorageLocationService with DAO dependency
            GetStorageLocationService getStorageLocationService = new GetStorageLocationService(storageLocationDao);

            // Call service method to fetch all storage locations and return response
            return getStorageLocationService.GetAllLocations();
        }
    }
}
