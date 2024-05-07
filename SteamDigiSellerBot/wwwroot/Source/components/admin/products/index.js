import React, { useEffect, useState, useRef } from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';


const products = () => {
  const { itemsMode } = state.use();
  const key = "PRODUCTS_CONTENT_SCROLLTOP";
  const contentRef = useRef(null);

  useEffect(() =>{
    var scrollTopValue = contentRef.current.scrollTop;
    if(itemsMode === mode[1] & localStorage.getItem(key) != null){
      // скроллим к сохраненным координатам
      contentRef.current.scroll({
        top: localStorage.getItem(key)
      });
      // удаляем данные с localStorage
      localStorage.removeItem(key);
    }

    return () => {
      if(localStorage.getItem(key) == null & itemsMode === mode[1]){
        localStorage.setItem(key, scrollTopValue);
      }
  }});

  return (
    
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <div className={css.content} ref={contentRef} >
        {itemsMode === mode[1] && <List />}
        {itemsMode === mode[2] && <PriceHierarchy />}
      </div>

    </div>
  );
};

export default products;
