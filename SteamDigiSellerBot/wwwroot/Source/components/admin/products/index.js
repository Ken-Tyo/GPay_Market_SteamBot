import React from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import ModalFilter from './list/modalProductsFilter';
import {
  state,
  toggleFilterProductsModal,
  //updateProductsFilter,
} from '../../../containers/admin/state';
const products = () => {

  const { productsFilter, filterProductsModalIsOpen } = state.use();
  return (
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <ModalFilter
          isOpen={filterProductsModalIsOpen}
          value={productsFilter}
          onCancel={() => {
            toggleFilterProductsModal(false);
          }}
      />
      <div className={css.content} >
        <List />
      </div>
      <PriceHierarchy />
    </div>
  );
};

export default products;
