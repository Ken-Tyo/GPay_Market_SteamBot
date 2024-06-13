import React, { useState, useEffect } from 'react';
import Button from '../../../shared/button';
import ModalBase from '../../../shared/modalBase';
import TextBox from './textbox';
import TextSwitch from './textSwitch';
import Select from './select';
import css from './styles.scss';
import { state } from '../../../../containers/admin/state';
import Textarea from "./textarea";

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

const FromItemTextarea = ({ name, onChange, value, placeholder }) => {
  return (
      <div className={css.formItem}>
        <div className={css.name}>{name}</div>
        <div>
          <Textarea
              onChange={onChange}
              defaultValue={value}
              placeholder={placeholder}
          />
        </div>
      </div>
  );
};

const ModalEdit = ({
  isOpen,
  //value,
  onCancel,
  onSave,
  response,
  resetResponse,
}) => {
  const initial = {
    id: null,
    isDlc: false,
    steamCurrencyId: 5,
    steamCountryCodeId: 28,
  };
  const [item, setItem] = useState(initial);

  // useEffect(() => {
  //   if (value) {
  //     let stateVal = {
  //       ...initial,
  //       ...value,
  //     };

  //     setItem(stateVal);
  //   }
  // }, [value]);

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
      val = currencies.find((c) => c.name === val).id;
    } else if (prop === 'steamCountryCodeId') {
      val = regions.find((c) => c.name === val).id;
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

  return (
    <ModalBase
      isOpen={isOpen}
      title={'Добавить новый заказ'}
      width={554}
      height={819}
      isLoading={response.loading}
    >
      {!response.loading && response.errors.length === 0 && (
        <>
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
              name={'Время на получение игры:'}
              symbol={'дней'}
              hint={'Необязательный параметр'}
              onChange={handleChange('daysExpiration')}
              value={item.daysExpiration}
            />

            <FromItemText
              name={'Отключение заказа при повышении цены'}
              symbol={'%'}
              hint={
                'Максимально допустимое превышение в изменении ценности игры. В случае, если игра станет дороже введенного % - заказ будет отключен автоматически'
              }
              onChange={handleChange('maxSellPercent')}
              value={item.maxSellPercent}
            />

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
                'Основа будет браться для отправки игры в выбранном регионе'
              }
            />

            <FromItemText
              name={'Количество копий:'}
              onChange={handleChange('copyCount')}
              value={item.copyCount}
              symbol={'шт'}
            />

            <FromItemTextarea
                name={'Комментарий:'}
                onChange={handleChange('comment')}
                value={item.comment}
                placeholder={'поиск по конкретному комментарию у сессии'}
            />
          </div>

          <div className={css.actions}>
            <Button
              text={'Добавить'}
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
                setItem(initial);
              }}
              style={{ backgroundColor: '#9A7AA9', marginLeft: '0px' }}
            />
          </div>
        </>
      )}
      {!response.loading && response.errors.length > 0 && (
        <>
          <div className={css.errors}>
            <div className={css.title}>
              <div>Ошибка при добавлении!</div>
            </div>
            <div className={css.list}>
              {response.errors.map((e) => {
                return <div className={css.item}>- {e}</div>;
              })}
            </div>
          </div>
          <div className={css.actions}>
            <Button
              text={'Закрыть'}
              onClick={() => {
                if (resetResponse) resetResponse();
              }}
              style={{
                backgroundColor: '#9A7AA9',
                marginLeft: '0px',
                width: '226px',
              }}
            />
          </div>
        </>
      )}
    </ModalBase>
  );
};

export default ModalEdit;
