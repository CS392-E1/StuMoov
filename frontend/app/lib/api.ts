import axios, { AxiosResponse } from "axios";
import { User, UserRole, StripeConnectAccount } from "@/types/user";
import {
  ApiResponse,
  OnboardingLinkResponse,
  VerifyResponse,
} from "@/types/api";
import { StorageLocation } from "@/types/storage";
import { Session, Message } from "@/types/chat";
import { Booking } from "@/types/booking";

axios.defaults.baseURL = "http://localhost:5004/api";
axios.defaults.withCredentials = true; // Important for cookies

// TODO: Move these to their own dedicated files

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
  return axios.post("/auth/logout", {}, { withCredentials: true });
}

export async function verifyAuth(): Promise<
  AxiosResponse<ApiResponse<VerifyResponse>>
> {
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

export async function getAccountStatus(): Promise<
  AxiosResponse<ApiResponse<StripeConnectAccount>>
> {
  return axios.get("/stripe/connect/accounts/status", {
    withCredentials: true,
  });
}

export async function getStorageLocations(): Promise<
  AxiosResponse<ApiResponse<StorageLocation[]>>
> {
  return axios.get("/storage");
}

export async function getStorageLocationById(
  id: string
): Promise<AxiosResponse<ApiResponse<StorageLocation>>> {
  return axios.get(`/storage/${id}`);
}

export async function getStorageLocationsByUserId(
  userId: string
): Promise<AxiosResponse<ApiResponse<StorageLocation[]>>> {
  return axios.get(`/storage/user/${userId}`);
}

export async function createStorageLocation(
  storageData: Omit<
    StorageLocation,
    "id" | "createdAt" | "updatedAt" | "imageUrl"
  >
): Promise<AxiosResponse<ApiResponse<StorageLocation>>> {
  return axios.post("/storage", storageData);
}

export async function updateStorageLocation(
  id: string,
  storageLocation: StorageLocation
): Promise<AxiosResponse<ApiResponse<StorageLocation>>> {
  return axios.put(`/storage/${id}`, storageLocation);
}

export async function getMySessions(): Promise<
  AxiosResponse<ApiResponse<Session[]>>
> {
  return axios.get("/chat/sessions", { withCredentials: true });
}

export async function getMessagesBySessionId(
  sessionId: string
): Promise<AxiosResponse<ApiResponse<Message[]>>> {
  return axios.get(`/chat/sessions/${sessionId}/messages`, {
    withCredentials: true,
  });
}

export async function createSession(
  renterId: string,
  lenderId: string,
  storageLocationId: string
): Promise<AxiosResponse<ApiResponse<Session>>> {
  return axios.post(
    "/chat/sessions",
    { renterId, lenderId, storageLocationId },
    { withCredentials: true }
  );
}

export async function sendMessage(
  sessionId: string,
  content: string
): Promise<AxiosResponse<ApiResponse<Message>>> {
  return axios.post(
    `/chat/sessions/${sessionId}/messages`,
    { content },
    { withCredentials: true }
  );
}

export async function getBookingsByStorageLocationId(
  storageLocationId: string
): Promise<AxiosResponse<ApiResponse<Booking[]>>> {
  return axios.get(`/bookings/storage/${storageLocationId}`, {
    withCredentials: true,
  });
}

export async function getSessionByParticipants(
  renterId: string,
  lenderId: string,
  storageLocationId: string
): Promise<AxiosResponse<ApiResponse<Session>>> {
  return axios.get(`/chat/sessions/participants`, {
    params: { renterId, lenderId, storageLocationId },
    withCredentials: true,
    validateStatus: function (status) {
      return (status >= 200 && status < 300) || status === 404;
    },
  });
}
