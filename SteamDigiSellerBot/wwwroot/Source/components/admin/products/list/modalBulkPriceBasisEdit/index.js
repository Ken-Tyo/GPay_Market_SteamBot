import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import css from './styles.scss';
import Select from '../modalEdit/select'
import { state } from '../../../../../containers/admin/state';

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

const ModalBulkPriceBasisEdit = ({ isOpen, onCancel, onSave }) => {
  const [steamCurrencyId, setSteamCurrencyId] = useState(5);

    const currencies = state.use().currencies.map((c) => {
        return {
            id: c.steamId,
            name: c.code,
        };
    });

    const handleChange = (prop) => (val) => {
        if (prop === 'steamCurrencyId') {
            val = currencies.find((c) => c.name === val).id;
        }
        setSteamCurrencyId(val);
    };

    const currencyVal = (
        currencies.find((c) => c.id === steamCurrencyId) || {}
    ).name;

  return (
      <ModalBase
        isOpen={isOpen}
        title={'Смена ценовой основы'}
        marginTop={'10px'}
        letterSpacing={'0.04em'}
        width={600}
        height={450}
      >
      <div className={css.content}>
          <FromItemSelect
              name={'Ценовая основа:'}
              options={currencies}
              onChange={handleChange('steamCurrencyId')}
              value={currencyVal}
              hint={'Основа будет браться для отправки игры в выбранном регионе'}
          />
      </div>
      <div className={css.actions}>
        <Button
          text={'Подтвердить'}
          style={{
            backgroundColor: '#478C35',
            marginRight: '24px',
          }}
          onClick={() => {
            onSave(steamCurrencyId);
          }}
          width={'290px'}
          height={'70px'}
        />
        <Button
          text={'Отмена'}
          onClick={() => {
            if (onCancel) onCancel();
          }}
          style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
          width={'200px'}
          height={'70px'}
        />
      </div>
    </ModalBase>
  );
};

export default ModalBulkPriceBasisEdit;
