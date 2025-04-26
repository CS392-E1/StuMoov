import { User as FirebaseUser } from "firebase/auth";
import { User } from "@/types/user";

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
    role: user.role,
    isAuthenticated: true,
    firebaseUid: user.firebaseUid || firebaseUser?.uid || "",
    isEmailVerified:
      user.isEmailVerified || firebaseUser?.emailVerified || false,
    stripeConnectAccount: user.stripeConnectAccount,
  };
};
