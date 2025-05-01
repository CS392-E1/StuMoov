import { useState } from "react";

// Define props for optional search callback
interface SearchBarProps {
  onSearch?: (query: string) => void;
}

export function SearchBar({ onSearch }: SearchBarProps) {
  const [searchQuery, setSearchQuery] = useState(""); // State to track user input

  // Handle form submission
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault(); // Prevent page reload
    if (onSearch) {
      onSearch(searchQuery); // Call the search callback if provided
    } else {
      console.log("Searching for:", searchQuery); // Default fallback action
    }
  };

  return (
    <div className="w-full pb-4">
      {/* Form with controlled input */}
      <form onSubmit={handleSearch} className="flex">
        <div className="relative w-full">
          <input
            type="text"
            className="w-full p-3 pr-20 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Enter Address"
            value={searchQuery} // Controlled input value
            onChange={(e) => setSearchQuery(e.target.value)} // Update state on change
          />
          {/* Search button positioned inside input container */}
          <button
            type="submit"
            className="absolute right-2 top-1/2 transform -translate-y-1/2 bg-blue-500 text-white font-semibold px-3 py-1 rounded-md cursor-pointer hover:bg-blue-600"
          >
            Find Storage
          </button>
        </div>
      </form>
    </div>
  );
}