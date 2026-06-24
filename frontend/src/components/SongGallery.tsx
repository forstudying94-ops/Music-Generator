import { useEffect, useRef } from "react";
import { cardsLoadDistance, cardColumnWidth, uiText, type UiLang } from "../config";
import type { Song } from "../config";
import { SongCard } from "./SongCard";

type Props = {
  songs: Song[];
  uiLang: UiLang;
  onLoadMore: () => void;
};

export function SongGallery({ songs, uiLang, onLoadMore }: Props) {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const node = bottomRef.current;
    if (!node) return;

    const watcher = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting) {
          onLoadMore();
        }
      },
      { rootMargin: cardsLoadDistance },
    );

    watcher.observe(node);
    return () => watcher.disconnect();
  }, [onLoadMore]);

  return (
    <section>
      <div
        className="gallery-grid"
        style={{
          gridTemplateColumns: `repeat(auto-fill, minmax(${cardColumnWidth}px, 1fr))`,
        }}
      >
        {songs.map((song) => (
          <SongCard key={song.index} song={song} />
        ))}
      </div>
      <div ref={bottomRef} className="sentinel">
        {uiText[uiLang].loadingMore}
      </div>
    </section>
  );
}
