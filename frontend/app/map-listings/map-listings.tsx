import { GoogleMap, LoadScript, Marker } from "@react-google-maps/api"
import { useEffect, useRef, useState } from "react"
import { Search, Plus, MoreVertical } from "lucide-react"

type StorageLocation = {
  id: string
  name: string
  description: string
  lat: number
  lng: number
  price: number
}

const containerStyle = {
  width: "100%",
  height: "100%",
}

const defaultCenter = {
  lat: 42.3505,
  lng: -71.1054,
}

export default function MapListings() {
  const mapRef = useRef<google.maps.Map | null>(null)
  const [locations, setLocations] = useState<StorageLocation[]>([])

  useEffect(() => {
    fetch("http://localhost:5004/api/StorageLocation")
      .then((res) => res.json())
      .then((data) => {
        if (data?.data) setLocations(data.data)
      })
      .catch((err) => console.error("Failed to fetch locations", err))
  }, [])

  const handleListingClick = (lat: number, lng: number) => {
    mapRef.current?.panTo({ lat, lng })
    mapRef.current?.setZoom(16)
  }

  return (
    <div className="flex flex-col h-screen w-full relative">
      <div className="flex-1 relative">
        <LoadScript googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY!}>
          <GoogleMap
            mapContainerStyle={containerStyle}
            center={defaultCenter}
            zoom={15}
            onLoad={(map) => {
              mapRef.current = map
            }}
          >
            {locations.map((loc) => (
              <Marker
                key={loc.id}
                position={{ lat: loc.lat, lng: loc.lng }}
              />
            ))}
          </GoogleMap>
        </LoadScript>

        {/* Controls */}
        <div className="absolute top-4 left-4 z-[1000]">
          <button className="bg-white p-3 rounded-md shadow-md">
            <MoreVertical className="w-5 h-5 text-gray-700" />
          </button>
        </div>
        <div className="absolute top-20 left-4 z-[1000]">
          <button className="bg-white p-3 rounded-md shadow-md">
            <Search className="w-5 h-5 text-gray-700" />
          </button>
        </div>
        <div className="absolute top-4 right-4 z-[1000]">
          <button className="bg-white p-3 rounded-md shadow-md">
            <Plus className="w-5 h-5 text-gray-700" />
          </button>
        </div>
      </div>

      {/* Listings Panel */}
      <div className="bg-gray-200 p-4 rounded-lg mx-4 mb-4">
        <h2 className="text-xl font-bold mb-4">Current Listings:</h2>
        <div className="space-y-3">
          {locations.map((loc) => (
            <div
              key={loc.id}
              className="bg-white p-3 rounded-full text-center shadow-sm cursor-pointer hover:bg-blue-100 transition"
              onClick={() => handleListingClick(loc.lat, loc.lng)}
            >
              {loc.name}: ${loc.price}/month
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}