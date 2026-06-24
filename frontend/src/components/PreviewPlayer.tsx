import { useEffect, useRef, useState, type MouseEvent } from "react";

let activePlayer: (() => void) | null = null;

type Props = {
  src: string;
  compact?: boolean;
};

export function PreviewPlayer({ src, compact = false }: Props) {
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [playing, setPlaying] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const audio = new Audio(src);
    audioRef.current = audio;

    const onPlaying = () => {
      setPlaying(true);
      setLoading(false);
    };
    const onPause = () => setPlaying(false);
    const onWaiting = () => setLoading(true);
    const onEnded = () => setPlaying(false);

    audio.addEventListener("playing", onPlaying);
    audio.addEventListener("pause", onPause);
    audio.addEventListener("waiting", onWaiting);
    audio.addEventListener("ended", onEnded);

    return () => {
      audio.pause();
      audio.removeEventListener("playing", onPlaying);
      audio.removeEventListener("pause", onPause);
      audio.removeEventListener("waiting", onWaiting);
      audio.removeEventListener("ended", onEnded);
      if (activePlayer === stop) {
        activePlayer = null;
      }
    };
  }, [src]);

  function stop() {
    const audio = audioRef.current;
    if (audio) {
      audio.pause();
      audio.currentTime = 0;
    }
  }

  function toggle(e: MouseEvent) {
    e.stopPropagation();
    const audio = audioRef.current;
    if (!audio) return;

    if (playing) {
      audio.pause();
      return;
    }

    if (activePlayer && activePlayer !== stop) {
      activePlayer();
    }
    activePlayer = stop;

    setLoading(true);
    audio.play().catch(() => setLoading(false));
  }

  let label = "Play";
  if (loading) label = "Loading";
  else if (playing) label = "Pause";

  return (
    <button
      type="button"
      className={compact ? "play-button play-button-compact" : "play-button"}
      onClick={toggle}
    >
      {label}
    </button>
  );
}
