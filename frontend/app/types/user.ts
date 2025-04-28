export enum StripeConnectAccountStatus {
  RESTRICTED = "RESTRICTED",
  PENDING = "PENDING",
  COMPLETED = "COMPLETED",
}

export type StripeConnectAccount = {
  id: string;
  userId: string;
  stripeConnectAccountId: string;
  status: StripeConnectAccountStatus;
  payoutsEnabled: boolean;
  accountLinkUrl?: string | null;
  createdAt: string;
  updatedAt: string;
};

export type StripeCustomer = {
  id: string;
  userId: string;
  stripeCustomerId: string | null;
  defaultPaymentMethodId: string | null;
  createdAt: string;
  updatedAt: string;
};

export type User = {
  id: string;
  firebaseUid: string;
  email: string;
  displayName: string;
  role: UserRole;
  isEmailVerified: boolean;
  isAuthenticated: boolean;
  stripeConnectAccount?: StripeConnectAccount | null;
  stripeCustomer?: StripeCustomer | null;
};

export enum UserRole {
  RENTER = "RENTER",
  LENDER = "LENDER",
}
