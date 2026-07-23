interface LoadingStateProps {
  message?: string;
}

export function LoadingState({ message = "Cargando..." }: LoadingStateProps) {
  return <div className="state-message" role="status">{message}</div>;
}