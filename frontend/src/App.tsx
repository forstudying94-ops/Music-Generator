import { useCallback, useEffect, useRef, useState } from "react";
import { getSongs } from "./api";
import { TopBar } from "./components/TopBar";
import { SongGallery } from "./components/SongGallery";
import { SongTable } from "./components/SongTable";
import {
  defaultUiLang,
  filterInputDelay,
  localeByUiLang,
  makeRandomSeed,
  screens,
  startFilters,
  uiText,
  type Filters,
  type Screen,
  type Song,
  type UiLang,
} from "./config";

export default function App() {
  const [uiLang, setUiLang] = useState<UiLang>(defaultUiLang);
  const t = uiText[uiLang];
  const [filters, setFilters] = useState<Filters>(startFilters);
  const [screen, setScreen] = useState<Screen>(screens.table);

  const [page, setPage] = useState(0);
  const [songs, setSongs] = useState<Song[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [cardSongs, setCardSongs] = useState<Song[]>([]);
  const [nextCardPage, setNextCardPage] = useState(0);
  const cardLoading = useRef(false);
  const delayTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  async function loadTable(pageNumber: number, current: Filters) {
    setLoading(true);
    setError("");
    try {
      const data = await getSongs(current, pageNumber);
      setSongs(data.items);
      setPage(pageNumber);
    } catch {
      setSongs([]);
      setError(t.loadError);
    } finally {
      setLoading(false);
    }
  }

  async function loadCardsFromStart(current: Filters) {
    cardLoading.current = false;
    setCardSongs([]);
    setNextCardPage(0);
    setLoading(true);
    setError("");

    try {
      const data = await getSongs(current, 0);
      setCardSongs(data.items);
      setNextCardPage(1);
    } catch {
      setCardSongs([]);
      setError(t.loadError);
    } finally {
      setLoading(false);
    }
  }

  const loadMoreCards = useCallback(async () => {
    if (cardLoading.current || error) return;

    cardLoading.current = true;
    try {
      const data = await getSongs(filters, nextCardPage);
      setCardSongs((old) => [...old, ...data.items]);
      setNextCardPage((old) => old + 1);
    } catch {
      setError(t.loadMoreError);
    } finally {
      cardLoading.current = false;
    }
  }, [filters, nextCardPage, error, t.loadMoreError]);

  function reload(next: Filters) {
    setPage(0);
    window.scrollTo(0, 0);

    if (screen === screens.table) {
      loadTable(0, next);
    } else {
      loadCardsFromStart(next);
    }
  }

  function scheduleReload(next: Filters) {
    if (delayTimer.current) {
      clearTimeout(delayTimer.current);
    }
    delayTimer.current = setTimeout(() => reload(next), filterInputDelay);
  }

  function changeUiLang(next: UiLang) {
    setUiLang(next);
    const nextFilters = { ...filters, lang: localeByUiLang[next] };
    setFilters(nextFilters);
    reload(nextFilters);
  }

  function changeSeed(seed: string) {
    const next = { ...filters, seed };
    setFilters(next);
    scheduleReload(next);
  }

  function changeLikes(likes: number) {
    const next = { ...filters, likes };
    setFilters(next);
    scheduleReload(next);
  }

  function randomizeSeed() {
    const next = { ...filters, seed: makeRandomSeed() };
    setFilters(next);
    reload(next);
  }

  function changeScreen(next: Screen) {
    setScreen(next);
    setPage(0);
    window.scrollTo(0, 0);

    if (next === screens.table) {
      loadTable(0, filters);
    } else {
      loadCardsFromStart(filters);
    }
  }

  useEffect(() => {
    loadTable(0, startFilters);
  }, []);

  return (
    <>
      <TopBar
        uiLang={uiLang}
        filters={filters}
        screen={screen}
        onUiLangChange={changeUiLang}
        onSeedChange={changeSeed}
        onRandomSeed={randomizeSeed}
        onLikesChange={changeLikes}
        onScreenChange={changeScreen}
      />

      <main>
        {error && <p className="status-message status-error">{error}</p>}
        {loading && !error && <p className="status-message">{t.loading}</p>}

        {!loading && !error && screen === screens.table && (
          <SongTable
            songs={songs}
            page={page}
            uiLang={uiLang}
            onPrev={() => loadTable(page - 1, filters)}
            onNext={() => loadTable(page + 1, filters)}
          />
        )}

        {!loading && !error && screen === screens.cards && (
          <SongGallery songs={cardSongs} uiLang={uiLang} onLoadMore={loadMoreCards} />
        )}
      </main>
    </>
  );
}
