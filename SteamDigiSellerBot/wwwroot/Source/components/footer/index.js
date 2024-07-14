import { useTranslation } from 'react-i18next';
import { useCookies } from 'react-cookie';
import useGeoLocation from 'react-ipgeolocation';

import TextSwitch from '../home/textSwitch';

import telegramIcon from '../../icons/telegram.svg';
import css from './styles.scss';

const Footer = () => {
  const { i18n } = useTranslation();
  const [cookies, setCookie] = useCookies();
  const location = useGeoLocation();

  if (!cookies.ln) {
    const lang = location.country === 'RU' ? 'ru' : 'en';
    setCookie('ln', lang, { path: '/', maxAge: 365 * 24 * 60 * 60 });
    i18n.changeLanguage(lang);
  } else {
    if (i18n.language !== cookies.ln) i18n.changeLanguage(cookies.ln);
  }

  let currLang = i18n.language;

  return (
    <div className={css.footer}>
      <a href="https://t.me/GPay_Market" target="_blank" className={css.telegram}>
        <img src={telegramIcon} alt="telegram"/>
      </a>
      <TextSwitch
        defaultValue={currLang === 'en'}
        options={['Ru', 'Eng']}
        onChange={(val) => {
          const lan = val ? 'en' : 'ru';
          i18n.changeLanguage(lan);
          setCookie('ln', i18n.language);
        }}
      />
    </div>
  );
}

export default Footer;