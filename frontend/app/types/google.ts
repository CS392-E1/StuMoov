export type GeocodeResult = {
  lat: number;
  lng: number;
};

export type GeocodeError = {
  message: string;
  status: google.maps.GeocoderStatus;
};
