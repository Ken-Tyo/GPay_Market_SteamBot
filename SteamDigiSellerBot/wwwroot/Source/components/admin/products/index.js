import React from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';


const products = () => {
  return (
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <div className={css.content} >
        <List />
      </div>
      <PriceHierarchy />
    </div>
  );
};

export default products;
