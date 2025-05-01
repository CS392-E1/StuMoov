/**
 * ImageService.cs
 *
 * Manages image-related operations for the StuMoov application.
 * Provides functionality to retrieve and upload images associated with storage locations
 * and bookings. Integrates with the ImageDao for data access.
 */

namespace StuMoov.Service
{
    using System;
    using System.Threading.Tasks;
    using StuMoov.Dao;
    using StuMoov.Models;
    using StuMoov.Models.ImageModel;

    /// <summary>
    /// Service responsible for managing image operations, including retrieving and uploading images
    /// for storage locations and bookings.
    /// </summary>
    public class ImageService
    {
        /// <summary>
        /// Data access object for image-related database operations.
        /// </summary>
        private readonly ImageDao _imageDao;

        /// <summary>
        /// Initializes a new instance of the ImageService with the required dependency.
        /// </summary>
        /// <param name="imageDao">DAO for image operations.</param>
        public ImageService(ImageDao imageDao)
        {
            _imageDao = imageDao;
        }

        /// <summary>
        /// Retrieves all images associated with a specific storage location.
        /// </summary>
        /// <param name="storageLocationId">The ID of the storage location.</param>
        /// <returns>A Response object with the status, message, and list of images.</returns>
        public async Task<Response> GetImagesByStorageLocationAsync(Guid storageLocationId)
        {
            var images = await _imageDao.GetImagesByStorageLocationIdAsync(storageLocationId);
            return new Response(200, "Images fetched successfully", images);
        }

        /// <summary>
        /// Retrieves all images associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>A Response object with the status, message, and list of images.</returns>
        public async Task<Response> GetImagesByBookingAsync(Guid bookingId)
        {
            var images = await _imageDao.GetImagesByBookingIdAsync(bookingId);
            return new Response(200, "Images fetched successfully", images);
        }

        /// <summary>
        /// Uploads an image associated with a storage location.
        /// </summary>
        /// <param name="image">The image data containing the URL and storage location ID.</param>
        /// <returns>A Response object with the status, message, and created image data.</returns>
        public async Task<Response> UploadStorageImageAsync(Image image)
        {
        Image newImage = new Image();
        newImage.Url = image.Url;
        newImage.StorageLocationId = image.StorageLocationId;

            await _imageDao.AddImageAsync(newImage);
            return new Response(201, "Image uploaded successfully", newImage);
        }

        /// <summary>
        /// Uploads an image associated with a booking, typically for dropoff purposes.
        /// </summary>
        /// <param name="image">The image data containing the URL and booking ID.</param>
        /// <returns>A Response object with the status, message, and created image data.</returns>
        public async Task<Response> UploadDropoffImageAsync(Image image)
        {
        Image newImage = new Image();
        newImage.Url = image.Url;
        newImage.BookingId = image.BookingId;

            await _imageDao.AddImageAsync(newImage);
            return new Response(201, "Image uploaded successfully", newImage);
        }
    }
}