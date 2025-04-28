import React from "react";
import { Link } from "react-router-dom";
import { useAuth } from "@/hooks/use-auth";

const Header: React.FC = () => {
  const { user } = useAuth();

  return (
    <header className="bg-white shadow-md py-4 px-6">
      <div className="max-w-7xl mx-auto flex justify-between items-center">
        <div>
          <Link to="/" className="text-2xl font-bold text-blue-600">
            StuMoov
          </Link>
        </div>
        {user ? (
          <div className="flex gap-4 items-center">
            <span>{user?.displayName || user?.email}</span>
          </div>
        ) : (
          <div className="flex gap-4">
            <Link
              to="/login"
              className="bg-blue-500 hover:bg-blue-600 text-white font-medium py-2 px-4 rounded-md transition-colors"
            >
              Login
            </Link>
            <Link
              to="/register"
              className="border border-blue-500 text-blue-500 hover:bg-blue-50 font-medium py-2 px-4 rounded-md transition-colors"
            >
              Register
            </Link>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
