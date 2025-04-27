import { useEffect, useRef, useState } from "react";
import {
  GoogleMaps,
  GoogleMapsRef,
} from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import { StorageLocation } from "@/types/storage";
import Modal from "@/components/common/Modal";
import { getStorageLocations } from "@/lib/api";
import { useAuth } from "@/hooks/use-auth";

export default function Listings() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>([]);
  const [selectedListing, setSelectedListing] =
    useState<StorageLocation | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { user } = useAuth();

  // Handler to add a new location
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
        const response = await getStorageLocations();
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
  }, []);

  const handleListingClick = (listing: StorageLocation) => {
    setSelectedListing(listing);
    setIsModalOpen(true);
    if (mapRef.current) {
      mapRef.current.panTo(listing.lat, listing.lng);
      mapRef.current.setZoom(16);
    }
  };

  const handleSearch = (query: string) => {
    console.log("Searching for:", query);
  };

  return (
    <div className="flex flex-col h-[calc(100vh-150px)] w-full">
      <div className="flex flex-1 w-full">
        <div className="w-1/3 p-4 overflow-y-auto">
          <SearchBar onSearch={handleSearch} />
          <ListingsPanel
            locations={locations}
            onListingClick={handleListingClick}
          />
        </div>
        <div className="w-2/3 relative">
          <GoogleMaps
            ref={mapRef}
            locations={locations}
            onAddLocation={handleAddLocation}
          />
        </div>
      </div>

      <Modal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        listing={selectedListing}
        isOwner={!!user?.id && selectedListing?.lenderId === user.id}
        currentUserId={user?.id ?? null}
      />
    </div>
  );
}
