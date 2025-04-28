namespace StuMoov.Dao;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.ImageModel;

public class ImageDao
{
    private readonly AppDbContext _dbContext;

    public ImageDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Image?> GetImageByIdAsync(Guid id)
    {
        return await _dbContext.Images.FirstOrDefaultAsync(img => img.Id == id);
    }

    public async Task<List<Image>> GetImagesByStorageLocationIdAsync(Guid storageLocationId)
    {
        return await _dbContext.Images
            .Where(img => img.StorageLocationId == storageLocationId)
            .ToListAsync();
    }

    public async Task<List<Image>> GetImagesByBookingIdAsync(Guid bookingId)
    {
        return await _dbContext.Images
            .Where(img => img.BookingId == bookingId)
            .ToListAsync();
    }

    public async Task AddImageAsync(Image image)
    {
        await _dbContext.Images.AddAsync(image);
        await _dbContext.SaveChangesAsync();
    }

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