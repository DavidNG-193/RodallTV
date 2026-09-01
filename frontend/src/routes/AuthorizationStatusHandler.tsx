import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { AUTH_FORBIDDEN_EVENT } from "../api/httpClient";

export function AuthorizationStatusHandler() {
  const navigate = useNavigate();

  useEffect(() => {
    const handleForbidden = () => navigate("/access-denied", { replace: true });
    window.addEventListener(AUTH_FORBIDDEN_EVENT, handleForbidden);
    return () => window.removeEventListener(AUTH_FORBIDDEN_EVENT, handleForbidden);
  }, [navigate]);

  return null;
}
