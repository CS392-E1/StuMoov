import React from "react";
import { Link } from "react-router-dom";

const Footer: React.FC = () => {
  return (
    <footer className="bg-gray-100 py-6 mt-auto">
      <div className="container mx-auto px-4">
        <div className="flex flex-col md:flex-row justify-between items-center">
          <div className="mb-4 md:mb-0">
            <p className="text-gray-600">
              © {new Date().getFullYear()} StuMoov. All rights reserved.
            </p>
          </div>

          <div className="flex gap-6">
            <Link to="/about" className="text-gray-600 hover:text-blue-500">
              About
            </Link>
            <Link to="/privacy" className="text-gray-600 hover:text-blue-500">
              Privacy
            </Link>
            <Link to="/terms" className="text-gray-600 hover:text-blue-500">
              Terms
            </Link>
            <Link to="/contact" className="text-gray-600 hover:text-blue-500">
              Contact
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
