import { useContext } from "react";
import { auth } from "@/lib/firebase";
import { AuthContext } from "@/contexts/AuthContext";
import { User } from "@/types/user";
export function useAuth() {
  const context = useContext(AuthContext);

  // if context has no user but localstorage has data, provide a user with role
  if (!context.user) {
    const storedUser = localStorage.getItem("stuMoov_user");
    const storedRole = localStorage.getItem("stuMoov_role");

    if (storedUser && storedRole && auth.currentUser) {
      // we have both localstorage data and firebase auth
      const parsedUser = JSON.parse(storedUser);
      const roleString = storedRole as User["role"];

      // return enhanced context
      return {
        ...context,
        user: {
          id: parsedUser.id || auth.currentUser.uid,
          firebaseUid: auth.currentUser.uid,
          email: auth.currentUser.email || "",
          displayName: parsedUser.displayName || auth.currentUser.email || "",
          role: roleString,
          isEmailVerified: auth.currentUser.emailVerified,
        },
      };
    }
  }

  return context;
}
