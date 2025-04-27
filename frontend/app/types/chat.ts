import { User } from "./user";

export type Message = {
  id: string;
  chatSessionId: string;
  senderId: string;
  recipientId: string;
  content: string;
  sentAt: string;
};

export type Session = {
  id: string;
  renterId: string;
  lenderId: string;
  renter?: User;
  lender?: User;
  bookingId?: string | null;
  storageLocationId?: string | null;
  createdAt: string;
  updatedAt: string;
};
