
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
        public ActionResult<StorageLocationResponse> GetAllStorageLocations()
        {
            StorageLocationResponse response = _storageLocationService.GetAllLocations();
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/{id}
        [HttpGet("{id}")]
        public ActionResult<StorageLocationResponse> GetStorageLocationById(Guid id)
        {
            StorageLocationResponse response = _storageLocationService.GetLocationById(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/user/{userId}
        [HttpGet("user/{userId}")]
        public ActionResult<StorageLocationResponse> GetStorageLocationsByUserId(Guid userId)
        {
            StorageLocationResponse response = _storageLocationService.GetLocationsByUserId(userId);
            return StatusCode(response.Status, response);
        }

        // POST: api/StorageLocation
        [HttpPost]
        public ActionResult<StorageLocationResponse> CreateStorageLocation([FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StorageLocationResponse response = _storageLocationService.CreateLocation(storageLocation);
            return StatusCode(response.Status, response);
        }

        // PUT: api/StorageLocation/{id}
        [HttpPut("{id}")]
        public ActionResult<StorageLocationResponse> UpdateStorageLocation(Guid id, [FromBody] StorageLocation storageLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StorageLocationResponse response = _storageLocationService.UpdateLocation(id, storageLocation);
            return StatusCode(response.Status, response);
        }

        // DELETE: api/StorageLocation/{id}
        [HttpDelete("{id}")]
        public ActionResult<StorageLocationResponse> DeleteStorageLocation(Guid id)
        {
            StorageLocationResponse response = _storageLocationService.DeleteLocation(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/nearby
        [HttpGet("nearby")]
        public ActionResult<StorageLocationResponse> FindNearbyLocations(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm)
        {
            StorageLocationResponse response = _storageLocationService.FindNearbyLocations(lat, lng, radiusKm);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/dimensions
        [HttpGet("dimensions")]
        public ActionResult<StorageLocationResponse> FindLocationsByDimensions(
            [FromQuery] double length = -1,
            [FromQuery] double width = -1,
            [FromQuery] double height = -1)
        {
            StorageLocationResponse response = _storageLocationService.FindLocationsByDimensions(length, width, height);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/capacity
        [HttpGet("capacity")]
        public ActionResult<StorageLocationResponse> FindLocationsWithSufficientCapacity([FromQuery] double volume)
        {
            StorageLocationResponse response = _storageLocationService.FindLocationsWithSufficientCapacity(volume);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/price
        [HttpGet("price")]
        public ActionResult<StorageLocationResponse> FindLocationsByPrice([FromQuery] double maxPrice)
        {
            StorageLocationResponse response = _storageLocationService.FindLocationsByPrice(maxPrice);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/count
        [HttpGet("count")]
        public ActionResult<int> GetLocationCount()
        {
            int count = _storageLocationService.GetLocationCount();
            return Ok(count);
        }

        // HEAD: api/StorageLocation/{id}
        [HttpHead("{id}")]
        public IActionResult CheckLocationExists(Guid id)
        {
            bool exists = _storageLocationService.LocationExists(id);
            return exists ? Ok() : NotFound();
        }
    }
}
