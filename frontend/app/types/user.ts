export type User = {
  id: string;
  firebaseUid: string;
  email: string;
  displayName: string;
  role: UserRole;
  isEmailVerified: boolean;
  isAuthenticated: boolean;
};

export enum UserRole {
  RENTER = "RENTER",
  LENDER = "LENDER",
}
