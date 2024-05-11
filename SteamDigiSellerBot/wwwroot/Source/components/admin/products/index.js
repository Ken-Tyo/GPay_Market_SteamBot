import React, { useEffect, useState, useRef} from 'react';
import List from './list';
import PriceHierarchy from './priceHierarchy';
import PageHeader from '../pageHeader';
import css from './styles.scss';
import { state } from '../../../containers/admin/state';
import { itemsMode as mode } from '../../../containers/admin/common';


var crossFullUnmountScroll = null;

const products = () => {
  const { itemsMode } = state.use();
  const key = "PRODUCTS_CONTENT_SCROLLTOP";
  const contentRef = useRef(null);

  const [scroll, setScroll] = useState(null);
  const [prevItemsMode, setPrevItemsMode] = useState(null);
  const [prevScroll, setPrevScroll] = useState(null);

// useEffect(() =>{
//   console.log(`IN prevItemsMode : ${prevItemsMode} prevScroll : ${prevScroll} itemsMode : ${itemsMode} `);
//   if(prevItemsMode == mode[1] & itemsMode === mode[2]){
//     //scroll = prevScroll;
//     setScroll(prevScroll);
//     // Если из itemsMode:priceHierarchy мы перейдем на другую вкладку, а потом обратно и выйдем в list, это гарантирует что мы вернемся все равно
//     crossFullUnmountScroll = prevScroll;
//   }
//   setPrevItemsMode(itemsMode);
//   setPrevScroll(contentRef?.current?.scrollTop);
// });

// useEffect(() =>{
//   if(itemsMode === mode[1] & prevItemsMode === mode[2]){  
//     console.log(`if scroll ${scroll}`);
//     if(scroll != null){
//       console.log(`scrollTo ${scroll}`);
//       contentRef.current.scroll({
//         top: scroll
//       });
//     }
//     else if(crossFullUnmountScroll != null ){
//        // Если из itemsMode:priceHierarchy мы перейдем на другую вкладку, а потом обратно и выйдем в list, это гарантирует что мы вернемся все равно
//       contentRef.current.scroll({
//         top: crossFullUnmountScroll
//       });
//       crossFullUnmountScroll = null;
//     }
// }},[itemsMode]);

  return (
    
    <div className={css.wrapper}>
      <PageHeader
        title="Digiseller"
        subTitle="Панель управления Digiseller ботом"
      />
      <div className={css.content} id="content-pane" ref={contentRef} >
        <List />
      </div>
      <PriceHierarchy />
    </div>
    
  );
};

export default products;
