import { useState, FormEvent } from "react";
import { signInWithEmailAndPassword, UserCredential } from "firebase/auth";
import { auth } from "@/lib/firebase";
import axios, { AxiosError } from "axios";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { login } from "@/lib/api";

const LoginForm = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const nav = useNavigate();
  const { refreshUser } = useAuth();

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      // 1) sign in with Firebase
      const cred: UserCredential = await signInWithEmailAndPassword(
        auth,
        email,
        password
      );

      // 2) get ID token
      const idToken = await cred.user.getIdToken(true);

      // 3) call backend login
      await login(idToken);

      // 4) refresh user state
      await refreshUser();

      // 5) redirect
      nav("/");
    } catch (err: unknown) {
      setLoading(false);

      // handle axios errors
      if (axios.isAxiosError(err)) {
        const axiosError = err as AxiosError<{ message: string }>;
        const errorMsg =
          axiosError.response?.data?.message ||
          axiosError.message ||
          "Login failed, please try again.";

        setError(errorMsg);
      }
      // handle firebase errors
      else if (err instanceof Error) {
        setError(err.message || "Login failed, please try again.");
      }
      // handle unknown errors
      else {
        setError("An unexpected error occurred. Please try again.");
      }
    }
  };

  return (
    <div className="w-full max-w-md p-6 bg-white rounded-lg shadow-md">
      <form onSubmit={handleLogin} className="flex flex-col gap-4">
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
          {loading ? "Logging in..." : "Log In"}
        </button>
      </form>
    </div>
  );
};

export default LoginForm;
