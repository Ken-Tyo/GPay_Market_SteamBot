import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useCookies } from 'react-cookie';
import useGeoLocation from 'react-ipgeolocation';

import TextSwitch from '../home/textSwitch';

import telegramIcon from '../../icons/telegram.svg';
import css from './styles.scss';
import { state } from '../../containers/home/state';

const Footer = () => {
    const { i18n } = useTranslation();
    const [cookies, setCookie, removeCookie] = useCookies();
    const location = useGeoLocation();
    const { t: tFooter } = useTranslation('footer');
    const { gameSession } = state?.use();


    if (!cookies.ln) {
        const lang = location.country === 'RU' ? 'ru' : 'en';
        setCookie('ln', lang, { path: '/', maxAge: 365 * 24 * 60 * 60 });
        i18n.changeLanguage(lang);
    } else {
        if (i18n.language !== cookies.ln) i18n.changeLanguage(cookies.ln);
    }
    let currLang = i18n.language;

    useEffect(() => {
        if (gameSession?.market === 1271) {
            setCookie('ggtghide', 'true', { path: '/', maxAge: 24 * 60 * 60 });
        }
        else if (gameSession?.market > 0 && cookies.ggtghide) {
            removeCookie('ggtghide', { path: '/' });
        }
    }, [gameSession?.market, cookies.ggtghide, setCookie]);
    const tgHideCoockie = cookies.ggtghide === 'true';

    return (
        <div className={css.footer}>
            {(!tgHideCoockie && gameSession?.market != 1271 ) && (
                <a href="https://t.me/GPay_Market" target="_blank" className={css.telegram}>
                    <img src={telegramIcon} alt="telegram" />
                </a>
            )}
            {(tgHideCoockie || gameSession?.market == 1271) && (
                <div></div>
            )}
            <TextSwitch
                defaultValue={currLang === 'en'}
                options={['Ru', 'Eng']}
                onChange={(val) => {
                    const lan = val ? 'en' : 'ru';
                    i18n.changeLanguage(lan);
                    setCookie('ln', i18n.language);
                }}
            />
            {(!tgHideCoockie && gameSession?.market != 1271) && (
                <div className={css.tg_hint}><div>{tFooter('giveaways')}<span>&nbsp;{tFooter('giveaways_sign')}</span></div></div>
            )}
        </div>
    );
}

export default Footer;