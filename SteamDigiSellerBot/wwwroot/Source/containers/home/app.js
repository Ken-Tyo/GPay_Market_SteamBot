import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider,
} from 'react-router-dom';

import LastOrders from '../../components/home/lastOrders';
import OrderState from '../../components/home/orderState';
import Footer from '../../components/footer';

import { init } from './state';

import css from './styles.scss';
import './i18n';

const Layout = () => {
  return (
    <div className={css.wrapper}>
      <div className={css.bg}/>
      <div className={css.wrapperBackground}>
        <div style={{ flex: '1 1 auto' }}>
          <LastOrders />
        </div>
        <div style={{ flex: '1 1 auto' }}>
          <OrderState />
        </div>
        <Footer/>
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
