using StuMoov.Dao;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Services.StorageLocationService;
using Microsoft.AspNetCore.Mvc;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageLocationController : ControllerBase
    {
        private readonly StorageLocationService _storageLocationService;

        // Constructor to inject StorageLocationService dependency
        public StorageLocationController(StorageLocationDao storageLocationDao)
        {
            // Create StorageLocationService instance using injected DAO
            this._storageLocationService = new StorageLocationService(storageLocationDao);
        }

        // GET: api/StorageLocation
        [HttpGet]
        public async Task<ActionResult<StorageLocationResponse>> GetAllStorageLocations()
        {
            StorageLocationResponse response = await _storageLocationService.GetAllLocationsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StorageLocationResponse>> GetStorageLocationById(Guid id)
        {
            StorageLocationResponse response = await _storageLocationService.GetLocationByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<StorageLocationResponse>> GetStorageLocationsByUserId(Guid userId)
        {
            StorageLocationResponse response = await _storageLocationService.GetLocationsByUserIdAsync(userId);
            return StatusCode(response.Status, response);
        }

        // POST: api/StorageLocation
        [HttpPost]
        public async Task<ActionResult<StorageLocationResponse>> CreateStorageLocation([FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StorageLocationResponse response = await _storageLocationService.CreateLocationAsync(storageLocation);
            return StatusCode(response.Status, response);
        }

        // PUT: api/StorageLocation/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<StorageLocationResponse>> UpdateStorageLocation(Guid id, [FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StorageLocationResponse response = await _storageLocationService.UpdateLocationAsync(id, storageLocation);
            return StatusCode(response.Status, response);
        }

        // DELETE: api/StorageLocation/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<StorageLocationResponse>> DeleteStorageLocation(Guid id)
        {
            StorageLocationResponse response = await _storageLocationService.DeleteLocationAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/nearby
        [HttpGet("nearby")]
        public async Task<ActionResult<StorageLocationResponse>> FindNearbyLocations(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm)
        {
            StorageLocationResponse response = await _storageLocationService.FindNearbyLocationsAsync(lat, lng, radiusKm);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/dimensions
        [HttpGet("dimensions")]
        public async Task<ActionResult<StorageLocationResponse>> FindLocationsByDimensions(
            [FromQuery] double length = -1,
            [FromQuery] double width = -1,
            [FromQuery] double height = -1)
        {
            StorageLocationResponse response = await _storageLocationService.FindLocationsByDimensionsAsync(length, width, height);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/capacity
        [HttpGet("capacity")]
        public async Task<ActionResult<StorageLocationResponse>> FindLocationsWithSufficientCapacity([FromQuery] double volume)
        {
            StorageLocationResponse response = await _storageLocationService.FindLocationsWithSufficientCapacityAsync(volume);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/price
        [HttpGet("price")]
        public async Task<ActionResult<StorageLocationResponse>> FindLocationsByPrice([FromQuery] double maxPrice)
        {
            StorageLocationResponse response = await _storageLocationService.FindLocationsByPriceAsync(maxPrice);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetLocationCount()
        {
            int count = await _storageLocationService.GetLocationCountAsync();
            return Ok(count);
        }

        // HEAD: api/StorageLocation/{id}
        [HttpHead("{id}")]
        public async Task<IActionResult> CheckLocationExists(Guid id)
        {
            bool exists = await _storageLocationService.LocationExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
    }
}
