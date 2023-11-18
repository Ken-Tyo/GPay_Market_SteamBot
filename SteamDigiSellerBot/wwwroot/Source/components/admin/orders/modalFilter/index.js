import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from './textbox';
import Select from '../../../shared/select';
import css from './styles.scss';
import { state } from '../../../../containers/admin/state';

const FromItemText = ({ name, onChange, hint, value, symbol }) => {
  return (
    <div className={css.formItem}>
      <div className={css.name}>{name}</div>
      <div>
        <TextBox
          hint={hint}
          onChange={onChange}
          defaultValue={value}
          symbol={symbol}
        />
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
          width={302}
        />
      </div>
    </div>
  );
};

const ModalFilter = ({ isOpen, value, onCancel, onSave }) => {
  const initial = {
    appId: '',
    gameName: '',
    steamCurrencyId: 0,
    statusId: 0,
    uniqueCodes: '',
    profileStr: '',
    orderId: null,
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

  let currencies = state.use().currencies.map((c) => {
    return {
      id: c.steamId,
      name: c.code,
    };
  });

  currencies = [{ id: 0, name: 'Все' }, ...currencies];

  const gameSessionsStatuses = state.use().gameSessionsStatuses;
  let statuses = [];
  for (let prop in gameSessionsStatuses) {
    statuses.push({
      id: gameSessionsStatuses[prop].statusId,
      name: gameSessionsStatuses[prop].name,
      color: gameSessionsStatuses[prop].color,
    });
  }
  statuses = [{ id: 0, name: 'Все' }, ...statuses];

  const handleChange = (prop) => (val) => {
    if (prop === 'steamCurrencyId') {
      val = currencies.find((c) => c.name === val).id;
    }

    if (prop === 'statusId') {
      val = statuses.find((c) => c.name === val).id;
    }
    console.log(item, val);

    setItem({ ...item, [prop]: val });
  };

  const currencyVal = (
    currencies.find((c) => c.id === item.steamCurrencyId) || {}
  ).name;

  const statusVal = (statuses.find((c) => c.id === item.statusId) || {}).name;

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

        <FromItemText
          name={'Название игры:'}
          onChange={handleChange('gameName')}
          value={item.gameName}
        />

        <FromItemSelect
          name={'Регион получения:'}
          options={currencies}
          onChange={handleChange('steamCurrencyId')}
          value={currencyVal}
        />

        <FromItemText
          name={'Профиль получателя:'}
          onChange={handleChange('profileStr')}
          value={item.profileStr}
        />

        <FromItemText
          name={'ID заказа:'}
          onChange={handleChange('orderId')}
          value={item.orderId}
        />

        <FromItemText
          name={'Уникальный код:'}
          onChange={handleChange('uniqueCodes')}
          value={item.uniqueCodes}
        />

        <FromItemSelect
          name={'Состояние заказа:'}
          options={statuses}
          onChange={handleChange('statusId')}
          value={statusVal}
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
