import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
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

const ModalExchangeRates = ({ isOpen, value, onCancel, onSave }) => {
  const [val, setVal] = useState({ id: null, currencies: [] });

  const handleChange = (newCurVal, code) => {
    //console.log(newCurVal, code);
    let clonedArray = JSON.parse(JSON.stringify(val.currencies));

    let cur = clonedArray.find((c) => c.code === code);
    //let idx = clonedArray.indexOf(cur);
    cur.value = +newCurVal;

    let newObj = {
      ...val,
      currencies: clonedArray,
    };

    //console.log('newObj', newObj);
    setVal(newObj);
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  const title = (
    <div className={css.custTitle}>
      <div className={css.t1}>Установить курсы валют Steam</div>
      <div className={css.t2}>Указывать валюту к Доллару</div>
    </div>
  );
  return (
    <ModalBase isOpen={isOpen} title={title} width={554} height={667}>
      <div className={css.content}>
        {val &&
          val.currencies &&
          val.currencies.map((c) => {
            return (
              <FromItemText
                name={`${c.name} (${c.code})`}
                onChange={(v) => {
                  handleChange(v, c.code);
                }}
                value={c.value.toFixed(3)}
              />
            );
          })}
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
            //setVal([]);
          }}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
            //setVal([]);
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
        />
      </div>
    </ModalBase>
  );
};

export default ModalExchangeRates;
