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
import { Filter } from "@/types/filter";

const FilterPopup = ({
  onClose,
  onApplyFilters,
  filters,
}: {
  onClose: () => void;
  onApplyFilters: (filters: Filter) => void;
  filters: Filter;
}) => {
  const [length, setLength] = useState<string>(String(filters.length ?? ""));
  const [width, setWidth] = useState<string>(String(filters.width ?? ""));
  const [height, setHeight] = useState<string>(String(filters.height ?? ""));
  const [volume, setVolume] = useState<string>(String(filters.volume ?? ""));
  const [price, setPrice] = useState<string>(String(filters.price ?? ""));

  const handleApplyFilters = () => {
    const appliedFilters: Filter = {
      length: length ? parseFloat(length) : undefined,
      width: width ? parseFloat(width) : undefined,
      height: height ? parseFloat(height) : undefined,
      volume: volume ? parseFloat(volume) : undefined,
      price: price ? parseFloat(price) : undefined,
    };
    onApplyFilters(appliedFilters);
    onClose();
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

  const initialFilterState: Filter = {
    length: undefined,
    width: undefined,
    height: undefined,
    volume: undefined,
    price: undefined,
  };
  const [filters, setFilters] = useState<Filter>(initialFilterState);

  const { geocodeAddress } = useGeocoding();

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
        let response;
        if (filters.length || filters.width || filters.height) {
          response = await getStorageLocationsByDimensions(
            filters.length,
            filters.width,
            filters.height
          );
        } else if (filters.volume) {
          response = await getStorageLocationsByCapacity(filters.volume);
        } else if (filters.price) {
          response = await getStorageLocationsByPrice(filters.price);
        } else {
          response = await getStorageLocations();
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

  const handleApplyFilters = async (newFilters: Filter) => {
    setFilters(newFilters);
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
