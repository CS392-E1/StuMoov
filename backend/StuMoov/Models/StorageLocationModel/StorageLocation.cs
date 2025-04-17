using System.ComponentModel.DataAnnotations;
using StuMoov.Models.UserModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.StorageLocationModel;
[Table("storage_locations")]
public class StorageLocation : BaseModel
{
    [Key]
    [PrimaryKey("id")]
    public Guid Id { get; private set; } // Unique identifier for the storage location, managed by the database
    [Column("user_id")]
    [Required]
    public Guid UserId { get; private set; } // Identifier for the user who owns or manages the storage location
    [Reference(typeof(User), ReferenceAttribute.JoinType.Inner, true, "user_id")]
    public Lender? User { get; private set; } // Reference to the Lender who owns the storage location
    // Public properties with private setters to ensure data integrity
    [Column("name")]
    [Required]
    public string Name { get; set; }  // Name of the storage location
    [Column("description")]
    public string Description { get; set; } // Description of the storage location
    [Column("lat")]
    [Required]
    public double Lat { get; set; } // Latitude coordinate
    [Column("lng")]
    [Required]
    public double Lng { get; set; } // Longitude coordinate
    [Column("storage_length")]
    [Required]
    public double StorageLength { get; private set; } // Length of the storage space
    [Column("storage_width")]
    [Required]
    public double StorageWidth { get; private set; } // Width of the storage space
    [Column("storage_height")]
    [Required]
    public double StorageHeight { get; private set; } // Height of the storage space
    [Column("storage_volume_total")]
    public double StorageVolumeTotal { get; private set; } // Total storage volume capacity
    [Column("price")]
    [Required]
    public double Price { get; private set; } // Remaining available volume in storage

    // Constructor for EF Core - it needs one that only takes scalar values
    private StorageLocation()
    {
        // This empty constructor is for EF Core
        // The private modifier restricts its usage to EF Core only
    }

    // Constructor to initialize a StorageLocation instance
    public StorageLocation(Lender User,
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
        UserId = User.Id; // userId is also assigned by the database system
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