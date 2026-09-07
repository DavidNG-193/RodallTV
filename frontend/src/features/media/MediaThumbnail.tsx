import { FileImage, FileVideo } from "lucide-react";
import { useEffect, useState } from "react";
import { mediaService } from "./media.service";
import type { MediaItem } from "./media.types";

type ThumbnailMedia = Pick<MediaItem, "id" | "mediaType">;

export function MediaThumbnail({ item }: { item: ThumbnailMedia }) {
  const [source, setSource] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    let objectUrl: string | null = null;

    void mediaService
      .getThumbnail(item.id)
      .then((thumbnail) => {
        objectUrl = URL.createObjectURL(thumbnail);
        if (cancelled) {
          URL.revokeObjectURL(objectUrl);
          objectUrl = null;
          return;
        }

        setSource(objectUrl);
      })
      .catch(() => {
        if (!cancelled) {
          setSource(null);
        }
      });

    return () => {
      cancelled = true;
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [item.id]);

  if (source) {
    return <img src={source} alt="" loading="lazy" />;
  }

  return (
    <span className="file-manager__placeholder" aria-hidden="true">
      {item.mediaType === "Image" ? (
        <FileImage size={38} />
      ) : (
        <FileVideo size={38} />
      )}
    </span>
  );
}
