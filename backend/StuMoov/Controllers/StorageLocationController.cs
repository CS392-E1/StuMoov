using StuMoov.Dao;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Services.StorageLocationService;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/storage")]
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
        public async Task<ActionResult<Response>> GetStorageLocationById(Guid id)
        {
            Response response = _storageLocationService.GetLocationById(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Response>> GetStorageLocationsByUserId(Guid userId)
        {
            Response response = _storageLocationService.GetLocationsByUserId(userId);
            return StatusCode(response.Status, response);
        }

        // POST: api/StorageLocation
        [HttpPost]
        public async Task<ActionResult<Response>> CreateStorageLocation([FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = _storageLocationService.CreateLocation(storageLocation);
            return StatusCode(response.Status, response);
        }

        // PUT: api/StorageLocation/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Response>> UpdateStorageLocation(Guid id, [FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = _storageLocationService.UpdateLocation(id, storageLocation);
            return StatusCode(response.Status, response);
        }

        // DELETE: api/StorageLocation/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response>> DeleteStorageLocation(Guid id)
        {
            Response response = _storageLocationService.DeleteLocation(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/nearby
        [HttpGet("nearby")]
        public async Task<ActionResult<Response>> FindNearbyLocations(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm)
        {
            Response response = _storageLocationService.FindNearbyLocations(lat, lng, radiusKm);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/dimensions
        [HttpGet("dimensions")]
        public async Task<ActionResult<Response>> FindLocationsByDimensions(
            [FromQuery] double length = -1,
            [FromQuery] double width = -1,
            [FromQuery] double height = -1)
        {
            Response response = _storageLocationService.FindLocationsByDimensions(length, width, height);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/capacity
        [HttpGet("capacity")]
        public async Task<ActionResult<Response>> FindLocationsWithSufficientCapacity([FromQuery] double volume)
        {
            Response response = _storageLocationService.FindLocationsWithSufficientCapacity(volume);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/price
        [HttpGet("price")]
        public async Task<ActionResult<Response>> FindLocationsByPrice([FromQuery] double maxPrice)
        {
            Response response = _storageLocationService.FindLocationsByPrice(maxPrice);
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
