import React, { useState, useEffect } from 'react';
import Button from '../../../../shared/button';
import ModalBase from '../../../../shared/modalBase';
import css from './styles.scss';
import Select from '../modalEdit/select'
import { state } from '../../../../../containers/admin/state';

const FromItemSelect = ({ name, onChange, value, options, hint }) => {
    return (
        <div className={css.formItem}>
            <div className={css.name} style={{ marginRight: '70px' }}>{name}</div>
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

const ModalBulkPriceBasisEdit = ({ isOpen, onCancel, onSave, selectedCount }) => {
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

const title = (
    <div className={css.custTitle}>
        <div className={css.t1}>Редактировать ценовую основу</div>
        <div className={css.t2}>Выбрано {selectedCount} товаров для редакитрования</div>
    </div>);

  return (
      <ModalBase
        isOpen={isOpen}
          title={title}
        marginTop={'10px'}
        letterSpacing={'0.04em'}
        width={600}
        height={450}
      >
       <div className={css.content}>
           <div className={css.formItem}>
              <FromItemSelect
                  name={"Ценовая основа:"}
                  options={currencies}
                  onChange={handleChange('steamCurrencyId')}
                  value={currencyVal}
                  hint={'Валюта, которая будет браться за основу цены товара. Стоимость будет конвертирована и установлена в рублях исходя из установленного курса в настройках.'}
              />
           </div>
      </div>
      <div className={css.actions}>
        <Button
          text={'Сохранить'}
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
