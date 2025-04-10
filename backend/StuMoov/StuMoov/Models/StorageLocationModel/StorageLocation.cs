namespace StuMoov.Models.StorageLocationModel;
public class StorageLocation
{
    public string Id { get; private set; } // Unique identifier for the storage location, managed by the database
    public string UserId { get; private set; } // Identifier for the user who owns or manages the storage location

    // Public properties with private setters to ensure data integrity
    public string Name { get; private set; }  // Name of the storage location
    public string Description { get; private set; } // Description of the storage location
    public double Lat { get; private set; } // Latitude coordinate
    public double Lng { get; private set; } // Longitude coordinate
    public double StorageLength { get; private set; } // Length of the storage space
    public double StorageWidth { get; private set; } // Width of the storage space
    public double StorageHeight { get; private set; } // Height of the storage space
    public double StorageVolumeTotal { get; private set; } // Total storage volume capacity
    public double Price { get; private set; } // Remaining available volume in storage

    // Constructor to initialize a StorageLocation instance
    public StorageLocation(string userId,
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
        Id = string.Empty;  // ID is expected to be assigned by the database system
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