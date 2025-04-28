import { User } from "@/types/user";

// Creates a User object from backend data
export const createUserObject = (
  user: User // Backend user data
): User => {
  return {
    id: user.id,
    email: user.email,
    displayName: user.displayName,
    role: user.role,
    isAuthenticated: true,
    firebaseUid: user.firebaseUid,
    isEmailVerified: user.isEmailVerified,
    stripeConnectAccount: user.stripeConnectAccount,
  };
};
