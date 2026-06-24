import { Fragment, useEffect, useState } from "react";
import { coverSize, tableColumns, uiText, type UiLang } from "../config";
import type { Song } from "../config";
import { PreviewPlayer } from "./PreviewPlayer";

type Props = {
  songs: Song[];
  page: number;
  uiLang: UiLang;
  onPrev: () => void;
  onNext: () => void;
};

export function SongTable({ songs, page, uiLang, onPrev, onNext }: Props) {
  const t = uiText[uiLang];
  const [openIndex, setOpenIndex] = useState<number | null>(null);

  useEffect(() => {
    setOpenIndex(null);
  }, [page, songs]);

  function toggleRow(index: number) {
    setOpenIndex((current) => (current === index ? null : index));
  }

  const colCount = tableColumns.length;

  return (
    <section>
      <table>
        <thead>
          <tr>
            {tableColumns.map((column) => (
              <th key={column.key} className={column.num ? "num" : ""}>
                {t[column.key]}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {songs.map((song) => (
            <Fragment key={song.index}>
              <tr
                className={`song-row${openIndex === song.index ? " song-row-open" : ""}`}
                onClick={() => toggleRow(song.index)}
              >
                <td className="num">{song.index}</td>
                <td>{song.title}</td>
                <td>{song.artist}</td>
                <td>{song.album}</td>
                <td>{song.genre}</td>
                <td className="num">Likes {song.likes}</td>
              </tr>
              {openIndex === song.index && (
                <tr className="song-detail-row">
                  <td colSpan={colCount}>
                    <div className="song-detail">
                      <img
                        className="song-detail-cover"
                        src={song.coverUrl}
                        alt=""
                        width={coverSize.table}
                        height={coverSize.table}
                      />
                      <div className="song-detail-body">
                        <PreviewPlayer src={song.previewUrl} />
                        <p className="song-detail-review">
                          <span className="song-detail-label">{t.review}</span>
                          {song.review}
                        </p>
                      </div>
                    </div>
                  </td>
                </tr>
              )}
            </Fragment>
          ))}
        </tbody>
      </table>

      <div className="pager">
        <button type="button" disabled={page === 0} onClick={onPrev}>
          {t.prev}
        </button>
        <span>
          {t.page} {page + 1}
        </span>
        <button type="button" onClick={onNext}>
          {t.next}
        </button>
      </div>
    </section>
  );
}
