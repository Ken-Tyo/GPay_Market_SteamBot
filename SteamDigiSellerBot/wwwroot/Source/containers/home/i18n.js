import i18n from 'i18next';
//import Backend from 'i18next-xhr-backend';
//import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';
import translationEN from '../public/locales/en/translation.json';
import translationRU from '../public/locales/ru/translation.json';

const resources = {
  en: {
    ...translationEN,
  },
  ru: {
    ...translationRU,
  },
};

i18n
  //   .use(Backend) // load translation using xhr -> see /public/locales. We will add locales in the next step
  //   .use(LanguageDetector) // detect user language
  .use(initReactI18next) // pass the i18n instance to react-i18next.
  .init({
    resources,
    lng: 'ru',
    debug: true,

    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;
