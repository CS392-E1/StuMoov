import { User } from "./user";
import { StorageLocation } from "./storage";
import { Payment } from "./payment";

export enum BookingStatus {
  PENDING = "PENDING",
  CONFIRMED = "CONFIRMED",
  CANCELLED = "CANCELLED",
}

export type Booking = {
  id: string;
  renterId: string;
  storageLocationId: string;
  paymentId: string | null;
  startDate: string;
  endDate: string;
  totalPrice: number | null;
  status: BookingStatus;
  createdAt: string;
  updatedAt: string;
  renter?: User;
  payment?: Payment;
  storageLocation?: StorageLocation;
};
