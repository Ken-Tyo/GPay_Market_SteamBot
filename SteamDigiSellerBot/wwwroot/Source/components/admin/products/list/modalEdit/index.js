import React, { useState, useEffect, useRef } from 'react';
import Button from '../../../../shared/button';
import DialogActions from '@mui/material/DialogActions';
import ModalBase from '../../../../shared/modalBase';
import TextBox from './textbox';
import Switch from '../../../../shared/switch';
import TextSwitch from './textSwitch';
import Select from './select';
import css from './styles.scss';
import { state } from '../../../../../containers/admin/state';
import CircularLoader from '../../../../shared/circularLoader';

const FromItemText = ({ name, onChange, hint, value, cymbol }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          cymbol={cymbol}
        />
      </div>
    </div>
  );
};

const FromItemSwitch = ({ name, onChange, value }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div
        style={{
          width: '226px',
          height: '51px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Switch value={value} onChange={onChange} />
      </div>
    </div>
  );
};

const FromItemSelect = ({ name, onChange, value, options, hint }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <Select
          options={options}
          defaultValue={value}
          onChange={onChange}
          hint={hint}
        />
      </div>
    </div>
  );
};

const FromItemTextSwitch = ({ name, onChange, value, options }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextSwitch
          defaultValue={value}
          onChange={onChange}
          options={options}
        />
      </div>
    </div>
  );
};

