import React from 'react';
import List from './list';
import PageHeader from '../pageHeader';
import css from './styles.scss';

const products = () => {
  return (
    <div className={css.wrapper}>
      <PageHeader title="Прокси" subTitle="Просмотр общего списка прокси" />

      <div className={css.content}>
        <List />
      </div>
    </div>
  );
};

export default products;
