import { songsOnPage, type Filters, type SongList } from "./config";

export async function getSongs(filters: Filters, page: number): Promise<SongList> {
  const params = new URLSearchParams({
    seed: filters.seed,
    likes: String(filters.likes),
    lang: filters.lang,
    page: String(page),
    pageSize: String(songsOnPage),
  });

  const res = await fetch(`/api/songs?${params}`);
  if (!res.ok) {
    throw new Error("Failed to load songs");
  }
  return res.json();
}
