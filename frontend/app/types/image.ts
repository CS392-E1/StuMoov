export type Image = {
  id: string;
  url: string;
  storageLocationId?: string | null;
  bookingId?: string | null;
  createdAt?: string;
  updatedAt?: string;
};

export type ImagePayload = {
  url: string;
  storageLocationId?: string;
  bookingId?: string;
};
