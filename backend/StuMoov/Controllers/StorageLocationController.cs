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
        private readonly StorageLocationService _storageLocationService;
        private readonly StorageLocationDao _storageLocationDao;
        private readonly UserDao _userDao;

        // Constructor to inject dependencies
        public StorageLocationController(StorageLocationDao storageLocationDao, UserDao userDao)
        {
            this._storageLocationDao = storageLocationDao;
            this._storageLocationService = new StorageLocationService(storageLocationDao);
            this._userDao = userDao;
        }

        // GET: api/StorageLocation
        [HttpGet]
        public async Task<ActionResult<Response>> GetAllStorageLocations()
        {
            Response response = await _storageLocationService.GetAllLocationsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Response>> GetStorageLocationById(Guid id)
        {
            Response response = await _storageLocationService.GetLocationByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Response>> GetStorageLocationsByUserId(Guid userId)
        {
            Response response = await _storageLocationService.GetLocationsByUserIdAsync(userId);
            return StatusCode(response.Status, response);
        }

        // POST: api/StorageLocation
        [HttpPost]
        public async Task<ActionResult<Response>> CreateStorageLocation([FromBody] CreateStorageLocationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request data.", ModelState));
            }

            User? lenderUser = await _userDao.GetUserByIdAsync(request.LenderId);
            if (lenderUser == null || !(lenderUser is Lender lender))
            {
                return NotFound(new Response(StatusCodes.Status404NotFound, "Lender not found or user is not a lender.", null));
            }

            double length = request.Length ?? 0;
            double width = request.Width ?? 0;
            double height = request.Height ?? 0;
            double volume = length * width * height;
            double price = request.Price ?? 0;

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

            Response response = await _storageLocationService.CreateLocationAsync(newStorageLocation);
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

            Response response = await _storageLocationService.UpdateLocationAsync(id, storageLocation);
            return StatusCode(response.Status, response);
        }

        // DELETE: api/StorageLocation/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response>> DeleteStorageLocation(Guid id)
        {
            Response response = await _storageLocationService.DeleteLocationAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/nearby
        [HttpGet("nearby")]
        public async Task<ActionResult<Response>> FindNearbyLocations(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm)
        {
            Response response = await _storageLocationService.FindNearbyLocationsAsync(lat, lng, radiusKm);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/dimensions
        [HttpGet("dimensions")]
        public async Task<ActionResult<Response>> FindLocationsByDimensions(
            [FromQuery] double length = -1,
            [FromQuery] double width = -1,
            [FromQuery] double height = -1)
        {
            Response response = await _storageLocationService.FindLocationsByDimensionsAsync(length, width, height);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/capacity
        [HttpGet("capacity")]
        public async Task<ActionResult<Response>> FindLocationsWithSufficientCapacity([FromQuery] double volume)
        {
            Response response = await _storageLocationService.FindLocationsWithSufficientCapacityAsync(volume);
            return StatusCode(response.Status, response);
        }

        // GET: api/StorageLocation/price
        [HttpGet("price")]
        public async Task<ActionResult<Response>> FindLocationsByPrice([FromQuery] double maxPrice)
        {
            Response response = await _storageLocationService.FindLocationsByPriceAsync(maxPrice);
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

// DTO for creating a storage location
public class CreateStorageLocationRequest
{
    public Guid LenderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double? Price { get; set; }
    public double? Length { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
}
