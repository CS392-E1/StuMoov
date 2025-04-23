import { createContext, useReducer, useEffect, ReactNode } from "react";
import { User as FirebaseUser, UserCredential } from "firebase/auth";
import {
  auth,
  loginFirebase,
  signOutFirebase,
  signupFirebase,
} from "@/lib/firebase";
import { login, register, verifyAuth } from "@/lib/api";
import { createUserObject } from "@/lib/userUtils";
import axios from "axios";
import { User, UserRole } from "@/types/user";

// Auth state
interface AuthState {
  isAuthenticated: boolean;
  isInitialized: boolean;
  user: User | null;
}

// Initial state
const initialState: AuthState = {
  isAuthenticated: false,
  isInitialized: false,
  user: null,
};

// Auth actions
type AuthAction =
  | {
      type: "INITIALIZE";
      payload: { isAuthenticated: boolean; user: User | null };
    }
  | { type: "LOGIN"; payload: { user: User } }
  | { type: "LOGOUT" };

// Auth reducer
const reducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case "INITIALIZE":
      return {
        ...state,
        isAuthenticated: action.payload.isAuthenticated,
        isInitialized: true,
        user: action.payload.user,
      };
    case "LOGIN":
      return {
        ...state,
        isAuthenticated: true,
        user: action.payload.user,
      };
    case "LOGOUT":
      return {
        ...state,
        isAuthenticated: false,
        user: null,
      };
    default:
      return state;
  }
};

// Auth context type
export interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, role: UserRole) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

// Create context
export const AuthContext = createContext<AuthContextType>({
  ...initialState,
  login: () => Promise.resolve(),
  register: () => Promise.resolve(),
  logout: () => Promise.resolve(),
  refreshUser: () => Promise.resolve(),
});

// Props for AuthProvider
interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [state, dispatch] = useReducer(reducer, initialState);

  const handleAuthStateChanged = async (firebaseUser: FirebaseUser | null) => {
    // There's a race condition where when a user registers, it triggers
    // the auth state listener before we call our backend register endpoint.
    // This causes it to call the verify endpoint with the uid rather than the jwt.
    // This is a hack to ensure the jwt is set before the auth state listener is called.
    // TODO: Find a better solution.
    await new Promise((resolve) => setTimeout(resolve, 1000));
    if (firebaseUser) {
      try {
        // get the user ID from the JWT in cookie
        const userId = await fetchUserId();

        if (userId) {
          // Fetch user data with the ID
          const response = await axios.get(`/user/${userId}`, {
            withCredentials: true,
          });

          if (response.data && response.data.data) {
            const userData = response.data.data[0]; // User data is in an array
            const user = createUserObject(userData, firebaseUser);

            dispatch({
              type: "INITIALIZE",
              payload: { isAuthenticated: true, user },
            });
            return;
          }
        }

        // If we couldn't get user data, user is not authenticated at backend
        dispatch({
          type: "INITIALIZE",
          payload: { isAuthenticated: false, user: null },
        });
      } catch (error) {
        console.error("Failed to initialize auth state:", error);
        dispatch({
          type: "INITIALIZE",
          payload: { isAuthenticated: false, user: null },
        });
      }
    } else {
      dispatch({
        type: "INITIALIZE",
        payload: { isAuthenticated: false, user: null },
      });
    }
  };

  // Helper to fetch the user ID from the JWT
  const fetchUserId = async (): Promise<string | null> => {
    try {
      // First try to verify with the backend
      const response = await verifyAuth();

      if (response.data && response.data.userId) {
        return response.data.userId;
      }

      // If that fails but Firebase user exists, use Firebase UID as fallback
      if (auth.currentUser) {
        return auth.currentUser.uid;
      }

      return null;
    } catch (error) {
      console.error("Failed to fetch user ID from backend:", error);

      // Use Firebase user ID as fallback if available
      if (auth.currentUser) {
        return auth.currentUser.uid;
      }

      return null;
    }
  };

  // Set up auth state listener
  useEffect(() => {
    const unsubscribe = auth.onAuthStateChanged(handleAuthStateChanged);
    return () => unsubscribe();
  }, []);

  // Login with email and password
  const loginWithEmailAndPassword = async (email: string, password: string) => {
    try {
      // 1. Login with Firebase
      const userCredential: UserCredential = await loginFirebase(
        email,
        password
      );

      // 2. Get Firebase ID token
      const idToken = await userCredential.user.getIdToken();

      // 3. Call backend login endpoint (will set HttpOnly cookie)
      const response = await login(idToken);

      if (response.data && response.data.data) {
        const userData = response.data.data;
        const user = createUserObject(userData, userCredential.user);

        dispatch({
          type: "LOGIN",
          payload: { user },
        });
      }
    } catch (error) {
      console.error("Login failed:", error);
      throw error;
    }
  };

  // Register with email and password
  const registerWithEmailAndPassword = async (
    email: string,
    password: string,
    role: UserRole
  ) => {
    try {
      // 1. Register with Firebase
      const userCredential: UserCredential = await signupFirebase(
        email,
        password
      );

      // 2. Get Firebase ID token
      const idToken = await userCredential.user.getIdToken();

      // 3. Call backend register endpoint (will set HttpOnly cookie)
      const response = await register(idToken, role);

      if (response.data && response.data.data) {
        const userData = response.data.data;
        const user = createUserObject(userData, userCredential.user);

        dispatch({
          type: "LOGIN",
          payload: { user },
        });
      }
    } catch (error) {
      console.error("Registration failed:", error);
      throw error;
    }
  };

  // Prevent concurrent refreshes
  let isRefreshing = false;

  // Logout
  const logout = async () => {
    try {
      // Sign out from Firebase
      await signOutFirebase();

      // Call backend logout endpoint to clear the cookie
      await axios.post("/auth/logout", {}, { withCredentials: true });

      dispatch({ type: "LOGOUT" });
    } catch (error) {
      console.error("Logout failed:", error);
      throw error;
    }
  };

  // Refresh user data
  const refreshUser = async () => {
    // Prevent concurrent refreshes
    if (isRefreshing) {
      return;
    }

    try {
      isRefreshing = true;

      // Get user ID from current state or try to fetch it
      const userId = state.user?.id || (await fetchUserId());

      if (!userId) {
        console.error("No user ID available for refresh");
        isRefreshing = false;
        return;
      }

      try {
        // First try to fetch from backend API
        const response = await axios.get(`/user/${userId}`, {
          withCredentials: true,
        });

        if (
          response.data &&
          response.data.data &&
          response.data.data.length > 0
        ) {
          const userData = response.data.data[0];
          const user = createUserObject(userData);

          dispatch({
            type: "LOGIN",
            payload: { user },
          });
          isRefreshing = false;
          return;
        }
      } catch (apiError) {
        console.error("Error fetching from user API:", apiError);
      }

      isRefreshing = false;
    } catch (error) {
      console.error("Failed to refresh user:", error);
      isRefreshing = false;
    }
  };

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login: loginWithEmailAndPassword,
        register: registerWithEmailAndPassword,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
