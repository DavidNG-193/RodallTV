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

httpClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("rodalltv_token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("rodalltv_token");
      localStorage.removeItem("rodalltv_user");
    }

    return Promise.reject(error);
  },
);