/**
 * StorageLocationController.cs
 * 
 * Handles storage location management including CRUD operations and search functionality.
 * Provides endpoints for creating, retrieving, updating, and deleting storage locations,
 * as well as various search capabilities based on location, dimensions, price, and capacity.
 */

using StuMoov.Dao;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Services.StorageLocationService;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using StuMoov.Models.UserModel;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/storage")]
    public class StorageLocationController : ControllerBase
    {
        private readonly StorageLocationService _storageLocationService;  // Service for storage location operations
        private readonly StorageLocationDao _storageLocationDao;          // Data access object for storage locations
        private readonly UserDao _userDao;                               // Data access object for user information

        /// <summary>
        /// Initialize the StorageLocationController with required dependencies
        /// </summary>
        /// <param name="storageLocationDao">Data access object for storage location operations</param>
        /// <param name="userDao">Data access object for user operations</param>
        public StorageLocationController(StorageLocationDao storageLocationDao, UserDao userDao)
        {
            this._storageLocationDao = storageLocationDao;
            this._storageLocationService = new StorageLocationService(storageLocationDao);
            this._userDao = userDao;
        }

        /// <summary>
        /// Retrieves all storage locations in the system
        /// </summary>
        /// <returns>List of all storage locations</returns>
        /// <route>GET: api/storage</route>
        [HttpGet]
        public async Task<ActionResult<Response>> GetAllStorageLocations()
        {
            // Get all storage locations from the service
            Response response = await _storageLocationService.GetAllLocationsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a specific storage location by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the storage location</param>
        /// <returns>Details of the requested storage location</returns>
        /// <route>GET: api/storage/{id}</route>
        [HttpGet("{id}")]
        public async Task<ActionResult<Response>> GetStorageLocationById(Guid id)
        {
            // Get the storage location by its ID
            Response response = await _storageLocationService.GetLocationByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all storage locations owned by a specific user (lender)
        /// </summary>
        /// <param name="userId">The unique identifier of the user (lender)</param>
        /// <returns>List of storage locations owned by the specified user</returns>
        /// <route>GET: api/storage/user/{userId}</route>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Response>> GetStorageLocationsByUserId(Guid userId)
        {
            // Get all storage locations associated with the specified user
            Response response = await _storageLocationService.GetLocationsByUserIdAsync(userId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Creates a new storage location
        /// </summary>
        /// <param name="request">Storage location creation details</param>
        /// <returns>Created storage location details</returns>
        /// <route>POST: api/storage</route>
        [HttpPost]
        public async Task<ActionResult<Response>> CreateStorageLocation([FromBody] CreateStorageLocationRequest request)
        {
            // Validate request model
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request data.", ModelState));
            }

            // Verify the lender exists and is actually a lender
            User? lenderUser = await _userDao.GetUserByIdAsync(request.LenderId);
            if (lenderUser == null || !(lenderUser is Lender lender))
            {
                return NotFound(new Response(StatusCodes.Status404NotFound, "Lender not found or user is not a lender.", null));
            }

            // Calculate the storage volume based on dimensions
            double length = request.StorageLength ?? 0;
            double width = request.StorageWidth ?? 0;
            double height = request.StorageHeight ?? 0;
            double volume = length * width * height;
            double price = request.Price ?? 0;

            // Create a new storage location object
            StorageLocation newStorageLocation = new StorageLocation(
                lender: lender,
                name: request.Name,
                description: request.Description,
                address: request.Address,
                lat: request.Lat,
                lng: request.Lng,
                storageLength: length,
                storageWidth: width,
                storageHeight: height,
                storageVolumeTotal: volume,
                price: price
            );

            // Create the storage location in the database
            Response response = await _storageLocationService.CreateLocationAsync(newStorageLocation);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Updates an existing storage location
        /// </summary>
        /// <param name="id">The unique identifier of the storage location to update</param>
        /// <param name="storageLocation">Updated storage location details</param>
        /// <returns>Result of the update operation</returns>
        /// <route>PUT: api/storage/{id}</route>
        [HttpPut("{id}")]
        public async Task<ActionResult<Response>> UpdateStorageLocation(Guid id, [FromBody] StorageLocation storageLocation)
        {
            // Validate request model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the storage location
            Response response = await _storageLocationService.UpdateLocationAsync(id, storageLocation);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Deletes a storage location
        /// </summary>
        /// <param name="id">The unique identifier of the storage location to delete</param>
        /// <returns>Result of the delete operation</returns>
        /// <route>DELETE: api/storage/{id}</route>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response>> DeleteStorageLocation(Guid id)
        {
            // Delete the storage location
            Response response = await _storageLocationService.DeleteLocationAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Finds storage locations within a specified radius of a geographic coordinate
        /// </summary>
        /// <param name="lat">Latitude of the center point</param>
        /// <param name="lng">Longitude of the center point</param>
        /// <param name="radiusKm">Search radius in kilometers</param>
        /// <returns>List of storage locations within the specified radius</returns>
        /// <route>GET: api/storage/nearby</route>
        [HttpGet("nearby")]
        public async Task<ActionResult<Response>> FindNearbyLocations(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm)
        {
            // Find locations within the specified radius of the given coordinates
            Response response = await _storageLocationService.FindNearbyLocationsAsync(lat, lng, radiusKm);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Finds storage locations matching specified dimension requirements
        /// </summary>
        /// <param name="length">Minimum length requirement (-1 for no restriction)</param>
        /// <param name="width">Minimum width requirement (-1 for no restriction)</param>
        /// <param name="height">Minimum height requirement (-1 for no restriction)</param>
        /// <returns>List of storage locations matching the dimension criteria</returns>
        /// <route>GET: api/storage/dimensions</route>
        [HttpGet("dimensions")]
        public async Task<ActionResult<Response>> FindLocationsByDimensions(
            [FromQuery] double length = -1,
            [FromQuery] double width = -1,
            [FromQuery] double height = -1)
        {
            // Find locations that meet the dimension requirements
            Response response = await _storageLocationService.FindLocationsByDimensionsAsync(length, width, height);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Finds storage locations with sufficient capacity for a specified volume
        /// </summary>
        /// <param name="volume">Required storage volume in cubic units</param>
        /// <returns>List of storage locations with sufficient capacity</returns>
        /// <route>GET: api/storage/capacity</route>
        [HttpGet("capacity")]
        public async Task<ActionResult<Response>> FindLocationsWithSufficientCapacity([FromQuery] double volume)
        {
            // Find locations with enough capacity for the specified volume
            Response response = await _storageLocationService.FindLocationsWithSufficientCapacityAsync(volume);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Finds storage locations with prices at or below a specified maximum
        /// </summary>
        /// <param name="maxPrice">Maximum price per unit</param>
        /// <returns>List of storage locations within the price range</returns>
        /// <route>GET: api/storage/price</route>
        [HttpGet("price")]
        public async Task<ActionResult<Response>> FindLocationsByPrice([FromQuery] double maxPrice)
        {
            // Find locations within the specified price range
            Response response = await _storageLocationService.FindLocationsByPriceAsync(maxPrice);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Gets the total count of storage locations in the system
        /// </summary>
        /// <returns>Count of storage locations</returns>
        /// <route>GET: api/storage/count</route>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetLocationCount()
        {
            // Get the total number of storage locations
            int count = await _storageLocationService.GetLocationCountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Checks if a storage location with the specified ID exists
        /// </summary>
        /// <param name="id">The unique identifier of the storage location</param>
        /// <returns>HTTP 200 if exists, 404 if not found</returns>
        /// <route>HEAD: api/storage/{id}</route>
        [HttpHead("{id}")]
        public async Task<IActionResult> CheckLocationExists(Guid id)
        {
            // Check if the storage location exists
            bool exists = await _storageLocationService.LocationExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
    }
}

/**
 * Data transfer object for creating a new storage location
 */
public class CreateStorageLocationRequest
{
    public Guid LenderId { get; set; }                      // ID of the lender who owns this storage location
    public string Name { get; set; } = string.Empty;        // Name/title of the storage location
    public string Description { get; set; } = string.Empty; // Detailed description of the storage location
    public string Address { get; set; } = string.Empty;     // Physical address of the storage location
    public double Lat { get; set; }                         // Latitude coordinate
    public double Lng { get; set; }                         // Longitude coordinate
    public double? Price { get; set; }                      // Price per unit time (nullable)
    public double? StorageLength { get; set; }              // Length dimension (nullable)
    public double? StorageWidth { get; set; }               // Width dimension (nullable)
    public double? StorageHeight { get; set; }              // Height dimension (nullable)
}