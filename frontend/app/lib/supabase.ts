import { createClient } from "@supabase/supabase-js";
import { getAuth } from "firebase/auth";

export const supabase = createClient(
  import.meta.env.VITE_SUPABASE_URL,
  import.meta.env.VITE_SUPABASE_SERVICE_ROLE_KEY,
  {
    // called before every request
    accessToken: async () => {
      const user = getAuth().currentUser;
      if (!user) return null;
      // false = use cached token until expiration
      return user.getIdToken(false);
    },
  }
);
