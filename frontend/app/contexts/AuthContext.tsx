import {
  createContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from "react";
import { onAuthStateChanged, User as FirebaseUser } from "firebase/auth";
import { auth } from "@/lib/firebase";
import { User } from "@/types/user";
import { fetchUserData as apiGetUserData } from "@/lib/api";

// Simple function to convert numeric roles to string values
export function convertRole(role: number | string): User["role"] {
  if (typeof role === "number") {
    return role === 1 ? "LENDER" : "RENTER";
  }

  if (typeof role === "string") {
    // Handle numeric strings
    const numRole = parseInt(role, 10);
    if (!isNaN(numRole)) {
      return numRole === 1 ? "LENDER" : "RENTER";
    }

    // Return valid role strings directly
    if (role === "LENDER" || role === "RENTER") {
      return role;
    }
  }

  // Default fallback
  return "RENTER";
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  refreshUser: () => Promise<User | null>;
}

export const AuthContext = createContext<AuthContextType>({
  user: null,
  loading: true,
  error: null,
  refreshUser: async () => null,
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // function to fetch user data from backend
  const fetchUserData = useCallback(async (firebaseUser: FirebaseUser) => {
    try {
      // check for local storage data and use it immediately
      const storedUser = localStorage.getItem("stuMoov_user");
      const storedRole = localStorage.getItem("stuMoov_role");

      if (storedUser && storedRole) {
        const parsedUser = JSON.parse(storedUser);
        const roleString = convertRole(storedRole);

        // set user immediately with stored data
        setUser({
          id: parsedUser.id || firebaseUser.uid,
          firebaseUid: firebaseUser.uid,
          email: firebaseUser.email || "",
          displayName: parsedUser.displayName || firebaseUser.email || "",
          role: roleString,
          isEmailVerified: firebaseUser.emailVerified,
        });
      } else {
        // no local storage data, set basic firebase user initially
        setUser({
          id: firebaseUser.uid,
          firebaseUid: firebaseUser.uid,
          email: firebaseUser.email || "",
          displayName: firebaseUser.displayName || firebaseUser.email || "",
          role: "RENTER",
          isEmailVerified: firebaseUser.emailVerified,
        });
      }

      // then try to fetch from backend for verification/updates
      const appJwt = localStorage.getItem("stuMoov_jwt");

      if (!appJwt) {
        const idToken = await firebaseUser.getIdToken();
        try {
          const response = await apiGetUserData(idToken);

          if (response.status === 200) {
            const userData = response.data;
            const roleString = convertRole(userData.role);

            // update with backend data
            setUser((prev: User | null) =>
              prev
                ? {
                    ...prev,
                    role: roleString,
                    displayName: userData.displayName || prev.displayName,
                  }
                : null
            );
          }
        } catch (error) {
          console.error("Error fetching user data:", error);
        }
      } else {
        try {
          const response = await apiGetUserData(appJwt);

          if (response.status === 200) {
            const userData = response.data;
            const roleString = convertRole(userData.role);

            // update with backend data
            setUser((prev: User | null) =>
              prev
                ? {
                    ...prev,
                    role: roleString,
                    displayName: userData.displayName || prev.displayName,
                  }
                : null
            );
          }
        } catch (error) {
          console.error("Error fetching user data:", error);
        }
      }
    } catch (error) {
      // make sure we at least set the basic firebase user
      console.error(
        "Error processing user data:",
        error instanceof Error ? error.message : String(error)
      );
      if (!user) {
        setUser({
          id: firebaseUser.uid,
          firebaseUid: firebaseUser.uid,
          email: firebaseUser.email || "",
          displayName: firebaseUser.displayName || firebaseUser.email || "",
          role: "RENTER",
          isEmailVerified: firebaseUser.emailVerified,
        });
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    try {
      const unsubscribe = onAuthStateChanged(auth, (firebaseUser) => {
        if (firebaseUser) {
          fetchUserData(firebaseUser);
        } else {
          setUser(null);
          setLoading(false);
        }
      });

      return () => unsubscribe();
    } catch (error) {
      setError(error as string);
      setLoading(false);
    }
  }, [fetchUserData]);

  const refreshUser = async () => {
    if (auth.currentUser) {
      await fetchUserData(auth.currentUser);
      return user;
    } else {
      // try to use local storage data when no firebase user is available
      const storedUser = localStorage.getItem("stuMoov_user");
      const storedRole = localStorage.getItem("stuMoov_role");

      if (storedUser && storedRole) {
        try {
          const parsedUser = JSON.parse(storedUser);
          const roleString = convertRole(storedRole);

          // create from local storage
          const localUser: User = {
            id: parsedUser.id || parsedUser.uid,
            firebaseUid: parsedUser.uid,
            email: parsedUser.email || "",
            displayName: parsedUser.displayName || parsedUser.email || "",
            role: roleString,
            isEmailVerified: parsedUser.emailVerified || false,
          };

          // update user state and return
          setUser(localUser);
          return localUser;
        } catch (error) {
          // error parsing stored user
          console.error(
            "Error parsing stored user data:",
            error instanceof Error ? error.message : String(error)
          );
          setError("Failed to load user data from storage");
        }
      }
    }
    return null;
  };

  return (
    <AuthContext.Provider value={{ user, loading, error, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}
