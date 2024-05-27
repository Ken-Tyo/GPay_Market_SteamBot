import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import css from './styles.scss';
import FormItemText from '../../../../shared/formItem/text';
import FormItemSelect from '../../../../shared/formItem/select';
import { state } from '../../../../../containers/admin/state';
import TextBox from '../../../../shared/textbox2';
import Select from '../../../../shared/select';

import OutlinedInput from '@mui/material/OutlinedInput';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import ListItemText from '@mui/material/ListItemText';
import MUISelect from '@mui/material/Select';
import Checkbox from '@mui/material/Checkbox';
import StyledOption from '../../../../shared/select/styledOption'; 


// const ITEM_HEIGHT = 48;
// const ITEM_PADDING_TOP = 8;
// const MenuProps = {
//   PaperProps: {
//     style: {
//       maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
//       width: 250,
//     },
//   },
// };

const ModalFilter = ({ isOpen, value, onCancel, onSave }) => {
    const initial = {
      appId: '',
      productName:"",
      steamCurrencyId: [5],
      steamCountryCodeId: [28],
      digiSellerIds: "",
      ThirdPartyPriceValue: null,
      ThirdPartyPriceType: 0,
    };
    const [item, setItem] = useState(initial);
    const { digiPriceSetType } = state.use();
  
    useEffect(() => {
      if (value) {
        let stateVal = {
          ...initial,
          ...value,
        };
  
        if (!stateVal.statusId) stateVal.statusId = 0;
  
        setItem(stateVal);
      }
    }, [value]);

    const currencies = state.use().currencies.map((c) => {
        return {
          id: c.steamId,
          name: c.code,
        };
      });
    
      const regions = state.use().steamRegions.map((c) => {
        return {
          id: c.id,
          name: c.name,
        };
      });
    
      const handleChange = (prop) => (val) => {
        if (prop === 'steamCurrencyId') {
          if(val != null){
            //var newVal = val.targer.value;
            //var resultVal = newVal.map(e => currencies.find((c) => c.name === e).id);
            val = val.target.value;
        }
        else{
          return;
        }
        } else if (prop === 'steamCountryCodeId') {
          val = regions.find((c) => c.name === val).id;
        }
        else if (prop === 'ThirdPartyPriceType') {
          val = val === '₽';
        }
    
        console.log(prop, val);
    
        setItem({ ...item, [prop]: val });
      };
    
      const currencyVal = (
        item.steamCurrencyId || []
      );
    
      const regionVal = (
        regions.find((c) => c.id === item.steamCountryCodeId) || {}
      ).name;
      const ThirdPartyPriceTypeVal = item.ThirdPartyPriceType
          ? digiPriceSetType[1].name
          : digiPriceSetType[0].name;

    return (
        <ModalBase
          isOpen={isOpen}
          title={'Фильтры отображения'}
          width={705}
          height={734}
        >
        <div className={css.content}>
                <FormItemText
                    name={'AppID:'}
                    onChange={handleChange('appId')}
                    value={item.appId}
                />
                <FormItemText
                    name={'Название товара:'}
                    onChange={handleChange('productName')}
                    value={item.itemName}
                />
                <FormItemSelect
                    name={'Регион получения:'}
                    options={regions}
                    onChange={handleChange('steamCountryCodeId')}
                    value={regionVal}
                />
                <div className={css.formItem}>
                    <div className={css.name}>Ценовая основа:</div>

                    <div className={css.wrapper}>
                      <div>
                        <FormControl className={css.formItem} sx={{ m: 1, width: 300 }}>
                          <Select
                            sx={{ '& .MuiOption-root ': { padding: 0 } }} 
                            multiple={true}
                            value={item.steamCurrencyId}
                            onChange={handleChange('steamCurrencyId')}
                            input={<OutlinedInput label="Tag" />}
                            renderValue={(selected) => selected.map(e => e.name).join(", ")}
                            // MenuProps={MenuProps}
                            options={currencies}
                            customRenderChild = {(curr) => 
                                              <StyledOption style={{display: "flex", flexDirection: "row"}} key={curr.id} value={curr.name}>
                                                    <Checkbox style={{display: "block", maxHeight:"14px"}} disablePadding size="small" className={css.paddingZero} sx={{ '& .MuiSvgIcon-root': { padding: 0, fontSize:"1em" } }}  checked={item.steamCurrencyId.indexOf(curr.id) > -1}>
                                                      
                                                    </Checkbox>
                                                    <span style={{display: "block", maxHeight:"14px"}}>{curr.name}</span>
                                                </StyledOption>}
                                                    // <MenuItem disablePadding className={css.paddingZero} key={curr.id} value={curr}>
                                                    //   <Checkbox disablePadding size="small" className={css.paddingZero} sx={{ '& .MuiSvgIcon-root': { padding: 0 } }}  checked={item.steamCurrencyId.indexOf(curr.id) > -1} />
                                                    //   <StyledOption key={curr.id} value={curr.name}>
                                                    //       {curr.name}
                                                    //   </StyledOption>
                                                    // </MenuItem>}
                          />
                        </FormControl>
                    </div>
                  </div>
                  </div>

                <FormItemText
                    name={'DigisellerIDs:'}
                    onChange={handleChange('digiSellerIds')}
                    value={item.digiSellerId}
                />
                
                  <div className={css.formItem}>
                    <div className={css.name}>
                      {!item.ThirdPartyPriceType ? 'Процент от Steam:' : 'Цена Digiseller'}
                    </div>
                    <div>
                      <div className={css.wrapper}>
                      <div className={css.doubleControl}>
                          <TextBox
                            onChange={handleChange('ThirdPartyPriceValue')}
                            defaultValue={item.ThirdPartyPriceValue}
                            width={157}
                          />
                        <Select
                          options={digiPriceSetType}
                          defaultValue={ThirdPartyPriceTypeVal}
                          onChange={handleChange('ThirdPartyPriceType')}
                          width={69}
                          height={75}
                        />
                      </div>
                  </div>
              </div>
            </div>
        </div>
        <div className={css.actions}>
        <Button
          text={'Отобразить'}
          style={{
            backgroundColor: '#A348CE',
            marginRight: '24px',
            width: '322px',
          }}
          onClick={() => {
            onSave(item);
          }}
        />
        <Button
          text={'Отмена'}
          onClick={async () => {
            if (onCancel) onCancel();
            //await setItem(initial);
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
        </ModalBase>
    );
};

export default ModalFilter;