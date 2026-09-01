import axios from "axios";

export const AUTH_UNAUTHORIZED_EVENT = "rodalltv:unauthorized";
export const AUTH_FORBIDDEN_EVENT = "rodalltv:forbidden";

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
      window.dispatchEvent(new Event(AUTH_UNAUTHORIZED_EVENT));
    }

    if (error.response?.status === 403) {
      window.dispatchEvent(new Event(AUTH_FORBIDDEN_EVENT));
    }

    return Promise.reject(error);
  },
);
