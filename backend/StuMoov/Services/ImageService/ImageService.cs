namespace StuMoov.Service;

using System;
using System.Threading.Tasks;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.ImageModel;

public class ImageService
{
    private readonly ImageDao _imageDao;

    public ImageService(ImageDao imageDao)
    {
        _imageDao = imageDao;
    }

    public async Task<Response> GetImagesByStorageLocationAsync(Guid storageLocationId)
    {
        var images = await _imageDao.GetImagesByStorageLocationIdAsync(storageLocationId);

        return new Response(200, "Images fetched successfully", images);
    }

    public async Task<Response> GetImagesByBookingAsync(Guid bookingId)
    {
        var images = await _imageDao.GetImagesByBookingIdAsync(bookingId);

        return new Response(200, "Images fetched successfully", images);
    }

    public async Task<Response> UploadStorageImageAsync(Image image)
    {
        Image newImage = new Image();
        newImage.Url = image.Url;
        newImage.StorageLocationId = image.StorageLocationId;

        await _imageDao.AddImageAsync(newImage);
        return new Response(201, "Image uploaded successfully", newImage);
    }

    public async Task<Response> UploadDropoffImageAsync(Image image)
    {
        Image newImage = new Image();
        newImage.Url = image.Url;
        newImage.BookingId = image.BookingId;

        await _imageDao.AddImageAsync(newImage);
        return new Response(201, "Image uploaded successfully", newImage);
    }
}