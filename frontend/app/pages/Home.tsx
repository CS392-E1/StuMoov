import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";
import { auth } from "@/lib/firebase";

export default function Home() {
  const { user, logout } = useAuth();
  const userRole = user?.role;
  const navigate = useNavigate();

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
            <Link to="/add-listing">
              <button className="px-6 py-3 bg-white text-blue-600 border border-blue-600 font-medium rounded-lg hover:bg-blue-50 transition-colors">
                Add Listing
              </button>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}