using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.StorageLocationModel;
public class StorageLocation
{
    public Guid Id { get; private set; } // Unique identifier for the storage location, managed by the database
    [Required]
    public Guid UserId { get; private set; } // Identifier for the user who owns or manages the storage location
    // Public properties with private setters to ensure data integrity
    [Required]
    public string Name { get; set; }  // Name of the storage location
    public string Description { get; set; } // Description of the storage location
    [Required]
    public double Lat { get; set; } // Latitude coordinate
    [Required]
    public double Lng { get; set; } // Longitude coordinate
    [Required]
    public double StorageLength { get; private set; } // Length of the storage space
    [Required]
    public double StorageWidth { get; private set; } // Width of the storage space
    [Required]
    public double StorageHeight { get; private set; } // Height of the storage space
    public double StorageVolumeTotal { get; private set; } // Total storage volume capacity
    [Required]
    public double Price { get; private set; } // Remaining available volume in storage

    // Constructor to initialize a StorageLocation instance
    public StorageLocation(Guid userId,
                           string name,
                           string description,
                           double lat,
                           double lng,
                           double storageLength,
                           double storageWidth,
                           double storageHeight,
                           double storageVolumeTotal,
                           double price)
    {
        Id = Guid.NewGuid();  // ID is expected to be assigned by the database system
        UserId = userId; // userId is also assigned by the database system
        Name = name;
        Description = description;
        Lat = lat;
        Lng = lng;
        StorageLength = storageLength;
        StorageWidth = storageWidth;
        StorageHeight = storageHeight;
        StorageVolumeTotal = storageVolumeTotal;
        Price = price;
    }
}