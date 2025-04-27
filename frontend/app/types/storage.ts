export interface Message {
  id?: string;
  senderId: string;
  recipientId: string;
  content: string;
  sentAt?: string;
}

export interface User {
  id: string;
  email: string;
  displayName?: string;
  [key: string]: any;
}

export interface StorageLocation {
  id: string;
  lenderId: string;
  name: string;
  description: string;
  lat: number;
  lng: number;
  price: number;
  address: string;
  imageUrl: string;
}