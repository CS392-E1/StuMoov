import { User as FirebaseUser } from "firebase/auth";
import { User, UserRole } from "@/types/user";

// Helper to convert role from various formats
export const convertRole = (role: string | number): UserRole => {
  if (typeof role === "number") {
    // Role 2 is RENTER, role 1 is LENDER
    return role === 2 ? UserRole.RENTER : UserRole.LENDER;
  }

  const upperRole = String(role).toUpperCase();
  return upperRole === "LENDER" ? UserRole.LENDER : UserRole.RENTER;
};

// Creates a User object from backend data and/or Firebase user
export const createUserObject = (
  user: User, // Backend user data
  firebaseUser?: FirebaseUser | null // Optional Firebase user for fallbacks
): User => {
  return {
    id: user.id || firebaseUser?.uid || "",
    email: user.email || firebaseUser?.email || "",
    displayName:
      user.displayName || firebaseUser?.displayName || user.email || "",
    role: convertRole(user.role),
    isAuthenticated: true,
    firebaseUid: user.firebaseUid || firebaseUser?.uid || "",
    isEmailVerified:
      user.isEmailVerified || firebaseUser?.emailVerified || false,
  };
};
