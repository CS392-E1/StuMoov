import { StorageLocation } from "@/types/storage";

// Props definition for the ListingsCard component
type ListingsCardProps = {
  listing: StorageLocation; // The listing data to display
  onListingClick: (listing: StorageLocation) => void; // Callback when the card is clicked
};

// Card component to display a single storage listing
export function ListingsCard({ listing, onListingClick }: ListingsCardProps) {
  return (
    <div
      // Wrapper styles and click behavior
      className="bg-white p-4 rounded-lg shadow-sm cursor-pointer hover:bg-blue-50 transition border border-gray-200"
      onClick={() => onListingClick(listing)} // Pass listing to parent handler
    >
      <div className="flex flex-row">
        {/* Left section: text details */}
        <div className="flex flex-col flex-grow pr-3">
          <div className="font-semibold">{listing.name}</div> {/* Listing title */}
          <div className="text-sm text-gray-600">{listing.address}</div> {/* Address */}
          <div className="text-sm mb-2">{listing.description}</div> {/* Description */}
          <div className="text-blue-600 font-semibold">
            ${listing.price}/month {/* Price */}
          </div>
        </div>

        {/* Right section: image thumbnail (if available) */}
        <div className="flex items-center">
          {listing.imageUrl && (
            <img
              src={listing.imageUrl}
              alt={listing.name}
              className="w-24 h-24 object-cover rounded" // Thumbnail styling
            />
          )}
        </div>
      </div>
    </div>
  );
}