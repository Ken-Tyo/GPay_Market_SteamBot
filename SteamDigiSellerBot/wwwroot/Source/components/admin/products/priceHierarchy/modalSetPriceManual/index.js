import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import TextBox from './textbox';
import css from './styles.scss';

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

const ModalSetItemPriceManual = ({ isOpen, value, onCancel, onSave }) => {
  const [val, setVal] = useState('');

  const handleChange = (val) => {
    setVal(val);
  };

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Редактировать цену'}
      width={565}
      height={355}
    >
      <div className={css.content}>
        <FromItemText
          name={value?.currencyName}
          onChange={handleChange}
          value={value?.priceRubRaw?.toFixed(2)}
          cymbol={'₽'}
        />
        <div className={css.hint}>
          После ручного редактирования - цена в данной валюте автоматически
          собираться не будет пока вы ее не обнулите!
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={'Готово'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '36px',
            width: '221px',
          }}
          onClick={() => {
            if (onSave) {
              onSave(val);
            }
            setVal('');
          }}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
            setVal('');
          }}
          style={{
            backgroundColor: '#9A7AA9',
            marginLeft: '0px',
            width: '221px',
          }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalSetItemPriceManual;
