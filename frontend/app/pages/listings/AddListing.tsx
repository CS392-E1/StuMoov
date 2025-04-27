import { useEffect, useRef, useState } from "react";
import axios from "axios";
import { GoogleMaps, GoogleMapsRef } from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import Modal from "@/components/common/Modal";
import { StorageLocation, User, Message } from "@/types/storage";

export default function AddListing() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>([]);
  const [selectedLocation, setSelectedLocation] = useState<StorageLocation | null>(null);
  const [interestedRenters, setInterestedRenters] = useState<User[]>([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<"Renters" | "Messages">("Renters");
  const [chatMessages, setChatMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");

  // Your IDs
  const lenderId = "00000000-0000-0000-0000-000000000000"; 
  const renterId = "11111111-2222-3333-4444-555555555555";

  useEffect(() => {
    axios.get(`http://localhost:5004/api/storage/user/${lenderId}`)
      .then((res) => {
        if (res.data?.data) {
          setLocations(res.data.data);
        }
      })
      .catch((err) => console.error("Failed to fetch user listings", err));
  }, []);

  const handleListingClick = async (listing: StorageLocation) => {
    console.log("Clicked listing:", listing);

    if (typeof listing.lat !== "number" || typeof listing.lng !== "number") {
      console.warn("Invalid lat/lng for listing:", listing);
      return;
    }

    setSelectedLocation(listing);
    setModalOpen(true);
    setActiveTab("Renters");

    try {
      const rentersRes = await axios.get("http://localhost:5004/api/user/renters");
      if (rentersRes.data?.data) {
        setInterestedRenters(rentersRes.data.data);
      }

      // HARD CALL this exact API to fetch messages
      const messagesRes = await axios.get(
        `http://localhost:5004/api/messages?user1=${lenderId}&user2=${renterId}`
      );
      if (messagesRes.data?.data) {
        console.log("Hard-fetched messages:", messagesRes.data.data);
        setChatMessages(messagesRes.data.data);
      }
    } catch (err) {
      console.error("Failed to fetch renters or messages", err);
    }

    mapRef.current?.panTo(listing.lat, listing.lng);
    mapRef.current?.setZoom(16);
  };

  const handleSend = async () => {
    if (newMessage.trim() === "" || !selectedLocation) return;

    try {
      await axios.post("http://localhost:5004/api/messages", {
        senderId: lenderId,
        recipientId: renterId, // lender sending message to renter
        content: newMessage,
      });

      const refreshed = await axios.get(
        `http://localhost:5004/api/messages?user1=${lenderId}&user2=${renterId}`
      );
      if (refreshed.data?.data) {
        setChatMessages(refreshed.data.data);
      }

      setNewMessage("");
    } catch (err) {
      console.error("Failed to send message", err);
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
          <ListingsPanel locations={locations} onListingClick={handleListingClick} />
        </div>
        <div className="w-2/3 relative">
          <GoogleMaps ref={mapRef} locations={locations} />
        </div>
      </div>

      {modalOpen && selectedLocation && (
        <Modal
          title={`Interested in ${selectedLocation.name}`}
          onClose={() => setModalOpen(false)}
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          chatMessages={chatMessages}
          newMessage={newMessage}
          setNewMessage={setNewMessage}
          onSendMessage={handleSend}
          interestedRenters={interestedRenters}
          currentUserId={lenderId}
        />
      )}
    </div>
  );
}