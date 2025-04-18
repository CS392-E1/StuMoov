import { GoogleMap, LoadScript, Marker } from "@react-google-maps/api";
import { useRef, forwardRef, useImperativeHandle } from "react";

// TODO: Place this in a types/ directory
export type StorageLocation = {
  id: string;
  name: string;
  description: string;
  lat: number;
  lng: number;
  price: number;
  image: string;
};

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
              />
            ))}
          </GoogleMap>
        </LoadScript>
      </div>
    );
  }
);
