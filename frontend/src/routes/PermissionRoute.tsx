import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../features/auth/useAuth";

type Props = {
  permission: string;
};

export function PermissionRoute({ permission }: Props) {
  const {
    isAuthenticated,
    isLoading,
    hasPermission,
  } = useAuth();

  if (isLoading) {
    return null;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!hasPermission(permission)) {
    return <Navigate to="/access-denied" replace />;
  }

  return <Outlet />;
}
