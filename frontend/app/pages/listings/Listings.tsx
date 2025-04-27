// import { useEffect, useRef, useState } from "react";
// import axios from "axios";
// import { GoogleMaps, GoogleMapsRef } from "@/components/features/listings/GoogleMaps";
// import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
// import { SearchBar } from "@/components/features/listings/SearchBar";
// import { StorageLocation } from "@/types/storage";

// export default function Listings() {
//   const mapRef = useRef<GoogleMapsRef>(null);
//   const [locations, setLocations] = useState<StorageLocation[]>([]);

//   useEffect(() => {
//     axios.get("http://localhost:5004/api/storage")
//       .then((res) => {
//         if (res.data?.data) {
//           setLocations(res.data.data);
//         }
//       })
//       .catch((err) => {
//         console.error("Failed to fetch storage locations", err);
//       });
//   }, []);

//   const handleListingClick = (lat: number, lng: number) => {
//     mapRef.current?.panTo(lat, lng);
//     mapRef.current?.setZoom(16);
//   };

//   const handleSearch = (query: string) => {
//     console.log("Searching for:", query);
//     // you can add search filtering later if you want
//   };

//   return (
//     <div className="flex flex-col h-[calc(100vh-150px)] w-full">
//       <div className="flex flex-1 w-full">
//         <div className="w-1/3 p-4 overflow-y-auto">
//           <SearchBar onSearch={handleSearch} />
//           <ListingsPanel locations={locations} onListingClick={handleListingClick} />
//         </div>
//         <div className="w-2/3 relative">
//           <GoogleMaps ref={mapRef} locations={locations} />
//         </div>
//       </div>
//     </div>
//   );
// }

import { useEffect, useRef, useState } from "react";
import axios from "axios";
import { GoogleMaps, GoogleMapsRef } from "@/components/features/listings/GoogleMaps";
import { ListingsPanel } from "@/components/features/listings/ListingsPanel";
import { SearchBar } from "@/components/features/listings/SearchBar";
import { StorageLocation } from "@/types/storage";

export default function Listings() {
  const mapRef = useRef<GoogleMapsRef>(null);
  const [locations, setLocations] = useState<StorageLocation[]>([]);

  useEffect(() => {
    axios.get("http://localhost:5004/api/storage")
      .then((res) => {
        if (res.data?.data) {
          const filledLocations = res.data.data.map((loc: any) => ({
            ...loc,
            address: loc.address || "No address provided",
            imageUrl: loc.imageUrl || "https://picsum.photos/200", // fallback mock image
          }));
          setLocations(filledLocations);
        }
      })
      .catch((err) => {
        console.error("Failed to fetch storage locations", err);
      });
  }, []);

  const handleListingClick = (location: StorageLocation) => {
    console.log("Pan to:", location.lat, location.lng);
    if (typeof location.lat === "number" && typeof location.lng === "number") {
      mapRef.current?.panTo(location.lat, location.lng);
      mapRef.current?.setZoom(16);
    } else {
      console.warn("Invalid lat/lng for listing:", location);
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
    </div>
  );
}