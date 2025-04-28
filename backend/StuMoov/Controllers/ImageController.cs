namespace StuMoov.Controllers;

using Microsoft.AspNetCore.Mvc;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.ImageModel;
using StuMoov.Service;

[ApiController]
[Route("api/image")]
public class ImageController : ControllerBase
{
    private readonly ImageService _imageService;

    public ImageController(ImageDao imageDao)
    {
        _imageService = new ImageService(imageDao);
    }

    [HttpGet("storage/{storageLocationId}")]
    public async Task<ActionResult<Response>> GetImagesByStorageLocation(Guid storageLocationId)
    {
        var response = await _imageService.GetImagesByStorageLocationAsync(storageLocationId);
        return Ok(response);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<ActionResult<Response>> GetImagesByBooking(Guid bookingId)
    {
        var response = await _imageService.GetImagesByBookingAsync(bookingId);
        return Ok(response);
    }

    [Route("storage")]
    [HttpPost]
    public async Task<ActionResult<Response>> UploadStorageLocationImage([FromBody] Image image)
    {
        var response = await _imageService.UploadStorageImageAsync(image);
        return StatusCode(response.Status, response);
    }

    [Route("dropoff")]
    [HttpPost]
    public async Task<ActionResult<Response>> UploadDropOffImage([FromBody] Image image)
    {
        var response = await _imageService.UploadDropoffImageAsync(image);
        return StatusCode(response.Status, response);
    }
}