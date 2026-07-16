import { useTranslation } from "react-i18next";
import "./languageToggle.css";

// Single button that flips the whole site between English and Arabic (and LTR/RTL — see
// applyDirection in i18n/index.js, triggered by i18next's languageChanged event).
export default function LanguageToggle({ className = "" }) {
  const { i18n, t } = useTranslation();

  const toggle = () => {
    i18n.changeLanguage(i18n.language === "ar" ? "en" : "ar");
  };

  return (
    <button type="button" className={`lang-toggle ${className}`} onClick={toggle}>
      🌐 {t("nav.languageToggle")}
    </button>
  );
}
