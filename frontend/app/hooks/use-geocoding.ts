import { useState, useCallback } from "react";
import { GeocodeError, GeocodeResult } from "@/types/google";

export const useGeocoding = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<GeocodeError | null>(null);

  const geocodeAddress = useCallback(
    (address: string): Promise<GeocodeResult> => {
      return new Promise((resolve, reject) => {
        if (!address) {
          reject({
            message: "Address cannot be empty.",
            status: "INVALID_REQUEST",
          });
          return;
        }

        if (
          typeof window.google === "undefined" ||
          typeof window.google.maps === "undefined"
        ) {
          console.error("Google Maps API not loaded.");
          reject({
            message: "Google Maps API not available.",
            status: "UNKNOWN_ERROR",
          });
          return;
        }

        setIsLoading(true);
        setError(null);
        const geocoder = new window.google.maps.Geocoder();

        geocoder.geocode({ address: address }, (results, status) => {
          setIsLoading(false);
          if (
            status === google.maps.GeocoderStatus.OK &&
            results &&
            results[0]
          ) {
            const location = results[0].geometry.location;
            const geocodeResult: GeocodeResult = {
              lat: location.lat(),
              lng: location.lng(),
            };
            resolve(geocodeResult);
          } else {
            console.error(
              `Geocode was not successful for the following reason: ${status}`
            );
            const geocodeError: GeocodeError = {
              message: `Geocoding failed: ${status}`,
              status: status,
            };
            setError(geocodeError);
            reject(geocodeError);
          }
        });
      });
    },
    []
  );

  return { geocodeAddress, isLoading, error };
};
