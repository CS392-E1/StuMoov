import axios from "axios";
import { UserRole } from "@/types/user";

axios.defaults.baseURL = "http://localhost:5004/api";
axios.defaults.withCredentials = true; // Important for cookies

export function register(idToken: string, role: UserRole) {
  const endpoint =
    role === UserRole.LENDER
      ? "/auth/register/lender"
      : "/auth/register/renter";

  return axios.post(
    endpoint,
    {},
    {
      headers: {
        Authorization: `Bearer ${idToken}`,
      },
    }
  );
}

export async function login(idToken: string) {
  return axios.post(
    "/auth/login",
    {},
    {
      headers: { Authorization: `Bearer ${idToken}` },
    }
  );
}

export async function logout() {
  return axios.post("/auth/logout");
}

export async function verifyAuth() {
  return axios.get("/auth/verify", {
    withCredentials: true,
  });
}

export async function fetchUserById(userId: string) {
  return axios.get(`/user/${userId}`);
}
