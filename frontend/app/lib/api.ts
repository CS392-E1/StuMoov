import axios from "axios";

axios.defaults.baseURL = "http://localhost:5004/api";

export function setAuthToken(token: string) {
  axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
}

export function signup(idToken: string, role: "RENTER" | "LENDER") {
  return axios.post(
    "/auth/signup",
    { role },
    {
      headers: {
        Authorization: `Bearer ${idToken}`,
      },
    }
  );
}

export async function login(idToken: string) {
  return axios.post("/auth/login", undefined, {
    headers: { Authorization: `Bearer ${idToken}` },
  });
}

export async function fetchUserData(token: string): Promise<{
  data: {
    id: string;
    firebaseUid: string;
    email: string;
    displayName: string;
    role: string | number;
    isEmailVerified: boolean;
  };
  status: number;
}> {
  return axios.get("/auth/me", {
    headers: { Authorization: `Bearer ${token}` },
  });
}
