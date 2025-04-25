import axios, { AxiosResponse } from "axios";
import { User, UserRole } from "@/types/user";
import { ApiResponse, OnboardingLinkResponse } from "@/types/api";

axios.defaults.baseURL = "http://localhost:5004/api";
axios.defaults.withCredentials = true; // Important for cookies

export function register(
  idToken: string,
  role: UserRole
): Promise<AxiosResponse<ApiResponse<User>>> {
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

export async function login(
  idToken: string
): Promise<AxiosResponse<ApiResponse<User>>> {
  return axios.post(
    "/auth/login",
    {},
    {
      headers: { Authorization: `Bearer ${idToken}` },
    }
  );
}

export async function logout(): Promise<AxiosResponse<ApiResponse<null>>> {
  return axios.post("/auth/logout");
}

export async function verifyAuth(): Promise<AxiosResponse<ApiResponse<User>>> {
  return axios.get("/auth/verify", {
    withCredentials: true,
  });
}

export async function fetchUserById(
  userId: string
): Promise<AxiosResponse<ApiResponse<User>>> {
  return axios.get(`/user/${userId}`);
}

export async function getOnboardingLink(): Promise<
  AxiosResponse<ApiResponse<OnboardingLinkResponse>>
> {
  return axios.get("/stripe/connect/accounts/onboarding-link", {
    withCredentials: true,
  });
}
