import { Booking } from "./booking";
import { User } from "./user";

export type Payment = {
  id: string;
  bookingId: string;
  renterId: string;
  lenderId: string;
  stripeInvoiceId?: string;
  stripePaymentIntentId: string;
  stripeChargeId?: string;
  stripeTransferId?: string;
  amountCharged: number;
  currency: string;
  platformFee: number;
  amountTransferred: number;
  status: PaymentStatus;
  createdAt: string;
  updatedAt: string;
  booking?: Booking;
  renter?: User;
  lender?: User;
};

export enum PaymentStatus {
  DRAFT = "DRAFT",
  OPEN = "OPEN",
  PAID = "PAID",
  VOID = "VOID",
  UNCOLLECTIBLE = "UNCOLLECTIBLE",
}
