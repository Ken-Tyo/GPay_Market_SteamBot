import React, { useEffect, useState } from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';

const products = () => {
  const { itemsMode } = state.use();
  const listRef = React.createRef();

  var prevItemMode = itemsMode;
  useEffect(() =>{
    console.log("wrapper lists " + listRef?.current?.scrollTop);
    listRef?.current?.addEventListener("scroll", event => {
      console.log(`scrollTop: ${listRef?.current?.scrollTop} <br>
                          scrollLeft: ${listRef?.current?.scrollLeft} `);
  }, { passive: true });
    if(prevItemMode === mode[2] & itemsMode === mode[1] & localStorage.getItem("scrollTop")){
      // скроллим к сохраненным координатам
      listRef.current.scroll({
        top: localStorage.getItem("scrollTop"),
        behavior: "smooth"
      });
      // удаляем данные с localStorage
      localStorage.removeItem("scrollTop");
    }
    prevItemMode = itemsMode;
    return () => {
      if(prevItemMode === mode[1] & itemsMode === mode[2]){
        localStorage.setItem("scrollTop", listRef.current.scrollTop);
        console.log("OUT" + localStorage.getItem("scrollTop")); 
      }
      prevItemMode = itemsMode;
  }},[itemsMode]);

  return (
    
    <div className={css.wrapper} ref={listRef} preventScrollReset={true}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      
      <div className={css.content} preventScrollReset={true}>
        {itemsMode === mode[1] && <List preventScrollReset={true} />}
        {itemsMode === mode[2] && <PriceHierarchy preventScrollReset={true} />}
      </div>
    </div>
  );
};

export default products;
