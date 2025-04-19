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

import { Plus } from "lucide-react";

interface AddListingProps {
  onAddLocation: (location: StorageLocation) => void;
}

export function AddListing({ onAddLocation }: AddListingProps) {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [newLocationName, setNewLocationName] = useState("");
  const [newLocationDesc, setNewLocationDesc] = useState("");
  const [address, setAddress] = useState("");
  const [price, setPrice] = useState("");
  const [selectedImage, setSelectedImage] = useState<File | null>(null);

  const handleCreateListing = () => {
    // TODO: Add geocoding for address -> lat/lng
    const newLocation: StorageLocation = {
      id: (Math.random() * 10000).toString(), // for now, just a random id
      name: newLocationName,
      description: newLocationDesc,
      lat: 42.36,
      lng: -71.104,
      price: Number(price) || 99,
      address: address,
      imageUrl: selectedImage
        ? URL.createObjectURL(selectedImage) // This is just to show the image for now, we will be using an actual platform to store image urls
        : "https://picsum.photos/200",
    };

    onAddLocation(newLocation);
    setDialogOpen(false);
    setNewLocationName("");
    setNewLocationDesc("");
    setAddress("");
    setPrice("");
    setSelectedImage(null);
  };

  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files[0]) {
      setSelectedImage(event.target.files[0]);
    }
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      <DialogTrigger asChild>
        <button className="absolute top-4 right-4 bg-blue-600 text-white text-2xl rounded-full shadow-lg cursor-pointer hover:bg-blue-700 transition p-2">
          <Plus />
        </button>
      </DialogTrigger>
      <DialogContent className="bg-white max-w-md">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold text-blue-600">
            Add New Listing
          </DialogTitle>
          <DialogDescription>
            Please fill out the form below to add a new listing.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
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

          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="price">Price ($ per month)</Label>
            <div className="relative">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">
                $
              </span>
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

          <div className="grid w-full items-center gap-1.5">
            <Label htmlFor="image-upload">Upload Image</Label>
            <div className="border-2 border-dashed border-input rounded-md p-4 text-center hover:bg-accent/50 transition">
              <Input
                type="file"
                accept="image/*"
                onChange={handleImageChange}
                className="hidden"
                id="image-upload"
              />
              <Label htmlFor="image-upload" className="cursor-pointer">
                {selectedImage ? (
                  <div className="flex flex-col items-center justify-center w-full h-full">
                    <div className="w-full h-24 bg-muted rounded flex items-center justify-center overflow-hidden mb-2">
                      <img
                        src={URL.createObjectURL(selectedImage)}
                        alt="Preview"
                        className="max-h-full object-cover"
                      />
                    </div>
                    <p className="text-sm text-green-600 font-medium text-center">
                      {selectedImage.name}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1 text-center">
                      Click to change
                    </p>
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center w-full h-full">
                    <svg
                      className="w-12 h-12 text-muted-foreground mx-auto"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      xmlns="http://www.w3.org/2000/svg"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth="2"
                        d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                      ></path>
                    </svg>
                    <p className="mt-2 text-sm text-foreground text-center">
                      Click to select an image
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground text-center">
                      PNG, JPG, GIF up to 10MB
                    </p>
                  </div>
                )}
              </Label>
            </div>
          </div>
        </div>

        <DialogFooter className="mx-auto">
          <div className="mt-6">
            <button
              onClick={handleCreateListing}
              className="bg-blue-600 text-white w-full p-3 rounded-md cursor-pointer hover:bg-blue-700 transition font-medium text-lg shadow-sm"
            >
              Create Listing
            </button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
