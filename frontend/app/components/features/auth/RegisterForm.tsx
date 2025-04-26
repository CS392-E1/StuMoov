import { useState, FormEvent } from "react";
import { getOnboardingLink } from "@/lib/api";
import axios, { AxiosError } from "axios";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { UserRole } from "@/types/user";

axios.defaults.baseURL = "http://localhost:5004/api";

const RegisterForm = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [role, setRole] = useState<UserRole>(UserRole.RENTER);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const nav = useNavigate();
  const { register: registerUser } = useAuth();

  const handleSignUp = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    if (password !== confirmPassword) {
      setError("Passwords do not match");
      setLoading(false);
      return;
    }

    try {
      await registerUser(email, password, role);

      if (role === UserRole.LENDER) {
        try {
          const response = await getOnboardingLink();
          const onboardingUrl = response.data.data?.url;
          if (onboardingUrl) {
            window.location.href = onboardingUrl;
          } else {
            setError(
              "Registration successful, but failed to get onboarding link."
            );
            setLoading(false);
          }
        } catch (onboardingError) {
          console.error("Failed to get onboarding link:", onboardingError);
          setError(
            "Registration successful, but failed to get onboarding link. Please try again."
          );
          setLoading(false);
        }
      } else {
        nav("/");
      }
    } catch (err: unknown) {
      setLoading(false);

      // handle axios errors
      if (axios.isAxiosError(err)) {
        const axiosError = err as AxiosError<{ message: string }>;
        const errorMsg =
          axiosError.response?.data?.message ||
          axiosError.message ||
          "Sign up failed, please try again.";

        setError(errorMsg);
      }
      // handle firebase errors
      else if (err instanceof Error) {
        setError(err.message || "Sign up failed, please try again.");
      }
      // handle unknown errors
      else {
        setError("An unexpected error occurred. Please try again.");
      }
    }
  };

  return (
    <div className="w-full max-w-md p-6 bg-white rounded-lg shadow-md">
      <form onSubmit={handleSignUp} className="flex flex-col gap-4">
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Email"
            required
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <Input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Password"
            required
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Confirm Password</Label>
          <Input
            id="confirmPassword"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Confirm Password"
            required
          />
        </div>

        <div className="space-y-2">
          <Label>Please choose one:</Label>
          <div className="flex flex-col space-y-2">
            <label className="flex items-center space-x-2">
              <input
                type="radio"
                value={UserRole.RENTER}
                checked={role === UserRole.RENTER}
                onChange={() => setRole(UserRole.RENTER)}
                className="rounded text-blue-500 focus:ring-blue-500"
              />
              <span>I want to rent storage space</span>
            </label>
            <label className="flex items-center space-x-2">
              <input
                type="radio"
                value={UserRole.LENDER}
                checked={role === UserRole.LENDER}
                onChange={() => setRole(UserRole.LENDER)}
                className="rounded text-blue-500 focus:ring-blue-500"
              />
              <span>I want to list my storage space</span>
            </label>
          </div>
        </div>

        {error && (
          <div className="p-3 text-sm text-red-500 bg-red-50 border border-red-200 rounded-md">
            {error}
          </div>
        )}

        <button
          type="submit"
          className="py-2 px-4 w-full bg-blue-500 hover:bg-blue-600 text-white font-medium rounded-md shadow transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={loading}
        >
          {loading ? "Signing up..." : "Sign Up"}
        </button>
      </form>
    </div>
  );
};

export default RegisterForm;
