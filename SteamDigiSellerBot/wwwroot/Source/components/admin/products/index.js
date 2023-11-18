import React from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';
const products = () => {
  const { itemsMode } = state.use();

  return (
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />

      <div className={css.content}>
        {itemsMode === mode[1] && <List />}
        {itemsMode === mode[2] && <PriceHierarchy />}
      </div>
    </div>
  );
};

export default products;
