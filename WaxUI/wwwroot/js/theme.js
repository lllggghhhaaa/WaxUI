const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";
const THEME_COOKIE = "wax-theme";
const media = window.matchMedia("(prefers-color-scheme: dark)");
const root = document.documentElement;
function removeThemeClasses() {
  [...root.classList].filter((c) => c.startsWith(THEME_PREFIX)).forEach((c) => root.classList.remove(c));
}
function resolveTheme(theme) {
  if (!theme || theme.toLowerCase() === "system")
    return media.matches ? "dark" : "light";
  return theme.toLowerCase() === "dark" ? "dark" : "light";
}
function applyClasses(resolved) {
  removeThemeClasses();
  root.classList.add(`${THEME_PREFIX}${resolved}`);
  try {
    document.cookie = `${THEME_COOKIE}=${resolved}; path=/; max-age=31536000; samesite=lax`;
  } catch {
  }
}
function getTheme() {
  return localStorage.getItem(STORAGE_KEY);
}
function setTheme(theme) {
  localStorage.setItem(STORAGE_KEY, theme);
  applyClasses(resolveTheme(theme));
}
function getSystemTheme() {
  return media.matches ? "dark" : "light";
}
function applyTheme(theme) {
  applyClasses(resolveTheme(theme));
}
media.addEventListener("change", () => {
  applyClasses(resolveTheme(localStorage.getItem(STORAGE_KEY)));
});
export {
  applyTheme,
  getSystemTheme,
  getTheme,
  setTheme
};
