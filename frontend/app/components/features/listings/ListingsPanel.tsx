import { StorageLocation } from "@/types/storage";
import { ListingsCard } from "./ListingsCard";

// Props for the ListingsPanel component
type ListingsPanelProps = {
  locations: StorageLocation[]; // Array of listings to display
  onListingClick: (listing: StorageLocation) => void; // Callback when a listing card is clicked
};

// Main panel component to display all current listings
export function ListingsPanel({
  locations,
  onListingClick,
}: ListingsPanelProps) {
  return (
    <div className="bg-gray-100 rounded-lg min-h-[calc(100vh-240px)] shadow-md">
      {/* Header */}
      <h2 className="text-xl font-bold p-4 bg-blue-600 text-white rounded-t-lg">
        Current Listings
      </h2>

      {/* Content area — scrollable if needed */}
      <div className="p-4 space-y-3 overflow-y-auto max-h-[calc(100vh-250px)]">
        {/* Show fallback text if there are no listings */}
        {locations.length === 0 ? (
          <div className="text-gray-500 text-center py-6">
            No listings available
          </div>
        ) : (
          // Render a ListingsCard for each location
          locations.map((location) => (
            <ListingsCard
              key={location.id} // Unique key for React rendering
              listing={location} // Pass listing data
              onListingClick={() => onListingClick(location)} // Trigger callback when clicked
            />
          ))
        )}
      </div>
    </div>
  );
}