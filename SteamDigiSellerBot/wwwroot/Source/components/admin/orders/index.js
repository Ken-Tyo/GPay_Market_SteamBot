import React from 'react';
import List from './list';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import ListFilter from './modalFilter';
import {
  state,
  toggleFilterOrdersModal,
  updateGameSessionsFilter,
} from '../../../containers/admin/state';

const orders = () => {
  const { gameSessionsFilter, filterOrdersModalIsOpen } = state.use();

  return (
    <div className={css.wrapper}>
      <PageHeader title="Заказы" subTitle="Просмотр заказов Digiseller" />

      <div className={css.content}>
        <List />
        <ListFilter
          isOpen={filterOrdersModalIsOpen}
          value={gameSessionsFilter}
          onCancel={() => {
            toggleFilterOrdersModal(false);
          }}
          onSave={(val) => {
            updateGameSessionsFilter({ ...val, page: 1 });
            toggleFilterOrdersModal(false);
          }}
        />
      </div>
    </div>
  );
};

export default orders;
