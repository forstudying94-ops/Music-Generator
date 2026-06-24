import { coverSize } from "../config";
import type { Song } from "../config";
import { PreviewPlayer } from "./PreviewPlayer";

type Props = {
  song: Song;
};

export function SongCard({ song }: Props) {
  return (
    <div className="card">
      <img
        className="card-cover"
        src={song.coverUrl}
        alt=""
        loading="lazy"
        width={coverSize.card}
        height={coverSize.card}
      />
      <div className="card-index">#{song.index}</div>
      <div className="card-title">{song.title}</div>
      <div className="card-artist">{song.artist}</div>
      <div className="card-meta">
        {song.album} {song.genre} Likes {song.likes}
      </div>
      <PreviewPlayer src={song.previewUrl} />
    </div>
  );
}
