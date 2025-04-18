import { Link } from "react-router-dom";

export default function Home() {
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
          </div>
        </div>
      </div>
    </div>
  );
}
