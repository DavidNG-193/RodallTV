export function formatFileSize(
  bytes: number,
): string {
  if (bytes === 0) {
    return "0 B";
  }

  const units = [
    "B",
    "KB",
    "MB",
    "GB",
  ];

  const unitIndex = Math.floor(
    Math.log(bytes) / Math.log(1024),
  );

  const safeIndex = Math.min(
    unitIndex,
    units.length - 1,
  );

  const value =
    bytes / Math.pow(1024, safeIndex);

  return `${value.toFixed(
    safeIndex === 0 ? 0 : 1,
  )} ${units[safeIndex]}`;
}

export function formatDuration(
  seconds: number | null,
): string {
  if (
    seconds === null ||
    seconds === undefined
  ) {
    return "No disponible";
  }

  const totalSeconds =
    Math.max(0, Math.round(seconds));

  const minutes = Math.floor(
    totalSeconds / 60,
  );

  const remainingSeconds =
    totalSeconds % 60;

  return `${minutes}:${remainingSeconds
    .toString()
    .padStart(2, "0")}`;
}

export function formatDateTime(
  value: string,
): string {
  return new Intl.DateTimeFormat(
    "es-MX",
    {
      dateStyle: "medium",
      timeStyle: "short",
    },
  ).format(new Date(value));
}