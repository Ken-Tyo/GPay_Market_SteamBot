import React, { useEffect, useState, useRef} from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';


var scroll = null;
var prevCheck = null;
var prevScroll = null;
const products = () => {
  const { itemsMode } = state.use();
  const key = "PRODUCTS_CONTENT_SCROLLTOP";
  const contentRef = useRef(null);
  
//   useEffect(() =>{
//     var scrollTopValue = contentRef.current.scrollTop;
//     console.log(`prevMode : ${prevMode} itemMode : ${itemsMode}`);
//     if(prevMode === mode[2] & itemsMode === mode[1]){
      
//       var storageScrollTop = localStorage.getItem(key);
//       console.log(`localStorage.getItem : ${storageScrollTop}`);
//       if(itemsMode === mode[1] & storageScrollTop != null ){
//         // скроллим к сохраненным координатам
//         if(storageScrollTop != 0){
//           console.log(`scrollTo : ${storageScrollTop}`);
//           contentRef.current.scroll({
//             top: storageScrollTop
//           });
//         }
//           // удаляем данные с localStorage
//           localStorage.removeItem(key);
//       }
//     }
//     if(prevMode === mode[1] & itemsMode === mode[2]){
//       console.log(`localStorage.setItem : ${scrollTopValue}`);
//       localStorage.setItem(key, scrollTopValue);
//     }
//     prevMode = itemsMode;
// });
useEffect(() =>{
  console.log(`IN prevCheck : ${prevCheck} prevScroll : ${prevScroll} itemsMode : ${itemsMode} `);
  if(prevCheck == mode[1] & itemsMode === mode[2]){
    scroll = prevScroll;
  }
  prevCheck = itemsMode;
  prevScroll = contentRef?.current?.scrollTop;
});

useEffect(() =>{
  const currentMode = state.get()["itemsMode"];
  if(itemsMode === mode[1]){  
    console.log(`if scroll ${scroll}`);
    if(scroll != null){
      console.log(`scrollTo ${scroll}`);
      contentRef.current.scroll({
        top: scroll
      });
    }
}},[itemsMode]);

  return (
    
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <div className={css.content} id="content-pane" ref={contentRef} >
        {itemsMode === mode[1] && <List />}
        {itemsMode === mode[2] && <PriceHierarchy />}
      </div>
    </div>
    
  );
};

export default products;
