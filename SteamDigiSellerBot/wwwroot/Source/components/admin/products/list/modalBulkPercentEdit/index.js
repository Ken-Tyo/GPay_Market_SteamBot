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

const ModalBulkPercentEdit = ({ isOpen, value, onCancel, onSave }) => {
  const [val, setVal] = useState('');

  const handleChange = (val) => {
    setVal(val);
  };

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Массовая смена цен'}
      width={554}
      height={272}
    >
      <div className={css.content}>
        <FromItemText
          name={
            'Введите цену в процентах. Цена будет рассчитываться от цены игр в Steam'
          }
          onChange={handleChange}
          value={val}
          cymbol={'%'}
        />
      </div>

      <div className={css.actions}>
        <Button
          text={'Подтвердить'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '24px',
            width: '271px',
          }}
          onClick={() => {
            onSave(val);
            setVal('');
          }}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
            setVal('');
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalBulkPercentEdit;
