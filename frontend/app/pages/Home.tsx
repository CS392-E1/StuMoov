import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";
import { auth } from "@/lib/firebase";
import { useState, useEffect } from "react";
import { getAccountStatus } from "@/lib/api";
import { StripeConnectAccount, UserRole } from "../types/user";

export default function Home() {
  const { user, logout } = useAuth();
  const userRole = user?.role;
  const navigate = useNavigate();
  const [stripeStatus, setStripeStatus] = useState<StripeConnectAccount | null>(
    null
  );
  const [isLoadingStatus, setIsLoadingStatus] = useState(false);
  const [statusError, setStatusError] = useState<string | null>(null);
  const [fetchStatusInProgress, setFetchStatusInProgress] = useState(false);

  // lol this code is so ugly but it was just for testing the stripe connect status
  // will be removed soon for the homepage re-design
  useEffect(() => {
    const fetchStatus = async () => {
      if (fetchStatusInProgress) {
        return;
      }

      if (user && user.role === UserRole.LENDER) {
        setFetchStatusInProgress(true);
        setIsLoadingStatus(true);
        setStatusError(null);
        setStripeStatus(null);
        try {
          const response = await getAccountStatus();
          if (response.data.status === 200 && response.data.data) {
            setStripeStatus(response.data.data);
          } else {
            setStatusError(
              response.data.message || "Failed to fetch Stripe status."
            );
          }
        } catch (error) {
          console.error("Error fetching Stripe status:", error);
          setStatusError("An error occurred while fetching Stripe status.");
        } finally {
          setIsLoadingStatus(false);
          setFetchStatusInProgress(false);
        }
      } else {
        setStripeStatus(null);
        setIsLoadingStatus(false);
        setStatusError(null);
      }
    };

    fetchStatus();
  }, []);

  const handleLogout = async () => {
    try {
      await logout();
      navigate("/");
    } catch (error) {
      console.error("logout error:", error);
    }
  };

  return (
    <div className="flex flex-col min-h-[calc(100vh-150px)]">
      <div className="flex-grow flex items-center justify-center bg-gray-50 py-16">
        <div className="max-w-3xl mx-auto text-center px-4">
          <h1 className="text-4xl font-bold text-gray-800 mb-6">
            Welcome to StuMoov
          </h1>

          {auth.currentUser && (
            <div className="mb-6 p-4 bg-white rounded-lg shadow-sm border border-gray-100">
              <h2 className="text-xl font-semibold mb-2">Your Account</h2>
              <p className="text-gray-600 mb-2">
                Logged in as:{" "}
                <span className="font-medium">{auth.currentUser.email}</span>
              </p>
              <p className="text-gray-600">
                Account Type:{" "}
                <span className="inline-block px-3 py-1 bg-blue-100 text-blue-800 rounded-full font-medium">
                  {user ? userRole : "Loading..."}
                </span>
              </p>
              {user && user.role === UserRole.LENDER && (
                <div className="mt-2 pt-2 border-t border-gray-200">
                  <p className="text-gray-600">
                    Stripe Account Status:{" "}
                    {isLoadingStatus ? (
                      <span className="font-medium text-gray-500">
                        Loading...
                      </span>
                    ) : statusError ? (
                      <span className="font-medium text-red-500">
                        Error: {statusError}
                      </span>
                    ) : stripeStatus ? (
                      <span className="font-medium text-green-700">
                        {stripeStatus.status} (Payouts:{" "}
                        {stripeStatus.payoutsEnabled ? "Enabled" : "Disabled"})
                      </span>
                    ) : (
                      <span className="font-medium text-gray-500">
                        Not available
                      </span>
                    )}
                  </p>
                </div>
              )}
            </div>
          )}

          <div className="flex flex-wrap gap-4 justify-center">
            <Link to="/listings">
              <button className="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors">
                Browse Listings
              </button>
            </Link>
            {auth.currentUser && (
              <button
                onClick={handleLogout}
                className="px-6 py-3 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 transition-colors"
              >
                Log Out
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
