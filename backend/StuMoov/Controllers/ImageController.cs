/**
 * ImageController.cs
 * 
 * Handles image management functionality including uploading and retrieving images
 * for storage locations and bookings. Provides endpoints for image operations.
 */

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
    private readonly ImageService _imageService;  // Service for managing image operations

    /// <summary>
    /// Initialize the ImageController with required dependencies
    /// </summary>
    /// <param name="imageDao">Data access object for image operations</param>
    public ImageController(ImageDao imageDao)
    {
        _imageService = new ImageService(imageDao);
    }

    /// <summary>
    /// Retrieves all images associated with a specific storage location
    /// </summary>
    /// <param name="storageLocationId">The unique identifier of the storage location</param>
    /// <returns>Collection of images for the specified storage location</returns>
    /// <route>GET: api/image/storage/{storageLocationId}</route>
    [HttpGet("storage/{storageLocationId}")]
    public async Task<ActionResult<Response>> GetImagesByStorageLocation(Guid storageLocationId)
    {
        // Get images for the specified storage location
        var response = await _imageService.GetImagesByStorageLocationAsync(storageLocationId);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all images associated with a specific booking
    /// </summary>
    /// <param name="bookingId">The unique identifier of the booking</param>
    /// <returns>Collection of images for the specified booking</returns>
    /// <route>GET: api/image/booking/{bookingId}</route>
    [HttpGet("booking/{bookingId}")]
    public async Task<ActionResult<Response>> GetImagesByBooking(Guid bookingId)
    {
        // Get images for the specified booking
        var response = await _imageService.GetImagesByBookingAsync(bookingId);
        return Ok(response);
    }

    /// <summary>
    /// Uploads an image for a storage location
    /// </summary>
    /// <param name="image">The image object containing image data and metadata</param>
    /// <returns>Result of the upload operation</returns>
    /// <route>POST: api/image/storage</route>
    [Route("storage")]
    [HttpPost]
    public async Task<ActionResult<Response>> UploadStorageLocationImage([FromBody] Image image)
    {
        // Process the storage location image upload
        var response = await _imageService.UploadStorageImageAsync(image);
        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Uploads an image for a drop-off event
    /// </summary>
    /// <param name="image">The image object containing image data and metadata</param>
    /// <returns>Result of the upload operation</returns>
    /// <route>POST: api/image/dropoff</route>
    [Route("dropoff")]
    [HttpPost]
    public async Task<ActionResult<Response>> UploadDropOffImage([FromBody] Image image)
    {
        // Process the drop-off image upload
        var response = await _imageService.UploadDropoffImageAsync(image);
        return StatusCode(response.Status, response);
    }
}