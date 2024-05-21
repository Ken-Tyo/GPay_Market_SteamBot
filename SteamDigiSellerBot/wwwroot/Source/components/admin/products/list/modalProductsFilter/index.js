import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import SymbolTextBox from '../../../../shared/SymbolTextbox';
import css from './styles.scss';

const FromItemText = ({ name, onChange, hint, value, symbol }) => {
    return (
      <div className={css.formItem}>
        <div className={css.name}>{name}</div>
        <div>
          <SymbolTextBox
            hint={hint}
            onChange={onChange}
            defaultValue={value}
            symbol={symbol}
          />
        </div>
      </div>
    );
  };


const ModalFilter = ({ isOpen, value, onCancel, onSave }) => {
    const initial = {
      appId: '',
    };
    const [item, setItem] = useState(initial);
  
    useEffect(() => {
      if (value) {
        let stateVal = {
          ...initial,
          ...value,
        };
  
        if (!stateVal.steamCurrencyId) stateVal.steamCurrencyId = 0;
        if (!stateVal.statusId) stateVal.statusId = 0;
  
        setItem(stateVal);
      }
    }, [value]);

    const handleChange = (prop) => (val) => {
        setItem({ ...item, [prop]: val });
      };
    return (
        <ModalBase
          isOpen={isOpen}
          title={'Фильтры отображения'}
          width={705}
          height={734}
        >
        <div className={css.content}>
                <FromItemText
                    name={'AppID:'}
                    onChange={handleChange('appId')}
                    value={item.appId}
                />
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