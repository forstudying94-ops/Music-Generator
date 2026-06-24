export const defaultSeed = "42";
export const defaultLikes = 0;

export const uiLanguages = [
  { code: "en", label: "English (USA)" },
  { code: "de", label: "Deutsch (Deutschland)" },
] as const;

export type UiLang = (typeof uiLanguages)[number]["code"];

export const defaultUiLang: UiLang = "en";

export const localeByUiLang: Record<UiLang, string> = {
  en: "en_US",
  de: "de_DE",
};

export const defaultLocale = localeByUiLang.en;

export const uiText: Record<UiLang, Record<string, string>> = {
  en: {
    language: "Language",
    seed: "Seed",
    randomSeed: "Random seed",
    randomSeedBtn: "Random",
    avgLikes: "Avg likes",
    view: "View",
    table: "Table",
    gallery: "Gallery",
    colIndex: "#",
    colTitle: "Title",
    colArtist: "Artist",
    colAlbum: "Album",
    colGenre: "Genre",
    colLikes: "Likes",
    review: "Review",
    prev: "Prev",
    next: "Next",
    page: "Page",
    loading: "Loading…",
    loadError: "Could not load songs.",
    loadMoreError: "Could not load more songs.",
    loadingMore: "Loading more…",
  },
  de: {
    language: "Sprache",
    seed: "Seed",
    randomSeed: "Zufälliger Seed",
    randomSeedBtn: "Zufall",
    avgLikes: "Durchschn. Likes",
    view: "Ansicht",
    table: "Tabelle",
    gallery: "Galerie",
    colIndex: "#",
    colTitle: "Titel",
    colArtist: "Interpret",
    colAlbum: "Album",
    colGenre: "Genre",
    colLikes: "Likes",
    review: "Rezension",
    prev: "Zurück",
    next: "Weiter",
    page: "Seite",
    loading: "Laden…",
    loadError: "Songs konnten nicht geladen werden.",
    loadMoreError: "Weitere Songs konnten nicht geladen werden.",
    loadingMore: "Mehr laden…",
  },
};

export const songsOnPage = 20;

export const likesMin = 0;
export const likesMax = 10;
export const likesStep = 0.1;

export const filterInputDelay = 250;
export const cardsLoadDistance = "300px";

export const screens = {
  table: "table",
  cards: "cards",
} as const;

export type Screen = (typeof screens)[keyof typeof screens];

export const tableColumns = [
  { key: "colIndex", num: true },
  { key: "colTitle", num: false },
  { key: "colArtist", num: false },
  { key: "colAlbum", num: false },
  { key: "colGenre", num: false },
  { key: "colLikes", num: true },
] as const;

export const cardColumnWidth = 220;

export const coverSize = {
  table: 240,
  card: 220,
} as const;

export type Song = {
  index: number;
  title: string;
  artist: string;
  album: string;
  genre: string;
  likes: number;
  review: string;
  coverUrl: string;
  previewUrl: string;
};

export type SongList = {
  items: Song[];
  page: number;
  hasMore: boolean;
};

export type Filters = {
  seed: string;
  likes: number;
  lang: string;
};

export function makeRandomSeed(): string {
  const part1 = Math.floor(Math.random() * 0x100000000);
  const part2 = Math.floor(Math.random() * 0x100000000);
  return String(part1 * 0x100000000 + part2);
}

export const startFilters: Filters = {
  seed: defaultSeed,
  likes: defaultLikes,
  lang: defaultLocale,
};
