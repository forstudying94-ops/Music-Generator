import { useEffect, useRef, useState, type MouseEvent } from "react";

let activePlayer: (() => void) | null = null;

type Props = {
  src: string;
  compact?: boolean;
};

function waitForCanPlay(audio: HTMLAudioElement) {
  return new Promise<void>((resolve, reject) => {
    if (audio.readyState >= HTMLMediaElement.HAVE_ENOUGH_DATA) {
      resolve();
      return;
    }

    const onReady = () => {
      cleanup();
      resolve();
    };
    const onError = () => {
      cleanup();
      reject(new Error("audio load failed"));
    };
    const cleanup = () => {
      audio.removeEventListener("canplaythrough", onReady);
      audio.removeEventListener("error", onError);
    };

    audio.addEventListener("canplaythrough", onReady);
    audio.addEventListener("error", onError);
    audio.load();
  });
}

export function PreviewPlayer({ src, compact = false }: Props) {
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [playing, setPlaying] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(false);

  useEffect(() => {
    return () => {
      audioRef.current?.pause();
      audioRef.current = null;
      if (activePlayer === stop) {
        activePlayer = null;
      }
    };
  }, [src]);

  function getAudio() {
    const absolute = new URL(src, window.location.origin).href;

    if (audioRef.current?.src === absolute) {
      return audioRef.current;
    }

    audioRef.current?.pause();
    const audio = new Audio(absolute);
    audio.preload = "none";
    audioRef.current = audio;

    audio.addEventListener("playing", () => {
      setPlaying(true);
      setLoading(false);
      setError(false);
    });
    audio.addEventListener("pause", () => setPlaying(false));
    audio.addEventListener("ended", () => setPlaying(false));

    return audio;
  }

  function stop() {
    const audio = audioRef.current;
    if (audio) {
      audio.pause();
      audio.currentTime = 0;
    }
    setPlaying(false);
  }

  async function toggle(e: MouseEvent) {
    e.stopPropagation();
    const audio = getAudio();

    if (playing) {
      audio.pause();
      return;
    }

    if (activePlayer && activePlayer !== stop) {
      activePlayer();
    }
    activePlayer = stop;

    setLoading(true);
    setError(false);

    try {
      await waitForCanPlay(audio);
      await audio.play();
    } catch {
      setLoading(false);
      setPlaying(false);
      setError(true);
    }
  }

  let label = "Play";
  if (error) label = "Retry";
  else if (loading) label = "Loading…";
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
