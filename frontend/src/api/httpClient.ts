import axios from "axios";

   const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

   if (!apiBaseUrl) {
     throw new Error(
       "No se encontró VITE_API_BASE_URL en las variables de entorno.",
     );
   }

   export const httpClient = axios.create({
     baseURL: apiBaseUrl,
     headers: {
       "Content-Type": "application/json",
     },
     timeout: 30000,
   });