import { useEffect, useRef, useState } from "react";
import {
  GoogleMaps,
  GoogleMapsRef,
} from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import { StorageLocation } from "@/types/storage";
import axios from "axios";

//Mock data
const mockLocations: StorageLocation[] = [
  {
    id: "1",
    name: "David's Storage",
    description: "Enough storage space for all your items!",
    lat: 42.35,
    lng: -71.105,
    price: 85,
    address: "123 Main St, Boston, MA",
    imageUrl: "https://picsum.photos/200",
  },
];

export default function Listings() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>(mockLocations);

  useEffect(() => {
    axios.get("http://localhost:5004/api/StorageLocation")
      .then((res) => {
        if (res.data?.data && res.data.data.length > 0) {
          setLocations(res.data.data);
        }
      })
      .catch((err) => {
        console.error("Failed to fetch locations", err);
      });
  }, []);

  const handleListingClick = (lat: number, lng: number) => {
    mapRef.current?.panTo(lat, lng);
    mapRef.current?.setZoom(16);
  };

  const handleSearch = (query: string) => {
    // This is a dummy search function that doesn't actually do anything
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
              <GoogleMaps ref={mapRef} locations={locations} />
            </div>
          </div>
        </div>
      );
    }