const ModalEdit = ({ isOpen, value, onCancel, onSave }) => {
  const initial = {
    id: null,
    isDlc: false,
    steamCurrencyId: 5,
    isFixedPrice: false,
    isAutoActivation: true,
    steamCountryCodeId: 28,
    currentSteamPriceRub: 0,
    addPrice: 0,
    steamPercent: 0,
  };
  const [item, setItem] = useState(initial);

  const [isCalculating, setIsCalculating] = useState(false);
  const [finalPrice, setFinalPrice] = useState(null);
  const calculationTimer = useRef(null);

  const { digiPriceSetType } = state.use();

  useEffect(() => {
    if (value) {
      let stateVal = {
        ...initial,
        ...value,
        digiSellerIds:
          (value.digiSellerIds && value.digiSellerIds.join(',')) || '',
      };

      // if (!stateVal.isFixedPrice) {
      //   delete stateVal.currentDigiSellerPrice;
      // }

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

  useEffect(() => {
    if (calculationTimer.current) {
      clearTimeout(calculationTimer.current);
    }

    if (['steamPercent', 'addPrice'].some(key => key in item)) {
      setIsCalculating(true);
      setFinalPrice(null);

      calculationTimer.current = setTimeout(() => {
        const { isFixedPrice, steamPercent, currentSteamPriceRub, addPrice } = item;
        if(isFixedPrice){
          return;
        }
        const percent = parseFloat(steamPercent) || 0;
        const additional = parseFloat(addPrice) || 0;
        const currentPriceRub = parseFloat(currentSteamPriceRub) || 0;
        const computedPrice = currentPriceRub * (percent / 100) + additional;
        setIsCalculating(false);
        setFinalPrice(computedPrice.toFixed(2));
      }, 2000);
    }

    return () => {
      if (calculationTimer.current) {
        clearTimeout(calculationTimer.current);
      }
    };
  }, [item.steamPercent, item.addPrice, item.isFixedPrice, item.currentSteamPriceRub]);

  const handleChange = (prop) => (val) => {
    if (prop === 'steamCurrencyId') {
      val = currencies.find((c) => c.name === val).id;
    } else if (prop === 'steamCountryCodeId') {
      val = regions.find((c) => c.name === val).id;
    } else if (prop === 'isFixedPrice') {
      val = val === '₽';
    }
    console.log(prop, val);
    setItem({ ...item, [prop]: val });
  };

  const currencyVal = (
    currencies.find((c) => c.id === item.steamCurrencyId) || {}
  ).name;

  const regionVal = (
    regions.find((c) => c.id === item.steamCountryCodeId) || {}
  ).name;

  const isFixedPriceVal = item.isFixedPrice
    ? digiPriceSetType[1].name
    : digiPriceSetType[0].name;

  return (
    <ModalBase
      isOpen={isOpen}
      title={
        item.id ? 'Редактировать товар Digiseller' : 'Добавить товар Digiseller'
      }
      width={554}
      height={819}
    >
      <div className={css.content}>
        <FromItemTextSwitch
          name={'Тип товара:'}
          onChange={handleChange('isDlc')}
          value={item.isDlc}
          options={['Игра', 'DLC']}
        />
        <FromItemText
          name={'AppID:'}
          onChange={handleChange('appId')}
          value={item.appId}
        />
        <FromItemText
          name={'Издание (SubID):'}
          onChange={handleChange('subId')}
          value={item.subId}
        />
        <FromItemText
          name={'Digiseller ID’s:'}
          hint={'Можно указать несколько позиций, разделять запятой'}
          onChange={handleChange('digiSellerIds')}
          value={item.digiSellerIds}
        />

      <div className={css.formItem}>
          <div className={css.name}>
            {!item.isFixedPrice ? 'Процент от Steam:' : 'Цена Digiseller'}
          </div>
          <div style={{display:'flex', flexDirection:'column', justifyContent:'space-between'}}>
            <div className={css.doubleControl}>
              {!item.isFixedPrice && (
                <TextBox
                  onChange={handleChange('steamPercent')}
                  defaultValue={item.steamPercent}
                  width={157}
                />
              )}
              {item.isFixedPrice && (
                <TextBox
                  onChange={handleChange('fixedDigiSellerPrice')}
                  defaultValue={item.fixedDigiSellerPrice}
                  width={157}
                />
              )}
              <Select
                options={digiPriceSetType}
                defaultValue={isFixedPriceVal}
                onChange={handleChange('isFixedPrice')}
                width={69}
                height={75}
              />
            </div>
            <div>
              {!item.isFixedPrice && (
                  <div className={css.calculatingPrice}>
                    {isCalculating ? (
                      <div>
                        <CircularLoader height={14} width={14} color="#D836E7" />
                      </div>
                    ) : finalPrice !== null ? (
                      <div>~<span style={{color:"#D836E7"}}>{finalPrice}</span> rub</div>
                    ) : null}
                  </div>
                )
              }
            </div>
          </div>
        </div>
        {!item.isFixedPrice && (
          <FromItemText
            name={'Доп. ценовой параметр:'}
            hint={
              'Данный параметр будет прибавляться или убавляться от итоговой расценки товара'
            }
            onChange={handleChange('addPrice')}
            value={item.addPrice}
          />
        )}
        {item.isFixedPrice && (
          <FromItemText
            name={'Мин. порог актуальности:'}
            hint={
              <div>
                Если процентная цена будет ниже данного значения - товар будет
                отключен автоматически
                <div
                  style={{
                    marginTop: '9px',
                    display: 'flex',
                    color: 'white',
                    fontSize: '12px',
                    alignItems: 'center',
                  }}
                >
                  <div>Авто-активация:</div>
                  <div>
                    <Switch
                      value={item.isAutoActivation}
                      onChange={handleChange('isAutoActivation')}
                      style={{ transform: 'scale(0.6)' }}
                    />
                  </div>
                </div>
              </div>
            }
            onChange={handleChange('minActualThreshold')}
            value={item.minActualThreshold}
            cymbol={'%'}
          />
        )}

        <FromItemSelect
          name={'Регион получения:'}
          options={regions}
          onChange={handleChange('steamCountryCodeId')}
          value={regionVal}
        />
        <FromItemSelect
          name={'Ценовая основа:'}
          options={currencies}
          onChange={handleChange('steamCurrencyId')}
          value={currencyVal}
          hint={
            'Валюта, которая будет браться за основу цены товара. Стоимость будет конвертирована и установлена в рублях исходя из установленного курса в настройках'
          }
        />
        {/* <FromItemSwitch
          name={'Игнорировать распродажи:'}
          onChange={handleChange('isDiscount')}
        /> */}
      </div>

      <div className={css.actions}>
        <Button
          text={item.id ? 'Сохранить' : 'Добавить'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '24px',
            width: '271px',
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

export default ModalEdit;
