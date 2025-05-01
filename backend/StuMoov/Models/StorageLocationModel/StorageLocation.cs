/**
 * StorageLocation.cs
 *
 * Represents a storage listing provided by a lender, including location, dimensions,
 * capacity, pricing, and ownership. Mapped to the "StorageLocations" table via Supabase
 * Postgrest attributes and compatible with EF Core. Inherits from BaseModel for common properties.
 */

using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using StuMoov.Models.UserModel;

namespace StuMoov.Models.StorageLocationModel
{
    [Table("StorageLocations")]
    public class StorageLocation : BaseModel
    {
        /// <summary>
        /// Unique identifier for the storage location.
        /// Managed by the database and set in the constructor.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the lender who owns this storage location.
        /// </summary>
        [Column("user_id")]
        [Required]
        public Guid LenderId { get; private set; }

        /// <summary>
        /// Navigation property to the Lender entity.
        /// </summary>
        [Reference(typeof(User), ReferenceAttribute.JoinType.Inner, true, "user_id")]
        public Lender? Lender { get; private set; }

        /// <summary>
        /// Display name of the storage location.
        /// </summary>
        [Column("name")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the storage location.
        /// </summary>
        [Column("description")]
        public string Description { get; set; }

        /// <summary>
        /// Physical address of the storage location.
        /// </summary>
        [Column("address")]
        public string Address { get; set; }

        /// <summary>
        /// Latitude coordinate for geospatial queries.
        /// </summary>
        [Column("lat")]
        [Required]
        public double Lat { get; set; }

        /// <summary>
        /// Longitude coordinate for geospatial queries.
        /// </summary>
        [Column("lng")]
        [Required]
        public double Lng { get; set; }

        /// <summary>
        /// Length dimension of the storage space in appropriate units.
        /// </summary>
        [Column("storage_length")]
        [Required]
        public double StorageLength { get; private set; }

        /// <summary>
        /// Width dimension of the storage space.
        /// </summary>
        [Column("storage_width")]
        [Required]
        public double StorageWidth { get; private set; }

        /// <summary>
        /// Height dimension of the storage space.
        /// </summary>
        [Column("storage_height")]
        [Required]
        public double StorageHeight { get; private set; }

        /// <summary>
        /// Total volume capacity of the storage space.
        /// </summary>
        [Column("storage_volume_total")]
        public double StorageVolumeTotal { get; private set; }

        /// <summary>
        /// Price for renting the storage space (e.g., per day or other unit).
        /// </summary>
        [Column("price")]
        [Required]
        public double Price { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core and Supabase deserialization.
        /// </summary>
        private StorageLocation()
        {
            // Required for EF Core and Supabase
        }

        /// <summary>
        /// Constructs a new StorageLocation with specified lender, location details, dimensions, volume, and price.
        /// Generates a new GUID for the record.
        /// </summary>
        /// <param name="lender">The lender who owns this storage location</param>
        /// <param name="name">Name of the storage location</param>
        /// <param name="description">Description of the storage location</param>
        /// <param name="address">Physical address of the storage location</param>
        /// <param name="lat">Latitude coordinate for the location</param>
        /// <param name="lng">Longitude coordinate for the location</param>
        /// <param name="storageLength">Length of the storage space</param>
        /// <param name="storageWidth">Width of the storage space</param>
        /// <param name="storageHeight">Height of the storage space</param>
        /// <param name="storageVolumeTotal">Total volume capacity of the storage space</param>
        /// <param name="price">Price for renting the storage space</param>
        public StorageLocation(
            Lender lender,
            string name,
            string description,
            string address,
            double lat,
            double lng,
            double storageLength,
            double storageWidth,
            double storageHeight,
            double storageVolumeTotal,
            double price)
        {
            Id = Guid.NewGuid();
            LenderId = lender.Id;
            Name = name;
            Description = description;
            Address = address;
            Lat = lat;
            Lng = lng;
            StorageLength = storageLength;
            StorageWidth = storageWidth;
            StorageHeight = storageHeight;
            StorageVolumeTotal = storageVolumeTotal;
            Price = price;
        }
    }
}