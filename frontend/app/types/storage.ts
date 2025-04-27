export type StorageLocation = {
  id: string;
  lenderId: string;
  name: string;
  description: string;
  address: string;
  lat: number;
  lng: number;
  price: number | null;
  storageLength: number | null;
  storageWidth: number | null;
  storageHeight: number | null;
  imageUrl?: string | null;
  createdAt?: string;
  updatedAt?: string;
};
