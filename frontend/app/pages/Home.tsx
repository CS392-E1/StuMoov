import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth"; // Custom hook for auth context
import { auth } from "@/lib/firebase"; // Firebase instance
import { useState, useEffect } from "react";
import { getAccountStatus } from "@/lib/api"; // API call to fetch Stripe status
import { StripeConnectAccount, UserRole } from "../types/user"; // Types

export default function Home() {
  const { user, logout } = useAuth(); // Access current user and logout function from auth context
  const userRole = user?.role;
  const navigate = useNavigate(); // Used to redirect after logout

  // Stripe connection status state (for lenders only)
  const [stripeStatus, setStripeStatus] = useState<StripeConnectAccount | null>(null);
  const [isLoadingStatus, setIsLoadingStatus] = useState(false); // UI loading flag
  const [statusError, setStatusError] = useState<string | null>(null); // Error message if fetch fails
  const [fetchStatusInProgress, setFetchStatusInProgress] = useState(false); // Prevents multiple calls

  // useEffect hook runs on mount to fetch Stripe status (for LENDERS only)
  useEffect(() => {
    const fetchStatus = async () => {
      // Prevent duplicate fetch calls
      if (fetchStatusInProgress) return;

      // Only proceed if user is a LENDER
      if (user && user.role === UserRole.LENDER) {
        setFetchStatusInProgress(true); // Lock fetch
        setIsLoadingStatus(true); // Show loading indicator
        setStatusError(null); // Reset previous errors
        setStripeStatus(null); // Clear old status

        try {
          const response = await getAccountStatus(); // API call to backend
          if (response.data.status === 200 && response.data.data) {
            setStripeStatus(response.data.data); // Save Stripe account info
          } else {
            // Server responded but without usable data
            setStatusError(response.data.message || "Failed to fetch Stripe status.");
          }
        } catch (error) {
          // Network or unexpected error
          console.error("Error fetching Stripe status:", error);
          setStatusError("An error occurred while fetching Stripe status.");
        } finally {
          setIsLoadingStatus(false); // Done loading
          setFetchStatusInProgress(false); // Allow new fetches
        }
      } else {
        // If not a lender, clear all Stripe-related states
        setStripeStatus(null);
        setIsLoadingStatus(false);
        setStatusError(null);
      }
    };

    fetchStatus(); // Invoke fetch logic once on mount
  }, []);

  // Handles logout and redirects to homepage
  const handleLogout = async () => {
    try {
      await logout();
      navigate("/"); // Redirect to homepage
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

          {/* If user is logged in, show account details */}
          {auth.currentUser && (
            <div className="mb-6 p-4 bg-white rounded-lg shadow-sm border border-gray-100">
              <h2 className="text-xl font-semibold mb-2">Your Account</h2>
              
              {/* Email display */}
              <p className="text-gray-600 mb-2">
                Logged in as:{" "}
                <span className="font-medium">{auth.currentUser.email}</span>
              </p>

              {/* Role display */}
              <p className="text-gray-600">
                Account Type:{" "}
                <span className="inline-block px-3 py-1 bg-blue-100 text-blue-800 rounded-full font-medium">
                  {user ? userRole : "Loading..."}
                </span>
              </p>

              {/* Stripe status section (only shown for LENDER role) */}
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

          {/* CTA Buttons: Always show 'Browse Listings'; show 'Log Out' only if logged in */}
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