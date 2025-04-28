import { StorageLocation } from "@/types/storage";
import { ListingsCard } from "./ListingsCard";

type ListingsPanelProps = {
  locations: StorageLocation[];
  onListingClick: (listing: StorageLocation) => void;
};

export function ListingsPanel({
  locations,
  onListingClick,
}: ListingsPanelProps) {
  return (
    <div className="bg-gray-100 rounded-lg min-h-[calc(100vh-240px)] shadow-md">
      <h2 className="text-xl font-bold p-4 bg-blue-600 text-white rounded-t-lg">
        Current Listings
      </h2>
      <div className="p-4 space-y-3 overflow-y-auto max-h-[calc(100vh-250px)]">
        {locations.length === 0 ? (
          <div className="text-gray-500 text-center py-6">
            No listings available
          </div>
        ) : (
          locations.map((location) => (
            <ListingsCard
              key={location.id}
              listing={location}
              onListingClick={() => onListingClick(location)}
            />
          ))
        )}
      </div>
    </div>
  );
}
