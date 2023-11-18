import React, { useEffect, Suspense } from 'react';
import LastOrders from '../../components/home/lastOrders';
import OrderState from '../../components/home/orderState';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
  Outlet,
  useSearchParams,
} from 'react-router-dom';
import { state, apiFetchLastOrders, checkCodeWithParams, init } from './state';
import css from './styles.scss';
import './i18n';
import TextSwitch from '../../components/home/textSwitch';
import { useTranslation } from 'react-i18next';
import { useCookies } from 'react-cookie';
import useGeoLocation from 'react-ipgeolocation';

const Layout = () => {
  const { t, i18n } = useTranslation();
  const [cookies, setCookie] = useCookies();
  const location = useGeoLocation();

  if (!cookies.ln) {
    let lang = location.country === 'RU' ? 'ru' : 'en';
    setCookie('ln', lang, { path: '/', maxAge: 365 * 24 * 60 * 60 });
    i18n.changeLanguage(lang);
  } else {
    if (i18n.language !== cookies.ln) i18n.changeLanguage(cookies.ln);
  }

  let currLang = i18n.language;

  return (
    <div className={css.wrapper}>
      <div className={css.bg}></div>
      <div className={css.wrapperBackground}>
        <div style={{ flex: '1 1 auto' }}>
          <LastOrders />
        </div>
        <div style={{ flex: '1 1 auto' }}>
          <OrderState />
        </div>
        <div className={css.footer}>
          <TextSwitch
            defaultValue={currLang === 'en'}
            options={['Ru', 'Eng']}
            onChange={(val) => {
              let len = val ? 'en' : 'ru';
              i18n.changeLanguage(len);
              setCookie('ln', i18n.language);
            }}
          />
        </div>
      </div>
    </div>
  );
};

let router = createBrowserRouter(
  createRoutesFromElements(
    <Route
      path="/"
      loader={async () => {
        await init();
        return true;
      }}
      element={<Layout />}
    />
  )
);

const App = () => {
  return <RouterProvider router={router} />;
};

export default App;
