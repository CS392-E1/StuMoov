export type StorageLocation = {
  id: string;
  lenderId: string;
  name: string;
  description: string;
  address: string;
  lat: number;
  lng: number;
  price: number | null;
  length: number | null;
  width: number | null;
  height: number | null;
  imageUrl: string | null;
  createdAt: string;
  updatedAt: string;
};
