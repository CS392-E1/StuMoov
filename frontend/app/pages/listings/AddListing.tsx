import { useEffect, useRef, useState } from "react";
import axios from "axios";
import { GoogleMaps, GoogleMapsRef } from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import Modal from "@/components/common/Modal";
import { StorageLocation, User } from "@/types/storage";

export default function AddListing() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>([]);
  const [selectedLocation, setSelectedLocation] = useState<StorageLocation | null>(null);
  const [interestedRenters, setInterestedRenters] = useState<User[]>([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<"Renters" | "Messages">("Renters");

  const userId = "00000000-0000-0000-0000-000000000000";

  useEffect(() => {
    axios.get(`http://localhost:5004/api/storage/user/${userId}`)
      .then((res) => {
        if (res.data?.data) {
          setLocations(res.data.data);
        }
      })
      .catch((err) => console.error("Failed to fetch user listings", err));
  }, [userId]);

  const handleListingClick = (listing: StorageLocation) => {
    setSelectedLocation(listing);
    setModalOpen(true);
    setActiveTab("Renters");

    axios.get("http://localhost:5004/api/user/renters")
      .then((res) => {
        if (res.data?.data) {
          setInterestedRenters(res.data.data);
        }
      })
      .catch((err) => console.error("Failed to fetch interested renters", err));

    mapRef.current?.panTo(listing.lat, listing.lng);
    mapRef.current?.setZoom(16);
  };

  const handleSearch = (query: string) => {
    console.log("Searching for:", query);
  };

  return (
    <div className="flex flex-col h-[calc(100vh-150px)] w-full">
      <div className="flex flex-1 w-full">
        <div className="w-1/3 p-4 overflow-y-auto">
          <SearchBar onSearch={handleSearch} />
          <ListingsPanel locations={locations} onListingClick={handleListingClick} />
        </div>
        <div className="w-2/3 relative">
          <GoogleMaps ref={mapRef} locations={locations} />
        </div>
      </div>

      {modalOpen && selectedLocation && (
        <div className="fixed inset-0 bg-gray-800 bg-opacity-30 backdrop-blur-sm flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl max-h-[80vh] overflow-y-auto">
            <div className="flex justify-between items-center p-4 border-b">
              <h2 className="text-xl font-semibold">{`Interested in ${selectedLocation.name}`}</h2>
              <button onClick={() => setModalOpen(false)} className="text-gray-600 hover:text-black text-lg">&times;</button>
            </div>

            <div className="border-b flex">
              {["Renters", "Messages"].map((tab) => (
                <button
                  key={tab}
                  className={`flex-1 p-2 text-sm font-medium transition ${
                    activeTab === tab
                      ? "border-b-2 border-blue-600 text-blue-600"
                      : "text-gray-600 hover:text-blue-500"
                  }`}
                  onClick={() => setActiveTab(tab as "Renters" | "Messages")}
                >
                  {tab}
                </button>
              ))}
            </div>

            <div className="p-4">
              {activeTab === "Renters" && (
                <ul className="space-y-2">
                  {interestedRenters.length > 0 ? (
                    interestedRenters.map((renter) => (
                      <li key={renter.id} className="p-2 border rounded">
                        <p><strong>Email:</strong> {renter.email}</p>
                        <p><strong>Display Name:</strong> {renter.displayName || "N/A"}</p>
                      </li>
                    ))
                  ) : (
                    <p>No interested renters found.</p>
                  )}
                </ul>
              )}

              {activeTab === "Messages" && (
                <div className="space-y-3">
                  <div className="text-sm text-gray-600">
                    <strong>You:</strong> Hey, I saw you booked this spot. Let me know if you have questions!
                  </div>
                  <div className="text-sm text-gray-600">
                    <strong>David:</strong> Thank you! Just wondering if there's a loading dock?
                  </div>
                  <div className="text-sm text-gray-600">
                    <strong>You:</strong> Yup! There's one at the rear entrance FAM.
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}