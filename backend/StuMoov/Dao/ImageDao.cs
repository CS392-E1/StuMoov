/**
 * ImageDao.cs
 * 
 * Handles data access operations for Image entities including retrieval,
 * creation, and deletion. Uses Entity Framework Core for database interactions.
 */

namespace StuMoov.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StuMoov.Db;
    using StuMoov.Models.ImageModel;

    public class ImageDao
    {
        private readonly AppDbContext _dbContext;  // EF Core database context for images

        /// <summary>
        /// Initialize the ImageDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for image operations</param>
        public ImageDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves an image by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the image</param>
        /// <returns>The Image entity if found; otherwise null</returns>
        public async Task<Image?> GetImageByIdAsync(Guid id)
        {
            return await _dbContext.Images
                .FirstOrDefaultAsync(img => img.Id == id);
        }

        /// <summary>
        /// Retrieves all images associated with a specific storage location.
        /// </summary>
        /// <param name="storageLocationId">The GUID of the storage location</param>
        /// <returns>List of Image entities for the storage location</returns>
        public async Task<List<Image>> GetImagesByStorageLocationIdAsync(Guid storageLocationId)
        {
            return await _dbContext.Images
                .Where(img => img.StorageLocationId == storageLocationId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all images associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">The GUID of the booking</param>
        /// <returns>List of Image entities for the booking</returns>
        public async Task<List<Image>> GetImagesByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.Images
                .Where(img => img.BookingId == bookingId)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new image record to the database.
        /// </summary>
        /// <param name="image">The Image entity to add</param>
        /// <returns>An asynchronous task</returns>
        public async Task AddImageAsync(Image image)
        {
            await _dbContext.Images.AddAsync(image);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an image by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the image to delete</param>
        /// <returns>An asynchronous task</returns>
        public async Task DeleteImageAsync(Guid id)
        {
            var image = await GetImageByIdAsync(id);
            if (image != null)
            {
                _dbContext.Images.Remove(image);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}