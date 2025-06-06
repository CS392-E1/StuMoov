import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { StorageLocation } from "@/types/storage";
import { useAuth } from "@/hooks/use-auth";
import { Plus } from "lucide-react";
import { createStorageLocation, uploadStorageImage } from "@/lib/api";
import { toast } from "sonner";
import { useGeocoding } from "@/hooks/use-geocoding";
import ImageUpload from "./ImageUpload";

type AddListingProps = {
  onAddLocation: (location: StorageLocation) => void;
};

export function AddListing({ onAddLocation }: AddListingProps) {
  // State for controlling dialog open/close
  const [dialogOpen, setDialogOpen] = useState(false);

  // Form input states
  const [newLocationName, setNewLocationName] = useState("");
  const [newLocationDesc, setNewLocationDesc] = useState("");
  const [address, setAddress] = useState("");
  const [price, setPrice] = useState("");
  const [length, setLength] = useState("");
  const [width, setWidth] = useState("");
  const [height, setHeight] = useState("");

  const { user } = useAuth(); // Get current user info
  const { geocodeAddress, isLoading: isGeocoding } = useGeocoding(); // Custom hook for geocoding
  const [imageUrl, setImageUrl] = useState<string | null>(null); // Uploaded image URL

  const handleCreateListing = async () => {
    if (!user) {
      toast.error("You must be logged in to create a listing.");
      return;
    }

    // Geocode the address to get lat/lng coordinates
    let coords: { lat: number; lng: number };
    try {
      coords = await geocodeAddress(address);
      toast.info(`Geocoded address: ${coords.lat.toFixed(4)}, ${coords.lng.toFixed(4)}`);
    } catch (geoError: unknown) {
      const errorMsg =
        typeof geoError === "object" &&
        geoError !== null &&
        "message" in geoError
          ? String(geoError.message)
          : "Failed to get coordinates for the address.";
      toast.error(`Geocoding Error: ${errorMsg}`);
      return;
    }

    // Prepare the payload for the new listing
    const newLocationData = {
      lenderId: user.id,
      name: newLocationName,
      description: newLocationDesc,
      lat: coords.lat,
      lng: coords.lng,
      price: Number(price) || null,
      address: address,
      storageLength: Number(length) || null,
      storageWidth: Number(width) || null,
      storageHeight: Number(height) || null,
      imageUrl: imageUrl,
    };

    const loadingToastId = toast.loading("Creating listing...");

    try {
      // API call to create the listing
      const response = await createStorageLocation(newLocationData);

      if (response.status >= 200 && response.status < 300 && response.data?.data) {
        const createdListing = Array.isArray(response.data.data)
          ? response.data.data[0]
          : response.data.data;

        if (createdListing && createdListing.id) {
          toast.success("Listing created successfully!", { id: loadingToastId });

          // Optional: upload image to listing if exists
          if (imageUrl) {
            try {
              await uploadStorageImage(imageUrl, createdListing.id);
              toast.info("Listing image associated successfully.");
            } catch (imgError) {
              toast.error("Listing created, but failed to associate image. You may need to re-upload.");
            }
          }

          // Call parent callback with new listing
          onAddLocation(createdListing);

          // Reset form and close dialog
          setDialogOpen(false);
          setNewLocationName("");
          setNewLocationDesc("");
          setAddress("");
          setPrice("");
          setLength("");
          setWidth("");
          setHeight("");
          setImageUrl(null);
        } else {
          toast.error("Received unexpected data from server.", { id: loadingToastId });
        }
      } else {
        const errorMsg =
          response.data?.message || `Failed with status code ${response.status}`;
        toast.error(`Failed to create listing: ${errorMsg}`, { id: loadingToastId });
      }
    } catch (err: unknown) {
      const errorMsg =
        err instanceof Error ? err.message : "An unexpected error occurred.";
      toast.error(`Failed to create listing: ${errorMsg}`, { id: loadingToastId });
    }
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      {/* Trigger Button */}
      <DialogTrigger asChild>
        <button className="absolute top-4 right-4 z-10 bg-blue-600 text-white text-2xl rounded-full shadow-lg cursor-pointer hover:bg-blue-700 transition p-2">
          <Plus />
        </button>
      </DialogTrigger>

      {/* Dialog Content */}
      <DialogContent className="bg-white max-w-md">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold text-blue-600">Add New Listing</DialogTitle>
          <DialogDescription>Please fill out the form below to add a new listing.</DialogDescription>
        </DialogHeader>

        {/* Form Fields */}
        <div className="space-y-4">
          {/* Listing Name */}
          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="listingName">Listing Name</Label>
            <Input
              id="listingName"
              type="text"
              placeholder="e.g., Spacious Storage in Cambridge"
              value={newLocationName}
              onChange={(e) => setNewLocationName(e.target.value)}
            />
          </div>

          {/* Description */}
          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              placeholder="Describe your space in detail..."
              value={newLocationDesc}
              onChange={(e) => setNewLocationDesc(e.target.value)}
              className="h-32 resize-none"
            />
          </div>

          {/* Address */}
          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="address">Address</Label>
            <Input
              id="address"
              type="text"
              placeholder="123 Main St, Boston, MA"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
            />
          </div>

          {/* Price */}
          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="price">Price ($ per month)</Label>
            <div className="relative">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">$</span>
              <Input
                id="price"
                type="number"
                placeholder="99"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                className="pl-8"
                min="0"
              />
            </div>
          </div>

          {/* Dimensions: Length / Width / Height */}
          <div className="grid grid-cols-3 gap-4">
            <div className="grid w-full items-center gap-1.5">
              <Label htmlFor="length">Length (ft)</Label>
              <Input
                id="length"
                type="number"
                placeholder="e.g., 10"
                value={length}
                onChange={(e) => setLength(e.target.value)}
                min="0"
              />
            </div>
            <div className="grid w-full items-center gap-1.5">
              <Label htmlFor="width">Width (ft)</Label>
              <Input
                id="width"
                type="number"
                placeholder="e.g., 8"
                value={width}
                onChange={(e) => setWidth(e.target.value)}
                min="0"
              />
            </div>
            <div className="grid w-full items-center gap-1.5">
              <Label htmlFor="height">Height (ft)</Label>
              <Input
                id="height"
                type="number"
                placeholder="e.g., 8"
                value={height}
                onChange={(e) => setHeight(e.target.value)}
                min="0"
              />
            </div>
          </div>

          {/* Image Upload */}
          <ImageUpload onUploadSuccess={setImageUrl} />
        </div>

        {/* Submit Button */}
        <DialogFooter className="mx-auto">
          <div className="mt-6">
            <button
              onClick={handleCreateListing}
              disabled={isGeocoding}
              className="bg-blue-600 text-white w-full p-3 rounded-md cursor-pointer hover:bg-blue-700 transition font-medium text-lg shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isGeocoding ? "Geocoding..." : "Create Listing"}
            </button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}