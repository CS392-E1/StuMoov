// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getAuth } from 'firebase/auth';
import { getFirestore } from 'firebase/firestore';
import { getAnalytics } from "firebase/analytics";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyBaDmd0vTifYTra58chRGV-dGtqtoq9Hwo",
  authDomain: "stumoov-eb219.firebaseapp.com",
  projectId: "stumoov-eb219",
  storageBucket: "stumoov-eb219.firebasestorage.app",
  messagingSenderId: "662076919979",
  appId: "1:662076919979:web:1a671ceea314371eaca7ee",
  measurementId: "G-9V3MYJ2YJR"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);

console.log("Firebase Initialized", app);

// Export Firebase services
export const auth = getAuth(app);
export const db = getFirestore(app);

export default app;