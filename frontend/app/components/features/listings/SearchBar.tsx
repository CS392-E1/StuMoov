import { useState } from "react";

interface SearchBarProps {
  onSearch?: (query: string) => void;
}

export function SearchBar({ onSearch }: SearchBarProps) {
  const [searchQuery, setSearchQuery] = useState("");

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (onSearch) {
      onSearch(searchQuery);
    } else {
      console.log("Searching for:", searchQuery);
    }
  };

  return (
    <div className="w-full pb-4">
      <form onSubmit={handleSearch} className="flex">
        <div className="relative w-full">
          <input
            type="text"
            className="w-full p-3 pr-20 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Enter Address"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
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
