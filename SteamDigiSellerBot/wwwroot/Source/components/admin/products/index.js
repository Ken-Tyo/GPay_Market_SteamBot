import React, { useEffect, useState, useRef } from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';
import { ScrollRestoration , Link } from "react-router-dom";

const products = () => {
  const { itemsMode } = state.use();

  const contentRef = useRef(null);

  useEffect(() =>{
    var ara = contentRef.current.scrollTop;
    console.log("IN itemsmode " + itemsMode + " scrollTop " + localStorage.getItem("scrollTop") + " " + (contentRef.current != null))
    if(itemsMode === mode[1] & localStorage.getItem("scrollTop") != null){
      // скроллим к сохраненным координатам
      console.log("scrollTo " + localStorage.getItem("scrollTop"));
      contentRef.current.scroll({
        top: localStorage.getItem("scrollTop")
      });
      // удаляем данные с localStorage
      localStorage.removeItem("scrollTop");
    }

    return () => {
      console.log("OUT itemsMode " + itemsMode + " " + (ara != null));
      if(localStorage.getItem("scrollTop") == null & itemsMode === mode[1]){
        console.log("setItem scrollTop : " +  ara);
        localStorage.setItem("scrollTop", ara);
      }
  }});

  return (
    
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <Link onClick={() => {
        console.log("contentRef.scrollTop : " + contentRef.current.scrollTop);
      }}>Scrolll</Link>
      <div className={css.content} ref={contentRef} >
        {itemsMode === mode[1] && <List />}
        {itemsMode === mode[2] && <PriceHierarchy />}
      </div>
      <ScrollRestoration
        getKey={(location, matches) => {
          // default behavior
          console.log("ScrollRestoration");
          console.log(location);
          return location.key;
        }}
      />
    </div>
  );
};

export default products;
