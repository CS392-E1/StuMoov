import React from "react";
import ReactDOM from "react-dom/client";
import { HydratedRouter } from "react-router/dom";
import "@/styles.css";
import { AuthProvider } from "@/contexts/AuthContext";

ReactDOM.hydrateRoot(
  document,
  <React.StrictMode>
    <AuthProvider>
      <HydratedRouter />
    </AuthProvider>
  </React.StrictMode>
);
