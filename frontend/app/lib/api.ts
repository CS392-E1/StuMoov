import axios, { AxiosResponse } from "axios";
import {
  User,
  UserRole,
  StripeConnectAccount,
  StripeCustomer,
} from "@/types/user";
import {
  ApiResponse,
  OnboardingLinkResponse,
  VerifyResponse,
} from "@/types/api";
import { StorageLocation } from "@/types/storage";
import { Session, Message } from "@/types/chat";
import { Booking } from "@/types/booking";
import { Image, ImagePayload } from "@/types/image";

axios.defaults.baseURL = "http://localhost:5004/api";
axios.defaults.withCredentials = true; // Important for cookies

// TODO: Move these to their own dedicated files
//these functions all refer to the backend, using associated models and controllers


//functions related to register, login
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

//authentication, and fetching users
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

//stripe calls
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

//gradding storagelocations, using various methods, id, userid, etc
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

export async function getStorageLocationsByCoordinates(
  lat: number,
  lng: number,
  radius: number = 10.0 // Default radius is 10 km
): Promise<AxiosResponse<ApiResponse<StorageLocation[]>>> {
  return axios.get(`/storage/nearby`, {
    params: { lat, lng, radiusKm: radius }, // Send lat, lng, and radius as query parameters
  });
}

export async function getStorageLocationsByDimensions(
  length?: number,
  width?: number,
  height?: number
): Promise<AxiosResponse<ApiResponse<StorageLocation[]>>> {
  try {
    const params: { length?: number; width?: number; height?: number } = {}; // Explicitly type params
    if (length) params.length = length;
    if (width) params.width = width;
    if (height) params.height = height;

    return axios.get("/storage/dimensions", { params });
  } catch (error) {
    console.error("Error fetching storage locations by dimensions:", error);
    throw error;
  }
}

export async function getStorageLocationsByPrice(
  maxPrice: number
): Promise<AxiosResponse<ApiResponse<StorageLocation[]>>> {
  try {
    return axios.get("/storage/price", { params: { maxPrice } });
  } catch (error) {
    console.error("Error fetching storage locations by price:", error);
    throw error;
  }
}

export async function getStorageLocationsByCapacity(
  volume: number
): Promise<AxiosResponse<ApiResponse<StorageLocation[]>>> {
  try {
    return axios.get("/storage/capacity", { params: { volume } });
  } catch (error) {
    console.error("Error fetching storage locations by capacity:", error);
    throw error;
  }
}

//create and update storage
export async function createStorageLocation(
  // Update type to include imageUrl and exclude only generated fields
  storageData: Omit<StorageLocation, "id" | "createdAt" | "updatedAt">
): Promise<AxiosResponse<ApiResponse<StorageLocation>>> {
  return axios.post("/storage", storageData);
}

export async function updateStorageLocation(
  id: string,
  storageLocation: StorageLocation
): Promise<AxiosResponse<ApiResponse<StorageLocation>>> {
  return axios.put(`/storage/${id}`, storageLocation);
}

//messaging related functions, session grabs, session creates and messages
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
  });
}

//booking functions, as well as image related work
export async function createBooking(
  booking: Booking
): Promise<AxiosResponse<ApiResponse<Booking>>> {
  return axios.post("/bookings", booking, { withCredentials: true });
}

//image related functions
export async function uploadStorageImage(
  imageUrl: string,
  storageLocationId: string
): Promise<AxiosResponse<ApiResponse<Image>>> {
  const payload: ImagePayload = { url: imageUrl, storageLocationId };
  return axios.post("/image/storage", payload, { withCredentials: true });
}

export async function uploadDropoffImage(
  imageUrl: string,
  bookingId: string
): Promise<AxiosResponse<ApiResponse<Image>>> {
  const payload: ImagePayload = { url: imageUrl, bookingId };
  return axios.post("/image/dropoff", payload, { withCredentials: true });
}

export async function getImagesByBookingId(
  bookingId: string
): Promise<AxiosResponse<ApiResponse<Image[]>>> {
  return axios.get(`/image/booking/${bookingId}`, { withCredentials: true });
}

export async function getImagesByStorageLocationId(
  storageLocationId: string
): Promise<AxiosResponse<ApiResponse<Image[]>>> {
  return axios.get(`/image/storage/${storageLocationId}`, {
    withCredentials: true,
  });
}

export async function createStripeCustomer(): Promise<
  AxiosResponse<ApiResponse<StripeCustomer>>
> {
  return axios.post("/stripe/customers", {}, { withCredentials: true });
}

//bookings and invoice related functions
export async function confirmBooking(
  bookingId: string
): Promise<AxiosResponse<ApiResponse<Booking>>> {
  return axios.put(
    `/bookings/${bookingId}/confirm`,
    {},
    { withCredentials: true }
  );
}

export async function getInvoiceUrl(
  paymentId: string
): Promise<AxiosResponse<ApiResponse<string>>> {
  return axios.get(`/payments/${paymentId}/invoice-url`, {
    withCredentials: true,
  });
}
