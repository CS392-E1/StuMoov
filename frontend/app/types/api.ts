export interface ApiResponse<T> {
  status: number;
  message: string;
  data: T | null;
}

export interface OnboardingLinkResponse {
  url: string;
}
