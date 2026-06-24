import {
  likesMax,
  likesMin,
  likesStep,
  screens,
  uiLanguages,
  uiText,
  type Screen,
  type UiLang,
  type Filters,
} from "../config";

type Props = {
  uiLang: UiLang;
  filters: Filters;
  screen: Screen;
  onUiLangChange: (lang: UiLang) => void;
  onSeedChange: (seed: string) => void;
  onRandomSeed: () => void;
  onLikesChange: (likes: number) => void;
  onScreenChange: (screen: Screen) => void;
};

export function TopBar({
  uiLang,
  filters,
  screen,
  onUiLangChange,
  onSeedChange,
  onRandomSeed,
  onLikesChange,
  onScreenChange,
}: Props) {
  const t = uiText[uiLang];

  return (
    <header className="toolbar">
      <div className="control">
        <label htmlFor="lang">{t.language}</label>
        <select
          id="lang"
          className="control-field"
          value={uiLang}
          onChange={(e) => onUiLangChange(e.target.value as UiLang)}
        >
          {uiLanguages.map((item) => (
            <option key={item.code} value={item.code}>
              {item.label}
            </option>
          ))}
        </select>
      </div>

      <div className="control">
        <label htmlFor="seed">{t.seed}</label>
        <div className="control-field seed-group">
          <input
            id="seed"
            type="text"
            value={filters.seed}
            autoComplete="off"
            onChange={(e) => onSeedChange(e.target.value)}
          />
          <button type="button" title={t.randomSeed} onClick={onRandomSeed}>
            {t.randomSeedBtn}
          </button>
        </div>
      </div>

      <div className="control">
        <label htmlFor="likes">{t.avgLikes}</label>
        <div className="control-field control-field-range">
          <input
            id="likes"
            type="range"
            min={likesMin}
            max={likesMax}
            step={likesStep}
            value={filters.likes}
            onChange={(e) => onLikesChange(Number(e.target.value))}
          />
          <span className="control-value">{filters.likes.toFixed(1)}</span>
        </div>
      </div>

      <div className="control">
        <label>{t.view}</label>
        <div className="control-field view-toggle">
          <button
            type="button"
            className={screen === screens.table ? "active" : ""}
            onClick={() => onScreenChange(screens.table)}
          >
            {t.table}
          </button>
          <button
            type="button"
            className={screen === screens.cards ? "active" : ""}
            onClick={() => onScreenChange(screens.cards)}
          >
            {t.gallery}
          </button>
        </div>
      </div>
    </header>
  );
}
