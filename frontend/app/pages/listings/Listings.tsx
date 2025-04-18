import { useEffect, useRef, useState } from "react";
import {
  GoogleMaps,
  GoogleMapsRef,
  StorageLocation,
} from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";

//Mock data
const mockLocations: StorageLocation[] = [
  {
    id: "1",
    name: "David's Storage",
    description: "Enough storage space for all your items!",
    lat: 42.35,
    lng: -71.105,
    price: 85,
    image: "https://picsum.photos/200",
  },
];

export default function Listings() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>(mockLocations);
  const [showPopup, setShowPopup] = useState(false);

  const [newLocationName, setNewLocationName] = useState("");
  const [newLocationDesc, setNewLocationDesc] = useState("");

  useEffect(() => {
    fetch("http://localhost:5004/api/StorageLocation")
      .then((res) => res.json())
      .then((data) => {
        if (data?.data && data.data.length > 0) setLocations(data.data);
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

  const handleCreateListing = () => {
    const newLocation: StorageLocation = {
      id: (Math.random() * 10000).toString(), // for now, just a random id
      name: newLocationName,
      description: newLocationDesc,
      lat: 42.36,
      lng: -71.104,
      price: 99,
      image: "https://picsum.photos/200",
    };

    setLocations([...locations, newLocation]);
    setShowPopup(false);
    setNewLocationName("");
    setNewLocationDesc("");
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
  
        {/* map + plus button + popup */}
        <div className="w-2/3 relative">
          <GoogleMaps ref={mapRef} locations={locations} />
  
          {/* Pplus button */}
          <button
            onClick={() => setShowPopup(!showPopup)}
            className="absolute top-4 right-4 bg-blue-600 text-white text-2xl rounded-full p-4 shadow-lg hover:bg-blue-700 transition"
          >
            +
          </button>
  
          {/* popup */}
          {showPopup && (
            <div className="absolute top-20 right-4 bg-white p-6 rounded-xl shadow-2xl border w-96 z-50">
              <h2 className="font-semibold text-lg mb-4">Add New Listing</h2>
              <input
                type="text"
                placeholder="Name"
                value={newLocationName}
                onChange={(e) => setNewLocationName(e.target.value)}
                className="border p-2 mb-3 w-full rounded"
              />
              <input
                type="text"
                placeholder="Description"
                value={newLocationDesc}
                onChange={(e) => setNewLocationDesc(e.target.value)}
                className="border p-2 mb-3 w-full rounded"
              />
              <button
                onClick={handleCreateListing}
                className="bg-blue-600 text-white w-full p-2 rounded hover:bg-blue-700"
              >
                Save
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}  