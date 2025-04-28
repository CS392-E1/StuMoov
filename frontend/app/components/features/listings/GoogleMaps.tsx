import {
  GoogleMap,
  LoadScript,
  Marker,
  InfoWindow,
} from "@react-google-maps/api";
import { useRef, forwardRef, useImperativeHandle, useState } from "react";
import { StorageLocation } from "@/types/storage";
import { AddListing } from "./AddListing";
import { UserRole } from "@/types/user";
import { useAuth } from "@/hooks/use-auth";

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
  onAddLocation: (location: StorageLocation) => void;
}

export const GoogleMaps = forwardRef<GoogleMapsRef, GoogleMapsProps>(
  ({ locations, onAddLocation }, ref) => {
    const mapRef = useRef<google.maps.Map | null>(null);
    const [selectedLocation, setSelectedLocation] =
      useState<StorageLocation | null>(null);
    const { user } = useAuth();

    useImperativeHandle(ref, () => ({
      panTo: (lat: number, lng: number) => {
        mapRef.current?.panTo({ lat, lng });
      },
      setZoom: (zoom: number) => {
        mapRef.current?.setZoom(zoom);
      },
    }));

    return (
      <div className="h-[calc(100vh-150px)] w-full overflow-hidden relative">
        {user?.role === UserRole.LENDER && (
          <AddListing onAddLocation={onAddLocation} />
        )}

        <LoadScript
          googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY!}
        >
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
                }}
              />
            ))}

            {selectedLocation && (
              <InfoWindow
                position={{
                  lat: selectedLocation.lat,
                  lng: selectedLocation.lng,
                }}
                onCloseClick={() => setSelectedLocation(null)}
              >
                <div className="p-2">
                  <h3 className="font-semibold">{selectedLocation.name}</h3>
                  <p>{selectedLocation.description}</p>
                  <p className="text-sm mt-1 text-gray-600">
                    {selectedLocation.address}
                  </p>
                  <p className="text-sm mt-1 text-gray-600">
                    ${selectedLocation.price}/month
                  </p>
                </div>
              </InfoWindow>
            )}
          </GoogleMap>
        </LoadScript>
      </div>
    );
  }
);
