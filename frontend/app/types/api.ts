export type ApiResponse<T> = {
  status: number;
  message: string;
  data: T | null;
};

export type OnboardingLinkResponse = {
  url: string;
};

export type VerifyResponse = {
  userId: string;
};
