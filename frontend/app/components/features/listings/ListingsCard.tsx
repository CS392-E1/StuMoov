import { StorageLocation } from "@/types/storage";
interface ListingsCardProps {
  listing: StorageLocation;
  onListingClick: () => void; // <-- just trigger onClick (no lat/lng here)
}

export function ListingsCard({ listing, onListingClick }: ListingsCardProps) {
  return (
    <div
      className="bg-white p-4 rounded-lg shadow-sm cursor-pointer hover:bg-blue-50 transition border border-gray-200"
      onClick={onListingClick} // <-- just call it
    >
      <div className="flex flex-row">
        <div className="flex flex-col flex-grow pr-3">
          <div className="font-semibold">{listing.name}</div>
          <div className="text-sm text-gray-600">{listing.address}</div>
          <div className="text-sm mb-2">{listing.description}</div>
          <div className="text-blue-600 font-semibold">
            ${listing.price}/month
          </div>
        </div>
        <div className="flex items-center">
          {listing.imageUrl && (
            <img
              src={listing.imageUrl}
              alt={listing.name}
              className="w-24 h-24 object-cover rounded"
            />
          )}
        </div>
      </div>
    </div>
  );
}