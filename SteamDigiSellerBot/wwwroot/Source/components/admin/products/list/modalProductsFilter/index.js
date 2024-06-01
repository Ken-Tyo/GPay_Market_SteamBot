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
import MultiSelect from '../../../../shared/multiselect/index'
import MultipleSelectCheckmarks from '../../../../shared/formItem/select/checkmarksSelect';


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
      steamCurrencyId: [{id:5, name:"RUB"}],
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
    const memoCurrencies = React.useMemo(() => currencies, []);
    
      const regions = state.use().steamRegions.map((c) => {
        return {
          id: c.id,
          name: c.name,
        };
      });

      const memoRegions = React.useMemo(() => regions, []);
    
      const handleChange = (prop) => (val, newVal) => {
        if (prop === 'steamCurrencyId') {
          if(val != null){
            //var newVal = val.targer.value;
            //var resultVal = newVal.map(e => currencies.find((c) => c.name === e).id);
            //debugger;
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
    
    
      const regionVal = (
        regions.find((c) => c.id === item.steamCountryCodeId) || {}
      ).name;
      const ThirdPartyPriceTypeVal = item.ThirdPartyPriceType
          ? digiPriceSetType[1].name
          : digiPriceSetType[0].name;
      console.log(item.steamCurrencyId);
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
                    options={memoRegions}
                    onChange={handleChange('steamCountryCodeId')}
                    value={regionVal}
                />
                <div className={css.formItem}>
                    <div className={css.name}>Ценовая основа:</div>

                    <div className={css.wrapper}>
                      <div>
                        <MultipleSelectCheckmarks options={memoCurrencies} value={item.steamCurrencyId} onChange={handleChange('steamCurrencyId')}/>
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