import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import ar from './locales/ar.json';

export const STORAGE_KEY = 'velvet_language';
export const RTL_LANGUAGES = ['ar'];

function getStoredLanguage() {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'en' || stored === 'ar') return stored;
  } catch { /* ignore */ }
  return 'en';
}

export function applyDirection(lang) {
  const dir = RTL_LANGUAGES.includes(lang) ? 'rtl' : 'ltr';
  document.documentElement.dir = dir;
  document.documentElement.lang = lang;
}

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    ar: { translation: ar },
  },
  lng: getStoredLanguage(),
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
});

applyDirection(i18n.language);

i18n.on('languageChanged', (lang) => {
  try {
    localStorage.setItem(STORAGE_KEY, lang);
  } catch { /* ignore */ }
  applyDirection(lang);
});

export default i18n;
