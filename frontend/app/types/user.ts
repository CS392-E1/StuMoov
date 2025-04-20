export type User = {
  id: string;
  firebaseUid: string;
  email: string;
  displayName: string;
  role: "RENTER" | "LENDER" | "ADMIN";
  isEmailVerified: boolean;
};
