import React from 'react';
import PageHeader from '../pageHeader';
import Grid from './grid';
import css from './styles.scss';

const bots = () => {
  return (
    <div className={css.wrapper}>
      <PageHeader title="Боты" subTitle="Просмотр списка ботов" />

      <div className={css.content}>
        <Grid />
      </div>
    </div>
  );
};

export default bots;
