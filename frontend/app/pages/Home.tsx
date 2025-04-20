import { Label } from "@/components/ui/label";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";
import { signOutFirebase, auth } from "@/lib/firebase";

export default function Home() {
  const { user } = useAuth();
  const userRole = user?.role;
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await signOutFirebase();
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

          <div className="flex flex-wrap gap-4 justify-center">
            <Link to="/listings">
              <button className="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors">
                Browse Listings
              </button>
            </Link>
            <div>
              <Label>
                <p>Your role is {userRole || "unknown"}</p>
              </Label>
            </div>

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
