import { GoogleMap, LoadScript, Marker, InfoWindow } from "@react-google-maps/api";
import { useRef, forwardRef, useImperativeHandle, useState } from "react";
import { StorageLocation } from "@/types/storage";
import { ChatPopup } from "./ChatPopUp"; // <-- import the chat popup

const containerStyle = {
  width: "100%",
  height: "100%",
};

const defaultCenter = {
  lat: 42.3505,
  lng: -71.1054,
};

export interface GoogleMapsRef {
  panTo: (lat: number, lng: number) => void;
  setZoom: (zoom: number) => void;
}

interface GoogleMapsProps {
  locations: StorageLocation[];
}

export const GoogleMaps = forwardRef<GoogleMapsRef, GoogleMapsProps>(
  ({ locations }, ref) => {
    const mapRef = useRef<google.maps.Map | null>(null);
    const [selectedLocation, setSelectedLocation] = useState<StorageLocation | null>(null);
    const [chatOpen, setChatOpen] = useState(false);

    useImperativeHandle(ref, () => ({
      panTo: (lat: number, lng: number) => {
        mapRef.current?.panTo({ lat, lng });
      },
      setZoom: (zoom: number) => {
        mapRef.current?.setZoom(zoom);
      },
    }));

    return (
      <div className="h-[calc(100vh-150px)] w-full overflow-hidden">
        <LoadScript googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY!}>
          <GoogleMap
            mapContainerStyle={containerStyle}
            center={defaultCenter}
            zoom={15}
            onLoad={(map) => {
              mapRef.current = map;
            }}
          >
            {locations.map((location) => (
              <Marker
                key={location.id}
                position={{ lat: location.lat, lng: location.lng }}
                onClick={() => {
                  console.log("Messaging lenderId:", location.lenderId);
                  setSelectedLocation(location);
                  setChatOpen(false); // close chat when opening info window
                }}
              />
            ))}

            {selectedLocation && (
              <InfoWindow
                position={{ lat: selectedLocation.lat, lng: selectedLocation.lng }}
                onCloseClick={() => setSelectedLocation(null)}
              >
                <div className="p-2">
                  <h3 className="font-semibold">{selectedLocation.name}</h3>
                  <p>{selectedLocation.description}</p>
                  <p className="text-sm mt-1 text-gray-600">{selectedLocation.address}</p>
                  <p className="text-sm mt-1 text-gray-600">${selectedLocation.price}/month</p>
                  <button
                    className="mt-2 bg-blue-600 text-white px-2 py-1 rounded"
                    onClick={() => {
                      setChatOpen(true);
                    }}
                  >
                    I'm Interested
                  </button>
                </div>
              </InfoWindow>
            )}
          </GoogleMap>
        </LoadScript>

        {chatOpen && selectedLocation && (
          <ChatPopup
            receiver={selectedLocation.lenderId} 
            onClose={() => setChatOpen(false)}
          />
        )}
      </div>
    );
  }
);
