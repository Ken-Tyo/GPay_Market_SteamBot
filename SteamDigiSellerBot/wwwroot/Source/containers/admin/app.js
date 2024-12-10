import React from 'react';
//import Test from '../../components/testCmp';
import Products from '../../components/admin/products';
import Orders from '../../components/admin/orders';
import Bots from '../../components/admin/bots';
import Sellers from '../../components/admin/sellers'
import Proxy from '../../components/admin/proxies';
import LeftMenu from '../../components/admin/leftMenu';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
  Outlet,
} from 'react-router-dom';
import {
  initAdmin,
  initBotsPage,
  apiFetchItems,
  apiFetchProxies,
  apiGetCurrencies,
  apiGetSteamRegions,
  apiFetchGameSessionsWithCurrentFilter,
  apiFetchGameSessStatuses,
} from './state';
import css from './styles.scss';

const Layout = () => {
  return (
    <div className={css.wrapper}>
      <div className={css.bg}></div>
      <div className={css.wrapperBackground}>
        <LeftMenu />
        <Outlet />
      </div>
    </div>
  );
};

let router = createBrowserRouter(
  createRoutesFromElements(
    <Route
      path="/admin"
      element={<Layout />}
      loader={async () => {
        await initAdmin();
        return true;
      }}
    >
      <Route
        path="products"
        loader={async () => {
          await apiGetCurrencies();
          Promise.all([apiFetchItems(), apiGetSteamRegions()]);
          return true;
        }}
        element={<Products />}
      />
      <Route
        path="bots"
        loader={async () => {
          initBotsPage();
          return true;
        }}
        element={<Bots />}
      />
      <Route
        path="orders"
        loader={async () => {
          await apiFetchGameSessStatuses();
          Promise.all([
            apiGetCurrencies(),
            apiFetchGameSessionsWithCurrentFilter(),
            apiGetSteamRegions(),
          ]);
          return true;
        }}
        element={<Orders />}
      />
      <Route
        path="sellers"
        element={<Sellers />}
      />
      <Route
        path="proxy"
        loader={async () => {
          await apiFetchProxies();
          return true;
        }}
        element={<Proxy />}
      />
    </Route>
  )
);

const App = () => {
  return <RouterProvider router={router} />;
};

export default App;
