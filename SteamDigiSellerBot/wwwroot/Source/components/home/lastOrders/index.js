import React from 'react';
import css from './styles.scss';
import { state } from '../../../containers/home/state';
import { useTranslation } from 'react-i18next';

const lastOrders = () => {
  const { lastOrders } = state.use();

  return (
    <div className={css.wrapper}>
      <div className={css.itemsWrap}>
        <div aria-hidden="true" className={css.items + ' ' + css.marquee}>
          {lastOrders.map((i) => {
            return <Item {...i} />;
          })}
        </div>
        <div aria-hidden="true" className={css.items + ' ' + css.marquee}>
          {lastOrders.map((i) => {
            return <Item {...i} />;
          })}
        </div>
      </div>
    </div>
  );
};

const Item = ({ userName, gameName, price }) => {
  const { t, i18n } = useTranslation('lastOrders');

  return (
    <div className={css.item}>
      <span>
        <span>
          {userName} {t('bought')}{' '}
        </span>
        <span className={css.gameName}> {gameName} </span>
        {t('for')} <span className={css.price}> {price.toFixed(0)} ₽</span>
      </span>
    </div>
  );
};

export default lastOrders;
