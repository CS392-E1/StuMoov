import { useEffect, useRef, useState } from "react";
import {
  GoogleMaps,
  GoogleMapsRef,
} from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import { StorageLocation } from "@/types/storage";
import Modal from "@/components/common/Modal";
import { useGeocoding } from "@/hooks/use-geocoding";
import { getStorageLocations } from "@/lib/api";
import { getStorageLocationsByCoordinates } from "@/lib/api";
import {
  getStorageLocationsByDimensions,
  getStorageLocationsByPrice,
  getStorageLocationsByCapacity,
} from "@/lib/api";
import { useAuth } from "@/hooks/use-auth";

const FilterPopup = ({
  onClose,
  onApplyFilters,
  filters,
}: {
  onClose: () => void;
  onApplyFilters: (filters: any) => void;
  filters: any;
}) => {
  const [length, setLength] = useState<number | string>(filters.length || "");
  const [width, setWidth] = useState<number | string>(filters.width || "");
  const [height, setHeight] = useState<number | string>(filters.height || "");
  const [volume, setVolume] = useState<number | string>(filters.volume || "");
  const [price, setPrice] = useState<number | string>(filters.price || "");

  const handleApplyFilters = () => {
    onApplyFilters({
      length: length ? parseFloat(length as string) : undefined,
      width: width ? parseFloat(width as string) : undefined,
      height: height ? parseFloat(height as string) : undefined,
      volume: volume ? parseFloat(volume as string) : undefined,
      price: price ? parseFloat(price as string) : undefined,
    });
    onClose(); // Close the filter popup after applying
  };

  return (
    <div className="fixed inset-0 z-50 bg-opacity-50 backdrop-blur-sm flex justify-center items-center">
      <div className="bg-white p-6 rounded-lg shadow-lg max-w-sm w-full">
        <h3 className="font-semibold text-xl mb-4">Filter Storage Locations</h3>
        <div className="flex flex-col gap-4">
          <input
            type="number"
            value={length}
            onChange={(e) => setLength(e.target.value)}
            placeholder="Length"
            className="input px-4 py-2"
          />
          <input
            type="number"
            value={width}
            onChange={(e) => setWidth(e.target.value)}
            placeholder="Width"
            className="input px-4 py-2"
          />
          <input
            type="number"
            value={height}
            onChange={(e) => setHeight(e.target.value)}
            placeholder="Height"
            className="input px-4 py-2"
          />
          <input
            type="number"
            value={volume}
            onChange={(e) => setVolume(e.target.value)}
            placeholder="Volume"
            className="input px-4 py-2"
          />
          <input
            type="number"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            placeholder="Max Price"
            className="input px-4 py-2"
          />
          <button
            className="mt-4 bg-blue-600 text-white px-4 py-2 rounded"
            onClick={handleApplyFilters}
          >
            Apply Filters
          </button>
          <button className="mt-2 text-red-500" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default function Listings() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>([]);
  const [selectedListing, setSelectedListing] =
    useState<StorageLocation | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isFilterPopupOpen, setIsFilterPopupOpen] = useState(false);
  const { user } = useAuth();

  const [filters, setFilters] = useState<any>({});

  const { geocodeAddress, isLoading, error } = useGeocoding();

  const handleAddLocation = (newLocation: StorageLocation) => {
    setLocations((prevLocations) => [...prevLocations, newLocation]);
    if (mapRef.current) {
      mapRef.current.panTo(newLocation.lat, newLocation.lng);
      mapRef.current.setZoom(16);
    }
  };

  useEffect(() => {
    const fetchLocations = async () => {
      try {
        // If no filters, fetch all locations
        let response;
        if (Object.keys(filters).length === 0) {
          response = await getStorageLocations();
        } else {
          response = await getStorageLocationsByDimensions(
            filters.length,
            filters.width,
            filters.height
          );
        }
        if (
          response.status >= 200 &&
          response.status < 300 &&
          response.data?.data
        ) {
          setLocations(response.data.data);
        } else {
          console.error(
            "Failed to fetch storage locations:",
            response.data?.message || `Status code ${response.status}`
          );
        }
      } catch (err) {
        console.error("Failed to fetch storage locations:", err);
      }
    };

    fetchLocations();
  }, [filters]);

  const handleListingClick = (listing: StorageLocation) => {
    setSelectedListing(listing);
    setIsModalOpen(true);
    if (mapRef.current) {
      mapRef.current.panTo(listing.lat, listing.lng);
      mapRef.current.setZoom(16);
    }
  };

  const handleSearch = async (query: string) => {
    console.log("Searching for:", query);

    try {
      const result = await geocodeAddress(query);
      console.log("Geocoded coordinates:", result);

      const response = await getStorageLocationsByCoordinates(
        result.lat,
        result.lng
      );

      if (
        response.status >= 200 &&
        response.status < 300 &&
        response.data?.data
      ) {
        setLocations(response.data.data);
      } else {
        console.error(
          "Failed to fetch storage locations:",
          response.data?.message
        );
      }
    } catch (err) {
      console.error("Geocoding failed:", err);
    }
  };

  const handleApplyFilters = async (newFilters: any) => {
    setFilters(newFilters);
    let filteredLocations: StorageLocation[] = [];

    if (newFilters.length || newFilters.width || newFilters.height) {
      const response = await getStorageLocationsByDimensions(
        newFilters.length,
        newFilters.width,
        newFilters.height
      );
      filteredLocations = response.data.data || [];
    }

    if (newFilters.volume) {
      const response = await getStorageLocationsByCapacity(newFilters.volume);
      filteredLocations = response.data.data || [];
    }

    if (newFilters.price) {
      const response = await getStorageLocationsByPrice(newFilters.price);
      filteredLocations = response.data.data || [];
    }

    setLocations(filteredLocations);
  };

  return (
    <div className="flex flex-col h-screen w-full">
      <div className="flex flex-1 w-full">
        {/* left panel */}
        <div className="w-1/3 p-4 overflow-y-auto max-h-[80vh]">
          <button
            className="mb-4 bg-blue-600 text-white px-4 py-2 rounded"
            onClick={() => setIsFilterPopupOpen(true)}
          >
            Show Filters
          </button>
          <SearchBar onSearch={handleSearch} />
          <div className="overflow-y-auto max-h-[calc(100vh-150px)]">
            <ListingsPanel
              locations={locations}
              onListingClick={handleListingClick}
            />
          </div>
        </div>

        {/* right panel */}
        <div className="w-2/3 relative">
          <GoogleMaps
            ref={mapRef}
            locations={locations}
            onAddLocation={handleAddLocation}
          />
        </div>
      </div>

      {/* modal for listing details */}
      <Modal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        listing={selectedListing}
        isOwner={!!user?.id && selectedListing?.lenderId === user.id}
        currentUserId={user?.id ?? null}
      />

      {/* filter popup */}
      {isFilterPopupOpen && (
        <FilterPopup
          filters={filters}
          onClose={() => setIsFilterPopupOpen(false)}
          onApplyFilters={handleApplyFilters}
        />
      )}
    </div>
  );
}
